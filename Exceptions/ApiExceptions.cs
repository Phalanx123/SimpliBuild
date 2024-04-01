using System;

namespace simpliBuild.Exceptions;

public class SimpliBuildApiException : Exception
{
    public SimpliBuildApiException(string message) : base(message) { }
    
}