using System;
using System.Threading;
using System.Threading.Tasks;
using Zs.Bot.Data.Models;
using Zs.Bot.Services.Commands;
using Zs.Bot.Services.Messaging;
using Zs.Common.Models;

namespace Zs.Bot.Services.Pipeline;

public sealed class HandleCommandStep : PipelineStep
{
    private readonly IBotClient _botClient;
    private readonly ICommandManager _commandManager;
    private readonly string? _botName;
    private readonly Func<Message, string?> _getMessageText;

    public HandleCommandStep(IBotClient botClient, ICommandManager commandManager, Func<Message, string?> getMessageText, string? botName)
    {
        _botClient = botClient;
        _commandManager = commandManager;
        _getMessageText = getMessageText;
        _botName = botName;
    }

    protected override async Task<Result> PerformInternalAsync(MessageActionData messageActionData, CancellationToken cancellationToken)
    {
        var (message, chat, _, action) = messageActionData;
        if (action != MessageAction.Received)
            return Result.Success();

        var messageText = _getMessageText.Invoke(message!);
        if (!messageText.IsBotCommand(_botName))
            return Result.Success();

        var commandResult = await _commandManager.ExecuteCommandAsync(messageText!, cancellationToken);
        await _botClient.SendMessageAsync(commandResult, chat!, cancellationToken: cancellationToken);
        return Result.Success();
    }
}