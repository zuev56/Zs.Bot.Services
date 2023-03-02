namespace Zs.Bot.Services.Commands;

/// <summary>Contains command execution result</summary>
public sealed record CommandResult(int ChatIdForAnswer, string Text);