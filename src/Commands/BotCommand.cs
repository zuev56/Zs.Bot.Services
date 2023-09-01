using System;
using System.Collections.Generic;
using System.Linq;

namespace Zs.Bot.Services.Commands;

internal sealed class BotCommand
{
    public string? BotName { get; private init; }
    public string Command { get; private init; } = null!;
    public CommandType Type { get; private init; }
    public string RawParameters { get; private init; } = null!;
    public IReadOnlyList<string> Parameters { get; private init; } = new List<string>();

    public static BotCommand Parse(string commandText)
    {
        ArgumentException.ThrowIfNullOrEmpty(commandText);

        commandText = commandText.Trim();
        if (!commandText.IsBotCommand())
            throw new ArgumentException($"{nameof(commandText)} is not a BotCommand");

        var command = ParseCommand(commandText);
        var botName = ParseBotName(commandText);
        var type = GetCommandType(command);
        var rawParameters = GetRawParameters(commandText);
        var parameters = ParseParametersToList(rawParameters);

        return new BotCommand
        {
            Command = command,
            BotName = botName,
            Type = type,
            RawParameters = rawParameters,
            Parameters = parameters
        };
    }

    private static string GetRawParameters(string commandText)
    {
        var indexOfParameters = commandText.IndexOf(' ') + 1;
        return indexOfParameters == 0
            ? string.Empty
            : commandText[indexOfParameters..];
    }

    private static string ParseCommand(string commandText)
    {
        var indexofAt = commandText.Contains('@') ? commandText.IndexOf('@') : commandText.Length;
        var indexOfSpace = commandText.Contains(' ') ? commandText.IndexOf(' '): commandText.Length;
        var endOfCommand = Math.Min(indexofAt, indexOfSpace);

        return commandText[..endOfCommand].ToLowerInvariant();
    }

    internal static string? ParseBotName(string commandText)
    {
        var indexofAt = commandText.IndexOf('@');
        var indexOfSpace = commandText.Contains(' ') ? commandText.IndexOf(' ') : commandText.Length;;

        if (indexofAt < 2 || indexofAt > indexOfSpace)
            return null;

        return commandText[(indexofAt+1)..indexOfSpace].ToLowerInvariant();
    }

    private static CommandType GetCommandType(string command)
    {
        var type = command switch
        {
            "/sql" => CommandType.Sql,
            "/cli" => CommandType.Cli,
            _ => CommandType.Default
        };

        return type;
    }

    private static IReadOnlyList<string> ParseParametersToList(string commandText)
    {
        // TODO: need refactoring
        // Example: /cmd arg1 "arg 2", arg3;"arg4"

        var quotedArgs = new Dictionary<int, string>();
        var begIndex = -1;
        for (var i = 0; i < commandText.Length; i++)
        {
            switch (commandText[i])
            {
                case '"' when begIndex == -1:
                    begIndex = i;
                    break;
                case '"' when begIndex > -1:
                    var quotedArg = commandText.Substring(begIndex + 1, i - begIndex - 1);
                    quotedArgs.Add(quotedArgs.Count, quotedArg);
                    commandText = commandText.Remove(begIndex, i - begIndex + 1);
                    commandText = commandText.Insert(begIndex, $" <[<{quotedArgs.Count - 1}>]> ");
                    i = begIndex;
                    begIndex = -1;
                    break;
            }
        }

        var args = commandText
            .Replace(',', ' ')
            .Replace(';', ' ')
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        for (var i = 0; i < args.Count; i++)
        {
            if (!args[i].StartsWith("<[<") || !args[i].EndsWith(">]>"))
                continue;

            var quotedArgIndexStr = args[i].Replace("<[<", "").Replace(">]>", "");
            var quotedArgIndex = int.Parse(quotedArgIndexStr);
            args[i] = quotedArgs[quotedArgIndex];
        }

        return args;
    }
}