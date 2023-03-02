using Xunit;
using Zs.Bot.Data.Factories;
using Zs.Bot.Services.Commands;

namespace UnitTests;

public sealed class BotCommandTests
{
    [Fact]
    public void GetCommandFromMessage_ReturnsCommand()
    {
        var commandName = "/correctCommand";
        var parameter5 = "para meter 5";
        var dbUserId = 8;
        var dbChatId = 7;
        var message = EntityFactory.NewMessage($"{commandName} parameter1 prameter2, parameter3;  parameter4 \"{parameter5}\"");
        message.UserId = dbUserId;
        message.ChatId = dbChatId;

        var command = BotCommand.GetCommandFromMessage(message);

        Assert.NotNull(command);
        Assert.Equal(commandName.ToLowerInvariant(), command.Name);
        Assert.Equal(5, command.Parameters?.Count);
        Assert.Equal(parameter5, command.Parameters[4]);
        Assert.Null(command.TargetBotName);
        Assert.Equal(dbUserId, command.FromUserId);
        Assert.Equal(dbChatId, command.ChatIdForAnswer);
        Assert.False(command.IsKnown);
    }

    [Fact]
    public void GetCommandFromMessage_ReturnsCommandWithBotName()
    {
        var botName = "TestBotName";
        var message = EntityFactory.NewMessage($"/correctCommand@{botName} parameter1");

        var command = BotCommand.GetCommandFromMessage(message);

        Assert.NotNull(command);
        Assert.Equal(botName.ToLower(), command.TargetBotName);
        Assert.Single(command.Parameters);
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
        var isCommand = BotCommand.IsCommand(messageText, botName);

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
        var isCommand = BotCommand.IsCommand(messageText);

        Assert.False(isCommand);
    }
}