using System;
using Zs.Bot.Data.Enums;
using Zs.Bot.Data.Models;

namespace Zs.Bot.Services.Messaging
{
    /// <summary>
    /// Набор необходимых свойств, которые надо передавать при получении, отправке или удалении сообщения
    /// </summary>
    public sealed class MessageActionEventArgs : EventArgs
    {
        public Message Message { get; init; }
        public Chat Chat { get; init; }
        public User User { get; init; }
        public Data.Enums.ChatType ChatType { get; init; }
        public MessageAction Action { get; init; }
        public bool IsHandled { get; set; }
    }
}
