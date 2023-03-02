using System;
using Zs.Bot.Data.Enums;
using Zs.Bot.Data.Models;

namespace Zs.Bot.Services.Messaging;

public sealed class MessageActionEventArgs : EventArgs
{
    public required Message Message { get; init; }
    public required Chat Chat { get; init; }
    public required User User { get; init; }
    public Data.Enums.ChatType ChatType { get; init; }
    public MessageAction Action { get; init; }
    public bool IsHandled { get; set; }
}