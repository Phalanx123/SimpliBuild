using System;

namespace simpliBuild.Exceptions;

    public class SimpliBuildAuthenticationException : Exception
    {
        public SimpliBuildAuthenticationException(string message) : base(message) { }
        
    }
    