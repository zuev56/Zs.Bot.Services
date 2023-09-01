using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zs.Bot.Data.Models;
using Zs.Bot.Data.Repositories;
using Zs.Bot.Services.Messaging;
using Zs.Common.Extensions;

namespace Zs.Bot.Services.Storages;

public sealed class MessageDataDbStorage : IMessageDataStorage
{
    private readonly IChatsRepository _chatsRepo;
    private readonly IUsersRepository _usersRepo;
    private readonly IMessagesRepository _messagesRepo;
    private readonly ILogger? _logger;


    public MessageDataDbStorage(
        IChatsRepository chatsRepo,
        IUsersRepository usersRepo,
        IMessagesRepository messagesRepo,
        ILogger<MessageDataDbStorage>? logger = null)
    {
        _chatsRepo = chatsRepo;
        _usersRepo = usersRepo;
        _messagesRepo = messagesRepo;
        _logger = logger;
    }

    public async Task SaveNewMessageDataAsync(MessageActionData messageActionData, CancellationToken cancellationToken)
    {
        var chat = messageActionData.Chat;
        var user = messageActionData.User;
        var message = messageActionData.Message;
        try
        {
            await SaveUserAsync(user, cancellationToken);
            await SaveChatAsync(chat, cancellationToken);
            await SaveMessageAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            ex.Data.Add("User", user);
            ex.Data.Add("Chat", chat);
            ex.Data.Add("Message", message);
            _logger?.LogErrorIfNeed(ex, "New message data saving error: \"{Message}\"", message?.RawData);
        }
    }

    private async Task SaveUserAsync(User? user, CancellationToken cancellationToken)
    {
        if (user != null)
        {
            var userExists = await _usersRepo.ExistsAsync(user.Id, cancellationToken);
            if (userExists)
            {
                var name = user.FullName ?? user.UserName;
                _logger?.LogTraceIfNeed("The user '{User}' already exists. Saving canceled", name);
                return;
            }

            var saved = await _usersRepo.AddAsync(user, cancellationToken).ConfigureAwait(false);
            if (_logger?.IsEnabled(LogLevel.Trace) == true)
            {
                var name = user.FullName ?? user.UserName;
                var result = saved == 1 ? "saved" : "not saved";
                _logger.LogTraceIfNeed("User '{User}' Id = {Id} {Result}", name, user.Id, result);
            }
        }
    }

    private async Task SaveChatAsync(Chat? chat, CancellationToken cancellationToken)
    {
        if (chat != null)
        {
            var chatExists = await _chatsRepo.ExistsAsync(chat.Id, cancellationToken);
            if (chatExists)
            {
                _logger?.LogTraceIfNeed("The chat '{Chat}' already exists. Saving canceled", chat.Name);
                return;
            }

            var saved = await _chatsRepo.AddAsync(chat, cancellationToken).ConfigureAwait(false);
            if (_logger?.IsEnabled(LogLevel.Trace) == true)
            {
                var result = saved == 1 ? "saved" : "not saved";
                _logger.LogTraceIfNeed("Chat '{Chat}' Id = {Id} {Result}", chat.Name, chat.Id, result);
            }
        }
    }

    private async Task SaveMessageAsync(Message? message, CancellationToken cancellationToken)
    {
        if (message != null)
        {
            var saved = await _messagesRepo.AddAsync(message, cancellationToken).ConfigureAwait(false);
            if (_logger?.IsEnabled(LogLevel.Trace) == true)
                _logger.LogTraceIfNeed("Message Id = {Id} {Result}", message.Id, saved);
        }
    }

    public async Task EditSavedMessageAsync(Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);

        var exists = await _messagesRepo.ExistsAsync(message.Id, cancellationToken);
        if (!exists)
        {
            _logger?.LogWarningIfNeed("The edited message was not found in the database. RawMessage = {RawMessage}", message.RawData);
            return;
        }

        var saved = await _messagesRepo.AddAsync(message, cancellationToken).ConfigureAwait(false);

        if (_logger?.IsEnabled(LogLevel.Trace) == true)
        {
            var result = saved == 1 ? "updated" : "not updated";
            _logger?.LogTraceIfNeed("Message Id = {Id} {Result}", message.Id, result);
        }
    }
}