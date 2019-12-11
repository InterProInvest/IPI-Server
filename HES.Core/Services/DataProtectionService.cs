using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public enum ProtectionStatus
    {
        Off,
        On,
        Activate,
        Busy
    }

    public class DataProtectionService : IDataProtectionService
    {
        private readonly IAsyncRepository<DataProtection> _dataProtectionRepository;
        private readonly IAsyncRepository<Device> _deviceRepository;
        private readonly IAsyncRepository<DeviceTask> _deviceTaskRepository;
        private readonly IAsyncRepository<SharedAccount> _sharedAccountRepository;
        private readonly IApplicationUserService _applicationUserService;
        private bool protectionEnabled;
        private bool protectionActivated;
        private bool protectionBusy;
        private byte[] key;
        private byte[] iv;

        public DataProtectionService(IAsyncRepository<DataProtection> dataProtectionRepository,
                                     IAsyncRepository<Device> deviceRepository,
                                     IAsyncRepository<DeviceTask> deviceTaskRepository,
                                     IAsyncRepository<SharedAccount> sharedAccountRepository,
                                     IApplicationUserService applicationUserService)
        {
            _dataProtectionRepository = dataProtectionRepository;
            _deviceRepository = deviceRepository;
            _deviceTaskRepository = deviceTaskRepository;
            _sharedAccountRepository = sharedAccountRepository;
            _applicationUserService = applicationUserService;
        }

        public async Task Initialize()
        {
            var data = await GetEncryptedEntityAsync();

            if (data != null)
            {
                protectionEnabled = true;
            }

            if (protectionEnabled)
            {
                await _applicationUserService.SendEmailDataProtectionNotify();
            }
        }

        public ProtectionStatus Status()
        {
            if (!protectionEnabled)
            {
                return ProtectionStatus.Off;
            }

            if (!protectionActivated)
            {
                return ProtectionStatus.Activate;
            }

            return ProtectionStatus.On;
        }

        public void Validate()
        {
            if (!protectionEnabled)
            {
                throw new Exception("Data protection not enabled.");
            }

            if (!protectionActivated)
            {
                throw new Exception("Data protection not activated.");
            }

            if (protectionBusy)
            {
                throw new Exception("Data protection is busy.");
            }
        }

        public async Task EnableProtectionAsync(string secret)
        {
            if (string.IsNullOrWhiteSpace(secret))
            {
                throw new ArgumentNullException(nameof(secret));
            }

            if (protectionBusy)
            {
                throw new Exception("Data protection is busy.");
            }

            if (protectionEnabled)
            {
                throw new Exception("Data protection is already enabled.");
            }

            try
            {
                protectionBusy = true;
                // Create encryption keys
                CreateEncryptionKeys(secret);
                // Encrypt value
                await SetEncryptedEntityAsync();
                // Enabled
                await EncryptAllAsync();
                protectionEnabled = true;
                protectionActivated = true;
                protectionBusy = false;
            }
            catch (Exception)
            {
                protectionBusy = false;
                throw;
            }
        }

        public async Task ActivateProtectionAsync(string secret)
        {
            if (string.IsNullOrWhiteSpace(secret))
            {
                throw new ArgumentNullException(nameof(secret));
            }

            if (protectionBusy)
            {
                throw new Exception("Data protection is busy.");
            }

            if (protectionActivated)
            {
                throw new Exception("Data protection is already activated.");
            }

            try
            {
                protectionBusy = true;
                // Decrypt value, if no error occurred during the decrypt, then the password is correct                
                await ValidateSecret(secret);
                // Activated
                protectionActivated = true;
                protectionBusy = false;
            }

            catch (Exception)
            {
                protectionBusy = false;
                throw;
            }
        }

        public async Task ChangeProtectionSecretAsync(string oldSecret, string newSecret)
        {
            if (string.IsNullOrWhiteSpace(oldSecret))
            {
                throw new ArgumentNullException(nameof(oldSecret));
            }

            if (string.IsNullOrWhiteSpace(newSecret))
            {
                throw new ArgumentNullException(nameof(newSecret));
            }

            if (protectionBusy)
            {
                throw new Exception("Data protection is busy.");
            }

            if (!protectionEnabled)
            {
                throw new Exception("Data protection is not enabled.");
            }

            if (!protectionActivated)
            {
                throw new Exception("Data protection is not activated.");
            }

            try
            {
                protectionBusy = true;
                // Decrypt value, if no error occurred during the decrypt, then the password is correct                
                await ValidateSecret(oldSecret);
                // Change data secret
                await ChangeDataSecretAsync(newSecret);
                protectionBusy = false;
            }
            catch (Exception)
            {
                protectionBusy = false;
                throw;
            }
        }

        public string Encrypt(string plainText)
        {
            if (plainText == null)
                return null;

            Validate();

            var enc = EncryptStringToBytes(plainText, key, iv);
            return Convert.ToBase64String(enc);
        }

        public string Decrypt(string cipherText)
        {
            if (cipherText == null)
                return null;

            Validate();

            var cipherBytes = Convert.FromBase64String(cipherText);
            return DecryptStringFromBytes(cipherBytes, key, iv);
        }

        private async Task ChangeDataSecretAsync(string secret)
        {
            var devices = await _deviceRepository.Query().ToListAsync(); ;
            var deviceTasks = await _deviceTaskRepository.Query().ToListAsync();
            var sharedAccounts = await _sharedAccountRepository.Query().ToListAsync();

            // Decrypt
            try
            {
                foreach (var device in devices)
                {
                    if (device.MasterPassword != null)
                    {
                        device.MasterPassword = InternalDecrypt(device.MasterPassword);
                    }
                }

                foreach (var task in deviceTasks)
                {
                    if (task.Password != null)
                    {
                        task.Password = InternalDecrypt(task.Password);
                    }
                    if (task.OtpSecret != null)
                    {
                        task.OtpSecret = InternalDecrypt(task.OtpSecret);
                    }
                }

                foreach (var account in sharedAccounts)
                {
                    if (account.Password != null)
                    {
                        account.Password = InternalDecrypt(account.Password);
                    }
                    if (account.OtpSecret != null)
                    {
                        account.OtpSecret = InternalDecrypt(account.OtpSecret);
                    }
                }
            }
            catch (CryptographicException)
            {
                throw new Exception("Decryption error, data was protected with another key.");
            }

            CreateEncryptionKeys(secret);
            await SetEncryptedEntityAsync();

            // Encrypt
            foreach (var device in devices)
            {
                if (device.MasterPassword != null)
                {
                    device.MasterPassword = InternalEncrypt(device.MasterPassword);
                }
            }

            foreach (var task in deviceTasks)
            {
                if (task.Password != null)
                {
                    task.Password = InternalEncrypt(task.Password);
                }
                if (task.OtpSecret != null)
                {
                    task.OtpSecret = InternalEncrypt(task.OtpSecret);
                }
            }

            foreach (var account in sharedAccounts)
            {
                if (account.Password != null)
                {
                    account.Password = InternalEncrypt(account.Password);
                }
                if (account.OtpSecret != null)
                {
                    account.OtpSecret = InternalEncrypt(account.OtpSecret);
                }
            }

            // Update devices
            await _deviceRepository.UpdateOnlyPropAsync(devices, new string[] { "MasterPassword" });
            // Update tasks
            await _deviceTaskRepository.UpdateOnlyPropAsync(deviceTasks, new string[] { "Password", "OtpSecret" });
            // Update sharedAccounts
            await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccounts, new string[] { "Password", "OtpSecret" });
        }

        private async Task EncryptAllAsync()
        {
            var devices = await _deviceRepository.Query().ToListAsync(); ;
            var deviceTasks = await _deviceTaskRepository.Query().ToListAsync();
            var sharedAccounts = await _sharedAccountRepository.Query().ToListAsync();

            // Encrypt
            foreach (var device in devices)
            {
                if (device.MasterPassword != null)
                {
                    device.MasterPassword = InternalEncrypt(device.MasterPassword);
                }
            }

            foreach (var task in deviceTasks)
            {
                if (task.Password != null)
                {
                    task.Password = InternalEncrypt(task.Password);
                }
                if (task.OtpSecret != null)
                {
                    task.OtpSecret = InternalEncrypt(task.OtpSecret);
                }
            }

            foreach (var account in sharedAccounts)
            {
                if (account.Password != null)
                {
                    account.Password = InternalEncrypt(account.Password);
                }
                if (account.OtpSecret != null)
                {
                    account.OtpSecret = InternalEncrypt(account.OtpSecret);
                }
            }

            // Update devices
            await _deviceRepository.UpdateOnlyPropAsync(devices, new string[] { "MasterPassword" });
            // Update tasks
            await _deviceTaskRepository.UpdateOnlyPropAsync(deviceTasks, new string[] { "Password", "OtpSecret" });
            // Update sharedAccounts
            await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccounts, new string[] { "Password", "OtpSecret" });
        }

        private string InternalEncrypt(string plainText)
        {
            if (plainText == null)
                return null;

            var enc = EncryptStringToBytes(plainText, key, iv);
            return Convert.ToBase64String(enc);
        }

        private string InternalDecrypt(string cipherText)
        {
            if (cipherText == null)
                return null;

            var cipherBytes = Convert.FromBase64String(cipherText);
            return DecryptStringFromBytes(cipherBytes, key, iv);
        }

        private byte[] GetSHA256(string value)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                return sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
        }

        private void CreateEncryptionKeys(string value)
        {
            var hash = GetSHA256(value);
            key = new byte[32];
            iv = new byte[16];
            Array.Copy(hash, 0, key, 0, 32);
            Array.Copy(hash, 0, iv, 0, 16);
        }

        private byte[] EncryptStringToBytes(string plainText, byte[] key, byte[] iv)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException(nameof(plainText));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException(nameof(iv));

            byte[] encrypted;

            // Add salt
            var salt = new byte[128];
            using (var random = new RNGCryptoServiceProvider())
            {
                random.GetNonZeroBytes(salt);
            }
            var saltBase64 = (Convert.ToBase64String(salt)).Substring(0, 128);
            plainText = string.Concat(saltBase64, plainText);

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            return encrypted;
        }

        private string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException(nameof(cipherText));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException(nameof(iv));

            string plaintext;

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            // Remove salt
            plaintext = plaintext.Substring(128);
            return plaintext;
        }

        private async Task ValidateSecret(string secret)
        {
            try
            {
                // Get value
                var data = await GetEncryptedEntityAsync();
                // Set temp keys
                var hash = GetSHA256(secret);
                byte[] tempKey = new byte[32];
                byte[] tempIv = new byte[16];
                Array.Copy(hash, 0, tempKey, 0, 32);
                Array.Copy(hash, 0, tempIv, 0, 16);
                // Try decrypt
                var cipherBytes = Convert.FromBase64String(data.Value);
                var decrypted = DecryptStringFromBytes(cipherBytes, tempKey, tempIv);
                var decryptedHash = GetSHA256(decrypted);
                var originalHash = Convert.FromBase64String(data.ValueHash);
                var equals = decryptedHash.SequenceEqual(originalHash);
                if (!equals)
                {
                    throw new CryptographicException("Secret is not valid.");
                }
                // Set keys
                key = tempKey;
                iv = tempIv;
            }
            catch (CryptographicException)
            {
                throw new Exception("Secret is not valid.");
            }
        }

        private async Task<DataProtection> GetEncryptedEntityAsync()
        {
            return await _dataProtectionRepository
                 .Query()
                 .OrderBy(v => v.Id)
                 .FirstOrDefaultAsync();
        }

        private async Task SetEncryptedEntityAsync()
        {
            var guid = Guid.NewGuid().ToString();
            // Encrypt
            var encrypted = EncryptStringToBytes(guid, key, iv);
            var encryptedBase64 = Convert.ToBase64String(encrypted);
            // Hash from guid
            var valueHash = Convert.ToBase64String(GetSHA256(guid));
            // Check exist
            var data = await GetEncryptedEntityAsync();
            if (data != null)
            {
                await _dataProtectionRepository.DeleteAsync(data);
            }
            // Save to base
            await _dataProtectionRepository.AddAsync(new DataProtection() { Value = encryptedBase64, ValueHash = valueHash });
        }
    }
}