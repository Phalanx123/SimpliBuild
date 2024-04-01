using System;

namespace simpliBuild.Exceptions;

public class SimpliBuildConnectionException : Exception
{
    /// <summary>
    /// SimpliBuildException
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public SimpliBuildConnectionException(string message, Exception innerException) : base(message, innerException) { }
}