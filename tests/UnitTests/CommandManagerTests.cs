using Moq;
using System.Threading.Tasks;
using Xunit;
using Zs.Bot.Data.Abstractions;
using Zs.Bot.Data.Factories;
using Zs.Bot.Data.Models;
using Zs.Bot.Services.Commands;
using Zs.Common.Abstractions;
using Zs.Common.Services.Abstractions;

namespace Zs.Bot.UnitTests
{
    public class CommandManagerTests
    {
        [Theory]
        [InlineData("/correctCommand parameter1 prameter2, parameter3;  parameter4")]
        [InlineData("/correctCommand parameter1  \"para meter 2\"  ")]
        [InlineData("/correctCommand   \"\"  ")]
        public async Task TryEnqueueCommandAsync_ReturnsTrue(string messageText)
        {
            // Arrange
            var commandManager = CreateDefaultCommandManager();
            var userMessage = EntityFactory.NewMessage(messageText);

            // Act
            var enqueueResult = await commandManager.TryEnqueueCommandAsync(userMessage);

            // Assert
            Assert.True(enqueueResult);
        }

        private static CommandManager CreateDefaultCommandManager()
        {
            return new CommandManager(
                Mock.Of<ICommandsRepository>(),
                Mock.Of<IUserRolesRepository>(),
                Mock.Of<IUsersRepository>(),
                Mock.Of<IDbClient>(),
                Mock.Of<IShellLauncher>());
        }

        [Theory]
        [InlineData("noCommand parameter1 prameter2, parameter3; \"\"  parameter4")]
        [InlineData("noCommand")]
        [InlineData("")]
        [InlineData(null)]
        public async Task TryEnqueueCommandAsync_ReturnsFalse(string messageText)
        {
            // Arrange
            var commandManager = CreateDefaultCommandManager();
            var message = EntityFactory.NewMessage(messageText);

            // Act
            var enqueueResult = await commandManager.TryEnqueueCommandAsync(message);

            // Assert
            Assert.False(enqueueResult);
        }
    }
}