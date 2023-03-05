using System.Threading.Tasks;
using Moq;
using Xunit;
using Zs.Bot.Data.Abstractions;
using Zs.Bot.Data.Factories;
using Zs.Bot.Services.Commands;
using Zs.Common.Abstractions;
using Zs.Common.Services.Shell;

namespace UnitTests;

public sealed class CommandManagerTests
{
    [Theory]
    [InlineData("/correctCommand parameter1 prameter2, parameter3;  parameter4")]
    [InlineData("/correctCommand parameter1  \"para meter 2\"  ")]
    [InlineData("/correctCommand   \"\"  ")]
    public async Task TryEnqueueCommandAsync_ReturnsTrue(string messageText)
    {
        var commandManager = CreateDefaultCommandManager();
        var userMessage = EntityFactory.NewMessage(messageText);

        var enqueueResult = await commandManager.TryEnqueueCommandAsync(userMessage);

        Assert.True(enqueueResult);
    }

    private static CommandManager CreateDefaultCommandManager()
    {
        return new CommandManager(
            Mock.Of<ICommandsRepository>(),
            Mock.Of<IUserRolesRepository>(),
            Mock.Of<IUsersRepository>(),
            Mock.Of<IDbClient>(),
            "bashPath...",
            "powershellPath/...");
    }

    [Theory]
    [InlineData("noCommand parameter1 prameter2, parameter3; \"\"  parameter4")]
    [InlineData("noCommand")]
    [InlineData("")]
    [InlineData(null)]
    public async Task TryEnqueueCommandAsync_ReturnsFalse(string messageText)
    {
        var commandManager = CreateDefaultCommandManager();
        var message = EntityFactory.NewMessage(messageText);

        var enqueueResult = await commandManager.TryEnqueueCommandAsync(message);

        Assert.False(enqueueResult);
    }
}