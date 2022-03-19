﻿namespace Zs.Bot.Services.Commands
{
    /// <summary>Contains command execution result</summary>
    public sealed class CommandResult
    {
        public int ChatIdForAnswer { get; private set; }

        /// <summary> Text result </summary>
        public string Text { get; private set; }

        public CommandResult(int chatIdForAnswer, string result)
        {
            ChatIdForAnswer = chatIdForAnswer;
            Text = result;
        }
    }
}
