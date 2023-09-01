using System.Threading;
using System.Threading.Tasks;
using Zs.Bot.Data.Models;
using Zs.Bot.Services.Messaging;

namespace Zs.Bot.Services.Storages;

public interface IMessageDataStorage
{
    // TODO: DeleteMessage() // logically in DB
    Task SaveNewMessageDataAsync(MessageActionData messageActionData, CancellationToken cancellationToken);
    Task EditSavedMessageAsync(Message message, CancellationToken cancellationToken);
}