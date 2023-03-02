using System;

namespace Zs.Bot.Services.Exceptions;

public sealed class OddNumberOfQuotesException : Exception
{
    public OddNumberOfQuotesException()
        : base("There must be an even number of quotes")
    {
    }
}