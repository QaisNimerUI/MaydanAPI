using System;

namespace Maydan.Application.Exceptions;

public class BilingualNotFoundException : BilingualException
{
    public BilingualNotFoundException(string messageAr, string messageEn)
        : base(messageAr, messageEn)
    {
    }
}
