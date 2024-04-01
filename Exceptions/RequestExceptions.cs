using System;

namespace simpliBuild.Exceptions;

public class SimpliBuildRequestException : Exception
{
    public SimpliBuildRequestException(string message) : base(message) { }
}