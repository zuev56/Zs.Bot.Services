using System;

namespace Zs.Bot.Services.Commands;

public static class StringExtensions
{
    /// <summary> Check if message text is <see cref="BotCommand"/> </summary>
    public static bool IsBotCommand(this string? messageText, string? botName = null)
    {
        if (messageText is null)
            return false;

        messageText = messageText.Trim();
        var isCommand = messageText.Length > 1 && messageText[0] == '/' && (messageText[1].IsLetterOrDigit() || messageText[1] == '_');

        if (isCommand && !string.IsNullOrWhiteSpace(botName) && messageText.Contains('@'))
        {
            var destinationBotName = BotCommand.ParseBotName(messageText);
            return string.Equals(destinationBotName, botName, StringComparison.OrdinalIgnoreCase);
        }

        return isCommand;
    }

    private static bool IsLetterOrDigit(this char c) => char.IsLetterOrDigit(c);
}