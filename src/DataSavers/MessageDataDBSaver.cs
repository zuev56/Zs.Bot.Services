using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Zs.Bot.Data.Abstractions;
using Zs.Bot.Services.Messaging;

namespace Zs.Bot.Services.DataSavers
{
    /// <summary>
    /// Saves message data to a database (repositories)
    /// </summary>
    public sealed class MessageDataDBSaver : IMessageDataSaver
    {
        private readonly ILogger<MessageDataDBSaver> _logger;
        private readonly IChatsRepository _chatsRepo;
        private readonly IUsersRepository _usersRepo;
        private readonly IMessagesRepository _messagesRepo;


        public MessageDataDBSaver(
            IChatsRepository chatsRepo,
            IUsersRepository usersRepo,
            IMessagesRepository messagesRepo,
            ILogger<MessageDataDBSaver> logger = null)
        {
            try
            {
                _chatsRepo = chatsRepo;
                _usersRepo = usersRepo;
                _messagesRepo = messagesRepo;
                _logger = logger;
            }
            catch (Exception ex)
            {
                var tex = new TypeInitializationException(typeof(MessageDataDBSaver).FullName, ex);
                _logger?.LogError(tex, $"{nameof(MessageDataDBSaver)} initialization error");
            }
        }

        public async Task SaveNewMessageData(MessageActionEventArgs args)
        {
            try
            {
                if (args.User != null)
                {
                    await _usersRepo.SaveAsync(args.User).ConfigureAwait(false);
                    args.Message.UserId = args.User.Id;
                }

                await _chatsRepo.SaveAsync(args.Chat).ConfigureAwait(false);
                args.Message.ChatId = args.Chat.Id;
                
                if (args.Message.Text == null)
                    args.Message.Text = "Empty/service message";
                
                await _messagesRepo.SaveAsync(args.Message).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ex.Data.Add("User", args?.User);
                ex.Data.Add("Chat", args?.Chat);
                ex.Data.Add("Message", args?.Message);
                _logger?.LogError(ex, "New message data saving error: \"{Message}\"", args?.Message.RawData);
            }
        }

        public async Task EditSavedMessage(MessageActionEventArgs args)
        {
            if (args.Message.Id != default)
            {
                if (args.Message.Text == null)
                    args.Message.Text = "[Empty]";

                await _messagesRepo.SaveAsync(args.Message).ConfigureAwait(false);
            }
            else
                _logger?.LogWarning("The edited message is not found in the database", args);
        }

    }
}
