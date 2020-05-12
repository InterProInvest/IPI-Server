using System;

namespace HES.Core.Exceptions
{
    public class IncorrectOtpException : Exception
    {
        public IncorrectOtpException(string message) : base(message)
        {

        }
    }
}