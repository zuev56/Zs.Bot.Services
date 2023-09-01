using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using NSubstitute;
using Xunit;
using Zs.Bot.Services.Commands;
using Zs.Common.Abstractions;

namespace Zs.Bot.Services.UnitTests.Commands;

public sealed class CommandManagerTests
{
    private readonly IFixture _fixture;

    public CommandManagerTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoNSubstituteCustomization());
    }


    [Theory (Skip = "unable to mock static ShellLauncher")]
    [InlineData("/cli only the name of the command is important here")]
    [InlineData("/cli ls")]
    [InlineData("/cli sudo apt update")]
    [InlineData("/cli Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope LocalMachine")]
    public async Task ExecuteCommandAsync_CliCommand_ReturnsResult(string messageText)
    {
        throw new NotImplementedException();
    }

    [Theory]
    [InlineData("/sql only the name of the command is important here")]
    [InlineData("/sql select * from table")]
    [InlineData("/sql \"select 1\"")]
    [InlineData("/sql insert into table (col1, col2, col3) values (1, 2, 3)")]
    public async Task ExecuteCommandAsync_SqlCommand_ReturnsResult(string messageText)
    {
        var dbClient = _fixture.Freeze<IDbClient>();
        var sqlQueryResult = _fixture.Create<string>();
        dbClient.GetQueryResultAsync(Arg.Any<string>()).Returns(sqlQueryResult);
        var commandManager = _fixture.Create<CommandManager>();

        var result = await commandManager.ExecuteCommandAsync(messageText, CancellationToken.None);

        result.Should().Be(sqlQueryResult);
    }
}