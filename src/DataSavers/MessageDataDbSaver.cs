using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Zs.Bot.Data.Abstractions;
using Zs.Bot.Services.Messaging;

namespace Zs.Bot.Services.DataSavers;

/// <summary>
/// Saves message data to a database (repositories)
/// </summary>
public sealed class MessageDataDbSaver : IMessageDataSaver
{
    private readonly IChatsRepository _chatsRepo;
    private readonly IUsersRepository _usersRepo;
    private readonly IMessagesRepository _messagesRepo;
    private readonly ILogger<MessageDataDbSaver>? _logger;


    public MessageDataDbSaver(
        IChatsRepository chatsRepo,
        IUsersRepository usersRepo,
        IMessagesRepository messagesRepo,
        ILogger<MessageDataDbSaver>? logger = null)
    {
        _chatsRepo = chatsRepo;
        _usersRepo = usersRepo;
        _messagesRepo = messagesRepo;
        _logger = logger;
    }

    public async Task SaveNewMessageData(MessageActionEventArgs args)
    {
        try
        {
            await _usersRepo.SaveAsync(args.User).ConfigureAwait(false);
            args.Message.UserId = args.User.Id;

            await _chatsRepo.SaveAsync(args.Chat).ConfigureAwait(false);
            args.Message.ChatId = args.Chat.Id;

            args.Message.Text ??= "Empty/service message";

            await _messagesRepo.SaveAsync(args.Message).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ex.Data.Add("User", args.User);
            ex.Data.Add("Chat", args.Chat);
            ex.Data.Add("Message", args.Message);
            _logger?.LogError(ex, "New message data saving error: \"{Message}\"", args?.Message.RawData);
        }
    }

    public async Task EditSavedMessage(MessageActionEventArgs args)
    {
        if (args.Message.Id != default)
        {
            args.Message.Text ??= "[Empty]";
            await _messagesRepo.SaveAsync(args.Message).ConfigureAwait(false);
        }
        else
        {
            _logger?.LogWarning("The edited message is not found in the database");
        }
    }
}