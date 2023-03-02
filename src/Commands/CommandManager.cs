using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Zs.Bot.Data.Abstractions;
using Zs.Bot.Data.Models;
using Zs.Common.Abstractions;
using Zs.Common.Enums;
using Zs.Common.Exceptions;
using Zs.Common.Extensions;
using Zs.Common.Models;
using Zs.Common.Services.Shell;

namespace Zs.Bot.Services.Commands;

/// <summary>
/// Handles commands
/// </summary>
public sealed class CommandManager : ICommandManager
{
    private readonly ICommandsRepository _commandsRepo;
    private readonly IUserRolesRepository _userRolesRepo;
    private readonly IUsersRepository _usersRepo;
    private readonly IDbClient _dbClient;
    private readonly IShellLauncher _shellLauncher;
    private readonly Buffer<BotCommand> _commandBuffer;
    private readonly ILogger? _logger;

    public event EventHandler<CommandResult>? CommandCompleted;


    public CommandManager(
        ICommandsRepository commandsRepo,
        IUserRolesRepository userRolesRepo,
        IUsersRepository usersRepo,
        IDbClient dbClient,
        IShellLauncher shellLauncher,
        ILogger? logger = null)
    {
        _commandsRepo = commandsRepo;
        _userRolesRepo = userRolesRepo;
        _usersRepo = usersRepo;
        _dbClient = dbClient;
        _shellLauncher = shellLauncher;
        _logger = logger;

        _commandBuffer = new Buffer<BotCommand>();
        _commandBuffer.OnEnqueue += CommandBuffer_OnEnqueue;
    }

    /// <summary>If the <see cref="Message"/> is a <see cref="BotCommand"/>, then queue it to process, otherwise returns false</summary>
    /// <param name="message"></param>
    /// <returns>Command execution result</returns>
    public async Task<bool> TryEnqueueCommandAsync(Message message)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(message);

            if (!BotCommand.IsCommand(message.Text))
            {
                return false;
            }

            var botCommand = BotCommand.GetCommandFromMessage(message);
            botCommand.IsKnown = await CommandIsKnown(botCommand).ConfigureAwait(false);

            if (botCommand.Parameters.Count == 0)
            {
                await SetDefaultParametersIfExist(botCommand).ConfigureAwait(false);
            }

            EnqueueCommand(botCommand);

            return true;
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Command enqueuing error");
            return false;
        }
    }

    private async Task<bool> CommandIsKnown(BotCommand botCommand)
    {
        if (Enum.IsDefined(typeof(Shell), botCommand.NameWithoutSlash.FirstCharToUpper()))
        {
            return true;
        }

        var dbCommand = await _commandsRepo.FindWhereIdLikeValueAsync(botCommand.Name).ConfigureAwait(false);
        return dbCommand != null;
    }

    private async Task SetDefaultParametersIfExist(BotCommand botCommand)
    {
        var dbCommand = await _commandsRepo.FindWhereIdLikeValueAsync(botCommand.Name).ConfigureAwait(false);

        if (dbCommand?.DefaultArgs != null)
        {
            var parameters = dbCommand.DefaultArgs?.Split(';').Cast<object>() ?? Enumerable.Empty<object>();
            botCommand.Parameters.AddRange(parameters);
        }
    }

    /// <summary>
    /// Execute command in OS
    /// </summary>
    /// <param name="shell"></param>
    /// <param name="botCommand"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Execution result</returns>
    private async Task<string> RunSystemCommandAsync(Shell shell, BotCommand botCommand, CancellationToken cancellationToken = default)
    {
        // TODO: Remove code duplicates with RunSqlCommandAsync

        Result<string> cmdExecResult;

        var dbUser = await _usersRepo.FindByIdAsync(botCommand.FromUserId, cancellationToken).ConfigureAwait(false);
        if (dbUser is null)
        {
            throw new ItemNotFoundException($"User with Id = {botCommand.FromUserId} not found");
        }

        var userPermissions = await GetPermissionsArrayAsync(dbUser.UserRoleId).ConfigureAwait(false);
        if (userPermissions.Any(static p => p.ToUpperInvariant() == "ALL")) // Only for OWNER
        {
            try
            {
                var shellCommand = string.Join(" ", botCommand.Parameters);
                if (string.IsNullOrWhiteSpace(shellCommand))
                {
                    return $"No command passed to {shell} in parameters";
                }

                cmdExecResult = shell switch
                {
                    Shell.Bash => await _shellLauncher.RunBashAsync(shellCommand, cancellationToken).ConfigureAwait(false),
                    Shell.Pwsh => await _shellLauncher.RunPowerShellAsync(shellCommand, cancellationToken).ConfigureAwait(false),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            catch (Exception ex)
            {
                ex.Data.Add("BotCommand", botCommand);
                _logger?.LogError(ex, $"System command execution error: {botCommand}");

                cmdExecResult = Result.Fail<string>(Fault.Unknown.SetMessage("System command execution: general error"));
            }
        }
        else
        {
            cmdExecResult = Result.Fail<string>(Fault.Unknown.SetMessage("You have no rights to execute this command"));
            _logger?.LogWarning($"{dbUser.Name} has no rights to execute {botCommand}");
        }

        return cmdExecResult.Value;
    }

    /// <summary>
    /// Execute command in database
    /// </summary>
    /// <param name="botCommand"></param>
    /// <returns>Execution result</returns>
    internal async Task<string> RunSqlCommandAsync(BotCommand botCommand)
    {
        string cmdExecResult;
        try
        {
            var dbCommand = await _commandsRepo.FindByIdAsync(botCommand.Name).ConfigureAwait(false);
            if (dbCommand != null)
            {
                // (i) SQL-запросы могут быть любые, не только функции.
                // (i) Должны содержать параметры типа object, иначе будут проблемы при форматировании строки {0}

                var dbUser = await _usersRepo.FindByIdAsync(botCommand.FromUserId).ConfigureAwait(false);
                if (dbUser is null)
                    throw new ItemNotFoundException($"User with Id = {botCommand.FromUserId} not found");

                var userHasRights = (await GetPermissionsArrayAsync(dbUser.UserRoleId).ConfigureAwait(false))
                    .Any(p => p.ToUpperInvariant() == "ALL"
                              || string.Equals(p, dbCommand.Group, StringComparison.InvariantCultureIgnoreCase));

                if (userHasRights)
                {
                    // Т.о. исключаются проблемы с форматированием строки
                    var sqlCommandStr = dbCommand.Script;
                    var parameters = await ProcessParametersAsync(botCommand).ConfigureAwait(false);

                    var queryWithParams = string.Format(sqlCommandStr, parameters);

                    // TODO: Определить спец. синтаксис для дефолтных(и не только) параметров команды,
                    //       который будет расшифровываться в этом блоке и обрабатываться определённым образом
                    //
                    //       ProcessSpecifiсParametres(...)

                    try
                    {
                        cmdExecResult = await _dbClient.GetQueryResultAsync(queryWithParams).ConfigureAwait(false) ?? "NULL";
                    }
                    catch (DbException pEx)
                    {
                        cmdExecResult = "Command execution: query processing error";
                        pEx.Data.Add("BotCommand", new { botCommand.Name, parameters = string.Join(", ", botCommand.Parameters) });
                        _logger?.LogError(pEx, $"Command execution sql error: {botCommand} (Parameters: {string.Join(", ", botCommand.Parameters)})");
                    }
                    catch (Exception ex)
                    {
                        ex.Data.Add("BotCommand", botCommand);
                        _logger?.LogError(ex, $"Command execution error: {botCommand}");

                        cmdExecResult = ex.Message == "Column is null"
                            ? "NULL"
                            : "Command execution: general error";
                    }
                }
                else
                {
                    cmdExecResult = "You have no rights to execute this command";
                    _logger?.LogWarning($"{dbUser.Name} has no rights to execute {botCommand}");
                }
            }
            else
                throw new ArgumentException($"Command '{botCommand.Name}' not found");
        }
        catch (Exception ex)
        {
            ex.Data.Add("BotCommand", botCommand);
            _logger?.LogError(ex, "Command execution error");
            return $"Command '{botCommand.Name}' execution failed!";
        }

        return cmdExecResult.Trim();
    }

    private void CommandBuffer_OnEnqueue(object sender, BotCommand item)
    {
        Task.Run(ProcessCommandQueueAsync);
    }

    private void EnqueueCommand(BotCommand command)
    {
        _logger?.LogDebug($"Command received: {command} (Parameters: {string.Join(", ", command.Parameters)})", command, nameof(CommandManager));
        _commandBuffer.Enqueue(command);
    }

    /// <summary>Changes generic parameters to theirs specific values</summary>
    /// <param name="parameters">An array, that can contain generic parametres</param>
    /// <returns>Specific parameters array</returns>
    private async Task<object[]> ProcessParametersAsync(BotCommand botCommand)
    {
        var regex = new Regex(@"<([^\s>]+)\>", RegexOptions.IgnoreCase);
        var genericParams = botCommand.Parameters.Cast<string>().Where(p => regex.IsMatch(p));
        var concreteParams = new Dictionary<string, string>(genericParams.Count());

        foreach (var parameter in genericParams)
        {
            switch (parameter.ToUpperInvariant())
            {
                case "<USERROLEID>":
                    var user = await _usersRepo.FindByIdAsync(botCommand.FromUserId).ConfigureAwait(false);
                    concreteParams.Add(parameter, $"'{user?.UserRoleId}'");
                    break;
                default:
                    concreteParams.Add(parameter, null);
                    break;
            }
        }

        foreach (var cp in concreteParams)
        {
            var index = botCommand.Parameters.IndexOf(cp.Key);
            if (index >= 0)
            {
                botCommand.Parameters[index] = cp.Value;
            }
        }

        return botCommand.Parameters.ToArray();
    }

    private async Task ProcessCommandQueueAsync()
    {
        string logCmdName = null;
        try
        {
            while (_commandBuffer.TryDequeue(out var command))
            {
                logCmdName = command.Name;

                var result = Enum.TryParse(command.NameWithoutSlash.FirstCharToUpper(), out Shell shell)
                    ? await RunSystemCommandAsync(shell, command).ConfigureAwait(false)
                    : await RunSqlCommandAsync(command).ConfigureAwait(false);

                const int maxResultLength = 4000; // max message length for Telegram is 4096
                if (result.Length < maxResultLength)
                {
                    CommandCompleted?.Invoke(this, new CommandResult(command.ChatIdForAnswer, result));
                }
                else
                {
                    foreach (var part in result.SplitIntoParts(maxResultLength))
                        CommandCompleted?.Invoke(this, new CommandResult(command.ChatIdForAnswer, part));
                }
            }
        }
        catch (Exception ex)
        {
            ex.Data.Add("Command", logCmdName);
            _logger?.LogError(ex, "Commands queue processing error");
        }
    }

    private async Task<string[]> GetPermissionsArrayAsync(string userRoleId)
    {
        var role = await _userRolesRepo.FindByIdAsync(userRoleId).ConfigureAwait(false);

        return role != null ? JsonSerializer.Deserialize<string[]>(role.Permissions)! : Array.Empty<string>();
    }
}