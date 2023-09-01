using System;
using FluentAssertions;
using Xunit;
using Zs.Bot.Services.Commands;

namespace Zs.Bot.Services.UnitTests.Commands;

public sealed class BotCommandTests
{
    [Theory]
    [InlineData("/command@botName parameter1 parameter2, parameter3;  parameter4", "/command", "botname", CommandType.Default)]
    [InlineData("/command1@botname1 parameter1  \"para meter 2\"  ", "/command1", "botname1", CommandType.Default)]
    [InlineData("/sql@2botName2 parameter1,\"para meter 2\"  ", "/sql", "2botname2", CommandType.Sql)]
    [InlineData("/3command3@bot3Name", "/3command3", "bot3name", CommandType.Default)]
    [InlineData("  /command@_123_  ", "/command", "_123_", CommandType.Default)]
    [InlineData("/_command@1   \"\"  ", "/_command", "1", CommandType.Default)]
    [InlineData("/cli@botName", "/cli", "botname", CommandType.Cli)]
    internal void Parse_CorrectMessageTextWithCommandForConcreteBot_ReturnsBotCommandWithBotName(string messageText, string command, string botName, CommandType commandType)
    {
        var botCommand = BotCommand.Parse(messageText);

        botCommand.Should().NotBeNull();
        botCommand.BotName.Should().Be(botName);
        botCommand.Command.Should().Be(command);
        botCommand.Type.Should().Be(commandType);
    }

    [Theory]
    [InlineData("/command parameter1 prameter2, parameter3;  parameter4", "/command", CommandType.Default)]
    [InlineData("/command1 parameter1  \"para meter 2\"  ", "/command1", CommandType.Default)]
    [InlineData("/sql parameter1,\"para meter 2\"  ", "/sql", CommandType.Sql)]
    [InlineData("/3command3", "/3command3", CommandType.Default)]
    [InlineData("  /command  ", "/command", CommandType.Default)]
    [InlineData("/_command   \"\"  ", "/_command", CommandType.Default)]
    [InlineData("/cli", "/cli", CommandType.Cli)]
    internal void Parse_CorrectMessageTextWithCommandAndEmptyBotName_ReturnsBotCommandWithoutBotName(string messageText, string command, CommandType commandType)
    {
        var botCommand = BotCommand.Parse(messageText);

        botCommand.Should().NotBeNull();
        botCommand.BotName.Should().BeNull();
        botCommand.Command.Should().Be(command);
        botCommand.Type.Should().Be(commandType);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    internal void Parse_MessageTextIsNullOrWhiteSpace_ThrowsArgumentException(string messageText)
    {
        var action = () => BotCommand.Parse(messageText);

        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("Hello")]
    [InlineData("12345678")]
    [InlineData("A B C")]
    internal void Parse_MessageTextIsNotABotCommand_ThrowsArgumentException(string messageText)
    {
        var action = () => BotCommand.Parse(messageText);

        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("/command@botName p1 p2, p3;  p4", "p1 p2, p3;  p4", new[] {"p1", "p2", "p3", "p4"})]
    [InlineData("/command1@botname p1  \"para meter 2\"  ", "p1  \"para meter 2\"", new[] {"p1", "para meter 2"})]
    [InlineData("/sql@2botName2", "", new string [0])]
    [InlineData("/cli@bot3Name   ", "", new string [0])]
    internal void Parse_ReturnsBotCommandWithExpectedParameters(string messageText, string rawParameters, string[] parameters)
    {
        var botCommand = BotCommand.Parse(messageText);

        botCommand.RawParameters.Should().Be(rawParameters);
        botCommand.Parameters.Should().BeEquivalentTo(parameters);
    }
}