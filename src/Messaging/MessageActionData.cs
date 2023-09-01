using Zs.Bot.Data.Models;

namespace Zs.Bot.Services.Messaging;

public sealed record MessageActionData
{
    public Message? Message { get; init; }
    public Chat? Chat { get; init; }
    public User? User { get; init; }
    public MessageAction Action { get; init; }

    public void Deconstruct(out Message? message, out Chat? chat, out User? user, out MessageAction action)
    {
        message = Message;
        chat = Chat;
        user = User;
        action = Action;
    }
}