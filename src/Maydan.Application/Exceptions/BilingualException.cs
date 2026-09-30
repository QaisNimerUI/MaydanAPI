using System;

namespace Maydan.Application.Exceptions;

public class BilingualException : Exception
{
    public string MessageAr { get; }
    public string MessageEn { get; }

    public BilingualException(string messageAr, string messageEn)
        : base(messageEn)
    {
        MessageAr = messageAr;
        MessageEn = messageEn;
    }
}
