using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Zs.Bot.Data.Enums;
using Zs.Bot.Data.Models;

namespace Zs.Bot.Services.Messaging;

public interface IMessenger
{
    /// <summary>Occurs when message is sent</summary>
    public event EventHandler<MessageActionEventArgs>? MessageSent;

    /// <summary>Occurs when message is edited</summary>
    public event EventHandler<MessageActionEventArgs>? MessageEdited;

    /// <summary>Occurs when message is received</summary>
    public event EventHandler<MessageActionEventArgs>? MessageReceived;

    /// <summary>Occurs when message is deleted</summary>
    public event EventHandler<MessageActionEventArgs>? MessageDeleted;

    /// <summary>Add a text message to the queue for sending</summary>
    bool AddMessageToOutbox(Chat chat, string messageText, Message? messageToReply = null);

    /// <summary>Add a text message to the queue for sending</summary>
    Task<bool> AddMessageToOutboxAsync(string messageText, params Role[] userRoles);

    /// <summary>Delete message from chat</summary>
    Task<bool> DeleteMessageAsync(Message message);

    Task<JsonElement> GetBotInfoAsync(CancellationToken cancellationToken = default);
}