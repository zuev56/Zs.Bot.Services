using System.Threading;
using System.Threading.Tasks;

namespace Zs.Bot.Services.Commands;

public interface ICommandManager
{
    Task<string> ExecuteCommandAsync(string command, CancellationToken cancellationToken = default);
}