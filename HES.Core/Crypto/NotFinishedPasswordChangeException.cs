using System;

namespace HES.Core.Crypto
{
    public class NotFinishedPasswordChangeException : Exception
    {
        public NotFinishedPasswordChangeException(string message) : base(message)
        {

        }
    }
}