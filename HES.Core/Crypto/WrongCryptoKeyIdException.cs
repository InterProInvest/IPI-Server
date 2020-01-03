using System;
using System.Security.Cryptography;

namespace HES.Core.Crypto
{
    public class WrongCryptoKeyIdException : CryptographicException
    {
        public WrongCryptoKeyIdException()
        {
        }

        public WrongCryptoKeyIdException(string message) 
            : base(message)
        {
        }

        public WrongCryptoKeyIdException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
