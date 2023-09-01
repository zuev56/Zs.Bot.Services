using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Zs.Bot.Data.Models;
using Zs.Bot.Data.Repositories;
using Zs.Bot.Services.Messaging;
using Zs.Bot.Services.Storages;

namespace Zs.Bot.Services.UnitTests.Storages;

public sealed class MessageDataDbStorageTests
{
    private readonly IFixture _fixture = new Fixture();

    private MessageDataDbStorage CreateMessageDataDbStorage(
        IUsersRepository? usersRepository = null,
        IChatsRepository? chatsRepository = null,
        IMessagesRepository? messagesRepository = null)
    {
        _fixture.Inject(usersRepository ?? Substitute.For<IUsersRepository>());
        _fixture.Inject(chatsRepository ?? Substitute.For<IChatsRepository>());
        _fixture.Inject(messagesRepository ?? Substitute.For<IMessagesRepository>());
        _fixture.Inject(Substitute.For<ILogger<MessageDataDbStorage>>());

        return _fixture.Create<MessageDataDbStorage>();
    }

    [Fact]
    public async Task SaveNewMessageDataAsync_MessageActionDataContainsNewUser_SaveUser()
    {
        var usersRepository = Substitute.For<IUsersRepository>();
        usersRepository.ExistsAsync(Arg.Any<long>()).Returns(false);
        var messageDataDbStorage = CreateMessageDataDbStorage(usersRepository);
        var user = _fixture.Create<User>();
        var messageActionData = new MessageActionData { User = user };

        await messageDataDbStorage.SaveNewMessageDataAsync(messageActionData, CancellationToken.None);

        await usersRepository.Received().ExistsAsync(user.Id);
        await usersRepository.Received().AddAsync(user);
    }

    [Fact]
    public async Task SaveNewMessageDataAsync_MessageActionDataContainsExistingUser_DoNotSaveUser()
    {
        var usersRepository = Substitute.For<IUsersRepository>();
        usersRepository.ExistsAsync(Arg.Any<long>()).Returns(true);
        var messageDataDbStorage = CreateMessageDataDbStorage(usersRepository);
        var user = _fixture.Create<User>();
        var messageActionData = new MessageActionData { User = user };

        await messageDataDbStorage.SaveNewMessageDataAsync(messageActionData, CancellationToken.None);

        await usersRepository.Received().ExistsAsync(user.Id);
        await usersRepository.DidNotReceive().AddAsync(user);
    }

    [Fact]
    public async Task SaveNewMessageDataAsync_MessageActionDataDoesNotContainUser_DoNotSaveUser()
    {
        var usersRepository = Substitute.For<IUsersRepository>();
        var messageDataDbStorage = CreateMessageDataDbStorage(usersRepository);
        var messageActionData = new MessageActionData { User = null };

        await messageDataDbStorage.SaveNewMessageDataAsync(messageActionData, CancellationToken.None);

        await usersRepository.DidNotReceive().ExistsAsync(Arg.Any<long>());
        await usersRepository.DidNotReceive().AddAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task SaveNewMessageDataAsync_MessageActionDataContainsNewChat_SaveChat()
    {
        var chatsRepository = Substitute.For<IChatsRepository>();
        chatsRepository.ExistsAsync(Arg.Any<long>()).Returns(false);
        var messageDataDbStorage = CreateMessageDataDbStorage(chatsRepository: chatsRepository);
        var chat = _fixture.Create<Chat>();
        var messageActionData = new MessageActionData { Chat = chat };

        await messageDataDbStorage.SaveNewMessageDataAsync(messageActionData, CancellationToken.None);

        await chatsRepository.Received().ExistsAsync(chat.Id);
        await chatsRepository.Received().AddAsync(chat);
    }

    [Fact]
    public async Task SaveNewMessageDataAsync_MessageActionDataContainsExistingChat_DoNotSaveChat()
    {
        var chatsRepository = Substitute.For<IChatsRepository>();
        chatsRepository.ExistsAsync(Arg.Any<long>()).Returns(true);
        var messageDataDbStorage = CreateMessageDataDbStorage(chatsRepository: chatsRepository);
        var chat = _fixture.Create<Chat>();
        var messageActionData = new MessageActionData { Chat = chat };

        await messageDataDbStorage.SaveNewMessageDataAsync(messageActionData, CancellationToken.None);

        await chatsRepository.Received().ExistsAsync(chat.Id);
        await chatsRepository.DidNotReceive().AddAsync(chat);
    }

    [Fact]
    public async Task SaveNewMessageDataAsync_MessageActionDataDoesNotContainChat_DoNotSaveChat()
    {
        var chatsRepository = Substitute.For<IChatsRepository>();
        var messageDataDbStorage = CreateMessageDataDbStorage(chatsRepository: chatsRepository);
        var messageActionData = new MessageActionData { Chat = null };

        await messageDataDbStorage.SaveNewMessageDataAsync(messageActionData, CancellationToken.None);

        await chatsRepository.DidNotReceive().ExistsAsync(Arg.Any<long>());
        await chatsRepository.DidNotReceive().AddAsync(Arg.Any<Chat>());
    }

    [Fact]
    public async Task SaveNewMessageDataAsync_MessageActionDataContainsMessage_SaveMessage()
    {
        var messagesRepository = Substitute.For<IMessagesRepository>();
        var messageDataDbStorage = CreateMessageDataDbStorage(messagesRepository: messagesRepository);
        var message = _fixture.Build<Message>()
            .Without(m => m.ReplyToMessage)
            .Create();
        var messageActionData = new MessageActionData { Message = message };

        await messageDataDbStorage.SaveNewMessageDataAsync(messageActionData, CancellationToken.None);

        await messagesRepository.Received().AddAsync(message);
    }

    [Fact]
    public async Task SaveNewMessageDataAsync_MessageActionDataDoesNotContainMessage_DoNotSaveMessage()
    {
        var messagesRepository = Substitute.For<IMessagesRepository>();
        var messageDataDbStorage = CreateMessageDataDbStorage(messagesRepository: messagesRepository);
        var messageActionData = new MessageActionData { Message = null };

        await messageDataDbStorage.SaveNewMessageDataAsync(messageActionData, CancellationToken.None);

        await messagesRepository.DidNotReceive().ExistsAsync(Arg.Any<long>());
        await messagesRepository.DidNotReceive().AddAsync(Arg.Any<Message>());
    }

    [Fact]
    public async Task EditSavedMessageAsync_MessageIsNull_ThrowArgumentNullException()
    {
        var messagesRepository = Substitute.For<IMessagesRepository>();
        var messageDataDbStorage = CreateMessageDataDbStorage(messagesRepository: messagesRepository);

        var action = async () => await messageDataDbStorage.EditSavedMessageAsync(null!, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task EditSavedMessageAsync_MessageExists_UpdateMessage()
    {
        var messagesRepository = Substitute.For<IMessagesRepository>();
        var messageDataDbStorage = CreateMessageDataDbStorage(messagesRepository: messagesRepository);
        var message = _fixture.Build<Message>()
            .Without(m => m.ReplyToMessage)
            .Create();
        messagesRepository.ExistsAsync(message.Id).Returns(true);

        await messageDataDbStorage.EditSavedMessageAsync(message, CancellationToken.None);

        await messagesRepository.Received().ExistsAsync(message.Id);
        await messagesRepository.Received().AddAsync(message);
    }

    [Fact]
    public async Task EditSavedMessageAsync_MessageDoesNotExist_NotThrow()
    {
        var messagesRepository = Substitute.For<IMessagesRepository>();
        var messageDataDbStorage = CreateMessageDataDbStorage(messagesRepository: messagesRepository);
        var message = _fixture.Build<Message>()
            .Without(m => m.ReplyToMessage)
            .Create();
        messagesRepository.ExistsAsync(message.Id).Returns(false);

        var action = async () => await messageDataDbStorage.EditSavedMessageAsync(message, CancellationToken.None);

        await action.Should().NotThrowAsync();
        await messagesRepository.Received().ExistsAsync(message.Id);
        await messagesRepository.DidNotReceive().AddAsync(message);
    }
}