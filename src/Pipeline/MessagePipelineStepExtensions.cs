using Zs.Common.Exceptions;

namespace Zs.Bot.Services.Pipeline;

public static class PipelineStepExtensions
{
    public static PipelineStep GetLastStep(this PipelineStep step)
    {
        var maxIterationsCount = 1000;
        while (--maxIterationsCount != 0)
        {
            if (step.Next == null)
                return step;

            step = step.Next;
        }

        throw new FaultException("Unable to find last step");
    }
}