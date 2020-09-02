using System;

namespace HES.Core.Exceptions
{
    public class SettingsNotSetException : Exception
    {
        public SettingsNotSetException(string message) : base(message)
        {

        }
    }
}
