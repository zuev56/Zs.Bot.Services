using System;
using System.Collections.Generic;
using System.Linq;
using Zs.Bot.Data.Models;
using Zs.Bot.Services.Exceptions;

namespace Zs.Bot.Services.Commands
{
    /// <summary> A command for a bot </summary>
    public sealed class BotCommand
    {
        public int FromUserId { get; private set; }
        public int ChatIdForAnswer { get; private set; }
        public string Name { get; private set; }
        public string NameWithoutSlash => Name.Substring(1);
        public string TargetBotName { get; private set; }
        public List<object> Parametres { get; set; }
        public bool IsKnown { get; set; }

        private BotCommand()
        {
        }

        /// <summary> Create <see cref="BotCommand"/> from <see cref="Message"/> </summary>
        public static BotCommand GetCommandFromMessage(Message message)
        {
            try
            {
                if (message is null)
                    throw new ArgumentNullException(nameof(message));

                if (IsCommand(message.Text))
                {
                    if (message.Text.Count(c => c == '"') % 2 != 0)
                    {
                        var aex = new OddNumberOfQuotesException();
                        aex.Data.Add("Message", message);
                        throw aex;
                    }

                    var messageWords = MessageSplitter(message.Text);

                    string commandName = messageWords[0].Replace("_", "\\_").Trim();
                    string targetBotName = null;

                    if (commandName.Contains('@'))
                    {
                        var commandNameWithTargetBotName = commandName;
                        int indexOfAt = commandNameWithTargetBotName.IndexOf('@');

                        commandName = commandNameWithTargetBotName.Substring(0, indexOfAt);
                        targetBotName = commandNameWithTargetBotName.Substring(indexOfAt + 1);
                    }

                    messageWords.RemoveAt(0);

                    return new BotCommand()
                    {
                        Name = commandName.ToLower(),
                        TargetBotName = targetBotName,
                        FromUserId = message.UserId,
                        ChatIdForAnswer = message.ChatId,
                        Parametres = messageWords.Cast<object>().ToList(),
                        IsKnown = false
                    };
                }
                else
                    throw new ArgumentException("Сообщение не является командой для бота");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary> Check if message is <see cref="BotCommand"/> </summary>
        public static bool IsCommand(string message, string botName = null)
        {
            var text = message?.Trim();
            var isCommand = text?.Length > 1
                         && text[0] == '/'
                         && char.IsLetterOrDigit(text[1]);

            if (isCommand && !string.IsNullOrWhiteSpace(botName) && message.Contains('@'))
            {
                var messageWithoutCommand = message.Substring(message.IndexOf('@') + 1);
                var destinationBotName = messageWithoutCommand
                    .Substring(0, messageWithoutCommand.Contains(' ')
                                ? messageWithoutCommand.IndexOf(' ')
                                : message.Length - message.IndexOf('@') - 1);

                return botName == destinationBotName;
            }

            return isCommand;
        }

        private static List<string> MessageSplitter(string argumentsLine)
        {
            // TODO: need refactoring

            // Example: /cmd arg1 "arg 2", arg3;"arg4"

            // Сначала выделяем аргументы в кавычках в отдельную группу <индекс, значение>
            // а на их место вставляем заглушку в формате <<индекс>>
            var quotedArgs = new Dictionary<int, string>();

            int begIndex = -1;
            for (int i = 0; i < argumentsLine.Length; i++)
            {
                // Начало составного аргумента
                if (argumentsLine[i] == '"' && begIndex == -1)
                {
                    begIndex = i;
                }
                // Дошли до конца составного аргумента
                else if (argumentsLine[i] == '"' && begIndex > -1)
                {
                    // Добавляем значение в список 
                    quotedArgs.Add(quotedArgs.Count, argumentsLine.Substring(begIndex + 1, i - begIndex - 1));

                    // Заменяем значение в строке аргументов на индекс
                    argumentsLine = argumentsLine.Remove(begIndex, i - begIndex + 1);
                    argumentsLine = argumentsLine.Insert(begIndex, $" <[<{quotedArgs.Count - 1}>]> ");

                    i = begIndex;
                    begIndex = -1;
                }
            }

            // Обрабатываем строку с аргументами, будто там нет составных значений
            var words = argumentsLine.Replace(',', ' ')
                                     .Replace(';', ' ').Trim()
                                     .Split(' ').ToList();

            words.RemoveAll(w => w.Trim() == "");

            // Убираем лишние символы из простых аргументов
            words.ForEach(w => w = w.Replace(",", "")
                                    .Replace(";", "")
                                    .Replace("-", "")
                                    .Replace("=", "")
                                    .Trim());


            // Заменяем в массиве индексы на их значения
            for (int i = 0; i < words.Count; i++)
            {
                // Получаем временный индекс
                int mapIndex = -1;
                if (words[i].Contains("<[<") && words[i].Contains(">]>"))
                {
                    mapIndex = int.Parse(words[i].Replace("<[<", "").Replace(">]>", ""));

                    // Присваиваем значение, соответствующее этому индексу
                    words[i] = quotedArgs[mapIndex];
                }
            }

            return words;
        }

        public override string ToString() => Name;
    }
}
