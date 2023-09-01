using System.Threading;
using System.Threading.Tasks;
using Zs.Bot.Services.Messaging;
using Zs.Common.Models;

namespace Zs.Bot.Services.Pipeline;

public abstract class PipelineStep
{
    public PipelineStep? Next { internal get; set; }

    protected abstract Task<Result> PerformInternalAsync(MessageActionData messageActionData, CancellationToken cancellationToken);

    public async Task PerformAsync(MessageActionData messageActionData, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var stepResult = await PerformInternalAsync(messageActionData, cancellationToken);

        if (Next != null && stepResult.Successful)
            await Next.PerformAsync(messageActionData, cancellationToken);
    }
}