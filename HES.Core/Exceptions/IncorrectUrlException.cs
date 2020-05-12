using System;

namespace HES.Core.Exceptions
{
    public class IncorrectUrlException : Exception
    {
        public IncorrectUrlException(string message) : base(message)
        {

        }
    }
}