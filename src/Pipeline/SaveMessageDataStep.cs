using System.Threading;
using System.Threading.Tasks;
using Zs.Bot.Services.Messaging;
using Zs.Bot.Services.Storages;
using Zs.Common.Models;

namespace Zs.Bot.Services.Pipeline;

public sealed class SaveMessageDataStep : PipelineStep
{
    private readonly IMessageDataStorage _messageDataSaver;

    public SaveMessageDataStep(IMessageDataStorage messageDataSaver)
    {
        _messageDataSaver = messageDataSaver;
    }

    protected override async Task<Result> PerformInternalAsync(MessageActionData messageActionData, CancellationToken cancellationToken)
    {
        await _messageDataSaver.SaveNewMessageDataAsync(messageActionData, cancellationToken);
        return Result.Success();
    }
}