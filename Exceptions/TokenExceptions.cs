using System;

namespace simpliBuild.Exceptions;

public class SimpliBuildTokenRetrievalException : Exception
{
    public SimpliBuildTokenRetrievalException(string message, Exception innerException) 
        : base(message, innerException) { }
}
public class SimpliBuildTokenNullException : Exception
{
    public SimpliBuildTokenNullException(string message) : base(message) { }
}