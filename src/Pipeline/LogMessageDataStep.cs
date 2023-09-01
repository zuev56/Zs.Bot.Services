using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zs.Bot.Services.Messaging;
using Zs.Common.Extensions;
using Zs.Common.Models;

namespace Zs.Bot.Services.Pipeline;

public sealed class LogMessageStep : PipelineStep
{
    private readonly ILogger _logger;

    public LogMessageStep(ILogger logger)
    {
        _logger = logger;
    }

    protected override Task<Result> PerformInternalAsync(MessageActionData messageActionData, CancellationToken cancellationToken)
    {
        _logger.LogInformationIfNeed($"Action: {messageActionData.Action}, Message: {messageActionData.Message?.RawData}");
        return Task.FromResult(Result.Success());
    }
}