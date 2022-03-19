using System.Threading.Tasks;
using Xunit;
using Zs.Bot.Data.Factories;
using Zs.Bot.Services.Commands;

namespace Zs.Bot.UnitTests
{
    public class BotCommandTests
    {
        [Fact]
        public void GetCommandFromMessage_ReturnsCommand()
        {
            // Arrange
            var commandName = "/correctCommand";
            var parameter5 = "para meter 5";
            int dbUserId = 8;
            int dbChatId = 7;

            var message = EntityFactory.NewMessage(
                $"{commandName} parameter1 prameter2, parameter3;  parameter4 \"{parameter5}\"");

            message.UserId = dbUserId;
            message.ChatId = dbChatId;


            // Act
            var command = BotCommand.GetCommandFromMessage(message);

            // Assert
            Assert.NotNull(command);
            Assert.Equal(commandName.ToLowerInvariant(), command.Name);
            Assert.Equal(5, command.Parametres?.Count);
            Assert.Equal(parameter5, command.Parametres[4]);
            Assert.Null(command.TargetBotName);
            Assert.Equal(dbUserId, command.FromUserId);
            Assert.Equal(dbChatId, command.ChatIdForAnswer);
            Assert.False(command.IsKnown);
        }

        [Fact]
        public void GetCommandFromMessage_ReturnsCommandWithBotName()
        {
            // Arrange
            var botName = "TestBotName";
            var message = EntityFactory.NewMessage($"/correctCommand@{botName} parameter1");

            // Act
            var command = BotCommand.GetCommandFromMessage(message);

            // Assert
            Assert.NotNull(command);
            Assert.Equal(botName, command.TargetBotName);
            Assert.Equal(1, command.Parametres?.Count);
        }

        [Theory]
        [InlineData("/correctCommand parameter1 prameter2, parameter3;  parameter4", null)]
        [InlineData("/correctCommand parameter1  \"para meter 2\"  ", null)]
        [InlineData("/correctCommand@botName parameter1,\"para meter 2\"  ", "botName")]
        [InlineData("/correctCommand@botName", "botName")]
        [InlineData("/correctCommand   \"\"  ", null)]
        [InlineData("/correctCommand\"\"  ", null)]
        [InlineData("/correctCommand", null)]
        public void IsCommand_ReturnsTrue(string messageText, string botName)
        {
            // Act
            var isCommand = BotCommand.IsCommand(messageText, botName);

            // Assert
            Assert.True(isCommand);
        }

        [Theory]
        [InlineData("noCommand/ parameter1 prameter2,\"para meter 3\" parameter4;  parameter5")]
        [InlineData("/ noCommand parameter1 prameter2,\"para meter 3\" parameter4;  parameter5")]
        [InlineData("//noCommand parameter1 prameter2,\"para meter 3\" parameter4;  parameter5")]
        [InlineData("")]
        [InlineData("/")]
        [InlineData("noCommand")]
        public void IsCommand_ReturnsFalse(string messageText)
        {
            // Act
            var isCommand = BotCommand.IsCommand(messageText);

            // Assert
            Assert.False(isCommand);
        }
    }
}
