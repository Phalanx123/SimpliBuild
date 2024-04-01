using System;

namespace simpliBuild.Exceptions;

public class SimpliBuildContentException : Exception
{
    public SimpliBuildContentException(string message) : base(message) { }
}