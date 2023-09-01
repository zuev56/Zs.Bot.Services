using Microsoft.Extensions.DependencyInjection;
using Zs.Bot.Services.Commands;
using Zs.Common.Abstractions;

namespace Zs.Bot.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommandManager(this IServiceCollection services, string cliPath)
    {
        var serviceProvider = services.BuildServiceProvider();
        var dbClient = serviceProvider.GetRequiredService<IDbClient>();
        var commandManager = new CommandManager(dbClient, cliPath);

        return services.AddSingleton<ICommandManager>(commandManager);
    }
}