using System;
using Microsoft.Extensions.Logging;
using Zs.Bot.Data.Models;
using Zs.Bot.Services.Commands;
using Zs.Bot.Services.Messaging;
using Zs.Bot.Services.Storages;

namespace Zs.Bot.Services.Pipeline;

public static class BotClientExtensions
{
    public static IBotClient Use(this IBotClient botClient, PipelineStep pipelineStep)
    {
        botClient.AddToMessagePipeline(pipelineStep);

        return botClient;
    }

    public static IBotClient UseMessageDataSaver(this IBotClient botClient, IMessageDataStorage messageDataSaver)
    {
        var saveMessageDataStep = new SaveMessageDataStep(messageDataSaver);
        botClient.AddToMessagePipeline(saveMessageDataStep);

        return botClient;
    }

    public static IBotClient UseCommandManager(this IBotClient botClient, ICommandManager commandManager, Func<Message, string?> getMessageText, string? botName)
    {
        var handleCommandStep = new HandleCommandStep(botClient, commandManager, getMessageText, botName);
        botClient.AddToMessagePipeline(handleCommandStep);

        return botClient;
    }

    public static IBotClient UseLogger(this IBotClient botClient, ILogger logger)
    {
        var handleCommandStep = new LogMessageStep(logger);
        botClient.AddToMessagePipeline(handleCommandStep);

        return botClient;
    }
}