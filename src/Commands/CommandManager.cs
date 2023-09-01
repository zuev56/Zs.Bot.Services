using System;
using System.Threading;
using System.Threading.Tasks;
using Zs.Common.Abstractions;
using Zs.Common.Services.Shell;

namespace Zs.Bot.Services.Commands;

public sealed class CommandManager : ICommandManager
{
    private readonly IDbClient _dbClient;
    private readonly string? _cliPath;

    public CommandManager(IDbClient dbClient, string? cliPath = null)
    {
        _cliPath = cliPath;
        _dbClient = dbClient;
    }

    public async Task<string> ExecuteCommandAsync(string commandText, CancellationToken cancellationToken)
    {
        var botCommand = BotCommand.Parse(commandText);

        var executeCommandTask = botCommand.Type switch
        {
            CommandType.Default => ExecuteDefaultCommandAsync(botCommand, cancellationToken),
            CommandType.Cli => ExecuteShellCommandAsync(botCommand, cancellationToken),
            CommandType.Sql => ExecuteSqlCommandAsync(botCommand, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(botCommand.Type))
        };
        var commandResult = await executeCommandTask;

        return commandResult;
    }

    private async Task<string> ExecuteDefaultCommandAsync(BotCommand botCommand, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private async Task<string> ExecuteShellCommandAsync(BotCommand botCommand, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_cliPath))
            return "CLI commands not supported";

        var cliCommand = botCommand.RawParameters;
        if (string.IsNullOrWhiteSpace(cliCommand))
            return "No command passed to CLI";

        var commandResult = await ShellLauncher.RunAsync(_cliPath, cliCommand, cancellationToken).ConfigureAwait(false);

        return commandResult.Value;
    }

    private async Task<string> ExecuteSqlCommandAsync(BotCommand botCommand, CancellationToken cancellationToken)
    {
        var sql = botCommand.RawParameters.Trim('"');
        var commandResult = await _dbClient.GetQueryResultAsync(sql, cancellationToken);

        return commandResult ?? string.Empty;
    }
}