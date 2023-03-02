using System;
using System.Collections.Generic;
using System.Linq;
using Zs.Bot.Data.Models;
using Zs.Bot.Services.Exceptions;

namespace Zs.Bot.Services.Commands;

public sealed class BotCommand
{
    public int FromUserId { get; private set; }
    public int ChatIdForAnswer { get; private init; }
    public string Name { get; private init; } = null!;
    public string NameWithoutSlash => Name[1..];
    public string? TargetBotName { get; private init; }
    public List<object> Parameters { get; } = new ();
    public bool IsKnown { get; set; }

    private BotCommand()
    {
    }

    /// <summary> Create <see cref="BotCommand"/> from <see cref="Message"/> </summary>
    public static BotCommand GetCommandFromMessage(Message message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (message.Text == null || !IsCommand(message.Text))
        {
            throw new ArgumentException("The message is not a command for a bot");
        }

        if (message.Text.Count(static c => c == '"') % 2 != 0)
        {
            var oddNumberOfQuotesException = new OddNumberOfQuotesException();
            oddNumberOfQuotesException.Data.Add("Message", message);
            throw oddNumberOfQuotesException;
        }

        var messageWords = MessageSplitter(message.Text);
        var commandName = messageWords[0].Replace("_", "\\_").Trim().ToLower();
        var targetBotName = default(string);

        if (commandName.Contains('@'))
        {
            var commandNameWithTargetBotName = commandName;
            var indexOfAt = commandNameWithTargetBotName.IndexOf('@');

            commandName = commandNameWithTargetBotName[..indexOfAt];
            targetBotName = commandNameWithTargetBotName[(indexOfAt + 1)..];
        }

        messageWords.RemoveAt(0);

        var botCommand = new BotCommand
        {
            Name = commandName,
            TargetBotName = targetBotName,
            FromUserId = message.UserId,
            ChatIdForAnswer = message.ChatId,
            IsKnown = false
        };

        var parameters = messageWords.Cast<object>();
        botCommand.Parameters.AddRange(parameters);
        return botCommand;
    }

    /// <summary> Check if message is <see cref="BotCommand"/> </summary>
    public static bool IsCommand(string message, string? botName = null)
    {
        ArgumentNullException.ThrowIfNull(message);

        var text = message.Trim();
        var isCommand = text.Length > 1 && text[0] == '/' && char.IsLetterOrDigit(text[1]);

        if (isCommand && !string.IsNullOrWhiteSpace(botName) && message.Contains('@'))
        {
            var messageWithoutCommand = message[(message.IndexOf('@') + 1)..];
            var botNameEndIndex = messageWithoutCommand.Contains(' ')
                ? messageWithoutCommand.IndexOf(' ')
                : message.Length - message.IndexOf('@') - 1;
            var destinationBotName = messageWithoutCommand[..botNameEndIndex];

            return botName == destinationBotName;
        }

        return isCommand;
    }

    private static List<string> MessageSplitter(string argumentsLine)
    {
        // Example: /cmd arg1 "arg 2", arg3;"arg4"

        // Сначала выделяем аргументы в кавычках в отдельную группу <индекс, значение>
        // а на их место вставляем заглушку в формате <<индекс>>
        var quotedArgs = new Dictionary<int, string>();

        var begIndex = -1;
        for (var i = 0; i < argumentsLine.Length; i++)
        {
            switch (argumentsLine[i])
            {
                // Начало составного аргумента
                case '"' when begIndex == -1:
                    begIndex = i;
                    break;
                // Дошли до конца составного аргумента
                case '"' when begIndex > -1:
                    // Добавляем значение в список
                    quotedArgs.Add(quotedArgs.Count, argumentsLine.Substring(begIndex + 1, i - begIndex - 1));

                    // Заменяем значение в строке аргументов на индекс
                    argumentsLine = argumentsLine.Remove(begIndex, i - begIndex + 1);
                    argumentsLine = argumentsLine.Insert(begIndex, $" <[<{quotedArgs.Count - 1}>]> ");

                    i = begIndex;
                    begIndex = -1;
                    break;
            }
        }

        // Обрабатываем строку с аргументами, будто там нет составных значений
        var words = argumentsLine.Replace(',', ' ')
            .Replace(';', ' ').Trim()
            .Split(' ').ToList();

        words.RemoveAll(static w => w.Trim() == "");

        // Убираем лишние символы из простых аргументов
        words.ForEach(w => w = w.Replace(",", "")
            .Replace(";", "")
            .Replace("-", "")
            .Replace("=", "")
            .Trim());

        // Заменяем в массиве индексы на их значения
        for (var i = 0; i < words.Count; i++)
        {
            // Получаем временный индекс
            if (!words[i].Contains("<[<") || !words[i].Contains(">]>"))
            {
                continue;
            }

            var mapIndex = int.Parse(words[i].Replace("<[<", "").Replace(">]>", ""));

            // Присваиваем значение, соответствующее этому индексу
            words[i] = quotedArgs[mapIndex];
        }

        return words;
    }

    public override string ToString() => Name;
}