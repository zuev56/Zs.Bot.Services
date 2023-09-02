using System.Threading;
using System.Threading.Tasks;
using Zs.Bot.Data.Models;
using Zs.Bot.Services.Pipeline;

namespace Zs.Bot.Services.Messaging;

public interface IBotClient
{
    void AddToMessagePipeline(PipelineStep nextStep, CancellationToken cancellationToken = default);
    Task<Message> SendMessageAsync(string text, Chat chat, Message? messageToReply = null, CancellationToken cancellationToken = default);
    Task DeleteMessageAsync(Message message, CancellationToken cancellationToken = default);
    Task<string> GetBotInfoAsync(CancellationToken cancellationToken = default);
}