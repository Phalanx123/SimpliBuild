using System;

namespace simpliBuild.Exceptions;

public class SimpliBuildResponseException : Exception
{
    public SimpliBuildResponseException(string message) : base(message) { }
}