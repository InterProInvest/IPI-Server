using HES.Core.Interfaces;
using Newtonsoft.Json;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace HES.Core.Services
{
    public class AesCryptographyService : IAesCryptographyService
    {
        private byte[] saltBytes { get; }
        private SymmetricAlgorithm cipher { get; }

        public AesCryptographyService()
        {
            saltBytes = new byte[] { 1, 123, 90, 49, 98, 121, 2, 56 };

            cipher = Aes.Create();
            cipher.Mode = CipherMode.CBC;
            cipher.BlockSize = 128;
            cipher.KeySize = 128;
        }

        private byte[] Encrypt(byte[] data, byte[] password)
        {
            byte[] encrypted;
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, saltBytes, 1000);
            cipher.Key = key.GetBytes(cipher.KeySize / 8);
            cipher.IV = key.GetBytes(cipher.BlockSize / 8);
            using (MemoryStream ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, cipher.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                }
                encrypted = ms.ToArray();
                return encrypted;
            }
        }

        private byte[] Decrypt(byte[] toDecrypt, byte[] password)
        {
            byte[] decrypted;
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, saltBytes, 1000);
            cipher.Key = key.GetBytes(cipher.KeySize / 8);
            cipher.IV = key.GetBytes(cipher.BlockSize / 8);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream reader = new CryptoStream(ms, cipher.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    reader.Write(toDecrypt, 0, toDecrypt.Length);
                }
                decrypted = ms.ToArray();
            }
            return decrypted;
        }

        public byte[] EncryptObject(object toEncrypt, byte[] password)
        {
            var byteObject = ObjectToByteArray(toEncrypt);
            return Encrypt(byteObject, password);
        }

        public T DecryptObject<T>(byte[] toDecrypt, byte[] password)
        {
            var decryptedObject = Decrypt(toDecrypt, password);
            //var str = Encoding.UTF8.GetString(decryptedObject);
            var str = Encoding.Unicode.GetString(decryptedObject);
            T res = JsonConvert.DeserializeObject<T>(str);
            return res;
        }

        private byte[] ObjectToByteArray(object obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            return Encoding.Unicode.GetBytes(json);
        }
    }
}