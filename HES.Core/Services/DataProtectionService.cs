using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly ILogger<DataProtectionService> _logger;
        private bool protectionEnabled;
        private bool protectionActivated;
        private bool protectionBusy;
        private byte[] key;
        private byte[] iv;

        public DataProtectionService(IAsyncRepository<DataProtection> dataProtectionRepository,
                                     IAsyncRepository<Device> deviceRepository,
                                     IAsyncRepository<DeviceTask> deviceTaskRepository,
                                     IAsyncRepository<SharedAccount> sharedAccountRepository,
                                     ILogger<DataProtectionService> logger)
        {
            _dataProtectionRepository = dataProtectionRepository;
            _deviceRepository = deviceRepository;
            _deviceTaskRepository = deviceTaskRepository;
            _sharedAccountRepository = sharedAccountRepository;
            _logger = logger;
        }

        public async Task<ProtectionStatus> Status()
        {
            var data = await GetEncryptedEntityAsync();

            if (data != null)
            {
                protectionEnabled = true;

                if (!protectionActivated)
                {
                    return ProtectionStatus.Activate;
                }

                if (protectionBusy)
                {
                    return ProtectionStatus.Busy;
                }

                return ProtectionStatus.On;
            }

            protectionEnabled = false;
            return ProtectionStatus.Off;
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
                protectionEnabled = true;
                protectionActivated = true;
                protectionBusy = false;
            }
            catch (Exception ex)
            {
                protectionBusy = false;
                _logger.LogError(ex.Message);
                throw;
            }
        }

        //public async Task DisableProtectionAsync(string secret)
        //{
        //    if (string.IsNullOrWhiteSpace(secret))
        //    {
        //        throw new ArgumentNullException(nameof(secret));
        //    }

        //    if (protectionBusy)
        //    {
        //        throw new Exception("Data protection is busy.");
        //    }

        //    if (!protectionEnabled)
        //    {
        //        throw new Exception("Data protection is already disabled.");
        //    }

        //    try
        //    {
        //        protectionBusy = true;
        //        // Get value
        //        var data = await GetEncryptedEntity();
        //        // Decrypt value, if no error occurred during the decrypt, then the password is correct                
        //        ValidateSecret(secret, data.Value);

        //        // Unprotect
        //        //await UnprotectAllDataAsync();
        //        // Disable protector
        //        //_dataProtector = null;
        //        //await _dataProtectionRepository.DeleteAsync(dataProtection);

        //        protectionEnabled = false;
        //        protectionActivated = false;
        //        protectionBusy = false;
        //    }
        //    catch (CryptographicException)
        //    {
        //        protectionBusy = false;
        //    }
        //    catch (Exception ex)
        //    {
        //        protectionBusy = false;
        //        throw new Exception(ex.Message);
        //    }
        //}

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
                // Get value
                var data = await GetEncryptedEntityAsync();
                // Decrypt value, if no error occurred during the decrypt, then the password is correct                
                ValidateSecret(secret, data.Value);
                // Activated
                protectionActivated = true;
                protectionBusy = false;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
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
                // Get value
                var data = await GetEncryptedEntityAsync();
                // Decrypt value, if no error occurred during the decrypt, then the password is correct                
                ValidateSecret(oldSecret, data.Value);
                // Change data secret
                await ChangeDataSecretAsync(newSecret);
                protectionBusy = false;
            }
            catch (Exception ex)
            {
                protectionBusy = false;
                _logger.LogError(ex.Message);
                throw;
            }
        }

        //public string Protect(string text)
        //{
        //    if (text == null)
        //        return null;

        //    if (protectionEnabled)
        //    {
        //        if (!protectionActivated)
        //        {
        //            throw new Exception("Data protection not activated");
        //        }
        //        if (protectionBusy)
        //        {
        //            throw new Exception("Data protection is busy");
        //        }
        //        return _dataProtector.Protect(text);
        //    }
        //    return text;
        //}

        //public string Unprotect(string text)
        //{
        //    if (text == null)
        //        return null;

        //    if (protectionEnabled)
        //    {
        //        if (!protectionActivated)
        //        {
        //            throw new Exception("Data protection not activated");
        //        }
        //        if (protectionBusy)
        //        {
        //            throw new Exception("Data protection is busy");
        //        }
        //        return _dataProtector.Unprotect(text);
        //    }
        //    return text;
        //}


        private async Task ChangeDataSecretAsync(string secret)
        {
            List<Device> devices;
            List<DeviceTask> deviceTasks;
            List<SharedAccount> sharedAccounts;

            // Decrypt
            try
            {
                devices = await _deviceRepository.Query().ToListAsync();
                foreach (var device in devices)
                {
                    if (device.MasterPassword != null)
                    {
                        device.MasterPassword = Decrypt(device.MasterPassword);
                    }
                }

                deviceTasks = await _deviceTaskRepository.Query().ToListAsync();
                foreach (var task in deviceTasks)
                {
                    if (task.Password != null)
                    {
                        task.Password = Decrypt(task.Password);
                    }
                    if (task.OtpSecret != null)
                    {
                        task.OtpSecret = Decrypt(task.OtpSecret);
                    }
                }

                sharedAccounts = await _sharedAccountRepository.Query().ToListAsync();
                foreach (var account in sharedAccounts)
                {
                    //var accountProperties = new List<string>();
                    if (account.Password != null)
                    {
                        account.Password = Decrypt(account.Password);
                        //accountProperties.Add("Password");
                    }
                    if (account.OtpSecret != null)
                    {
                        account.OtpSecret = Decrypt(account.OtpSecret);
                        //accountProperties.Add("OtpSecret");
                    }
                    //await _sharedAccountRepository.UpdateOnlyPropAsync(account, accountProperties.ToArray());
                }
            }
            catch (CryptographicException)
            {
                throw new Exception("Decryption error, data was protected with another key.");
            }


            CreateEncryptionKeys(secret);
            await SetEncryptedEntityAsync();

            // Encrypt
            //var devices = await _deviceRepository.Query().ToListAsync();
            foreach (var device in devices)
            {
                if (device.MasterPassword != null)
                {
                    device.MasterPassword = Encrypt(device.MasterPassword);
                    //await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "MasterPassword" });
                }
            }

            //_logger.LogDebug($"DeviceTasks stage");
            //var deviceTasks = await _deviceTaskRepository.Query().ToListAsync();
            foreach (var task in deviceTasks)
            {
                //var taskProperties = new List<string>();
                if (task.Password != null)
                {
                    task.Password = Encrypt(task.Password);
                    //taskProperties.Add("Password");
                }
                if (task.OtpSecret != null)
                {
                    task.OtpSecret = Encrypt(task.OtpSecret);
                    //taskProperties.Add("OtpSecret");
                }
                //await _deviceTaskRepository.UpdateOnlyPropAsync(task, taskProperties.ToArray());
            }

            //_logger.LogDebug($"SharedAccounts stage");
            //var sharedAccounts = await _sharedAccountRepository.Query().ToListAsync();
            foreach (var account in sharedAccounts)
            {
                //var accountProperties = new List<string>();
                if (account.Password != null)
                {
                    account.Password = Encrypt(account.Password);
                    //accountProperties.Add("Password");
                }
                if (account.OtpSecret != null)
                {
                    account.OtpSecret = Encrypt(account.OtpSecret);
                    //accountProperties.Add("OtpSecret");
                }
                //await _sharedAccountRepository.UpdateOnlyPropAsync(account, accountProperties.ToArray());
            }


            // Update devices
            await _deviceRepository.UpdateOnlyPropAsync(devices, new string[] { "MasterPassword" });
            // Update tasks
            await _deviceTaskRepository.UpdateOnlyPropAsync(deviceTasks, new string[] { "Password", "OtpSecret" });
            // Update sharedAccounts
            await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccounts, new string[] { "Password", "OtpSecret" });
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

        private void CreateEncryptionKeys(string value)
        {
            var hash = GetSHA256(value);
            key = new byte[32];
            iv = new byte[16];
            Array.Copy(hash, 0, key, 0, 32);
            Array.Copy(hash, 0, iv, 0, 16);
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

        private byte[] EncryptStringToBytes(string plainText, byte[] key, byte[] iv)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException(nameof(plainText));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException(nameof(iv));
            byte[] encrypted;

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

            return plaintext;
        }

        private void ValidateSecret(string secret, string cipherText)
        {
            try
            {
                // Set temp keys
                var hash = GetSHA256(secret);
                byte[] tempKey = new byte[32];
                byte[] tempIv = new byte[16];
                Array.Copy(hash, 0, tempKey, 0, 32);
                Array.Copy(hash, 0, tempIv, 0, 16);
                // Try decrypt
                var cipherBytes = Convert.FromBase64String(cipherText);
                DecryptStringFromBytes(cipherBytes, tempKey, tempIv);
                // Set keys
                key = tempKey;
                iv = tempIv;
            }
            catch (CryptographicException)
            {
                throw new Exception("Cannot to decrypt. Secret is not valid.");
            }
        }

        private async Task<DataProtection> GetEncryptedEntityAsync()
        {
            return await _dataProtectionRepository
                 .Query()
                 .AsNoTracking()
                 .FirstOrDefaultAsync();
        }

        private async Task SetEncryptedEntityAsync()
        {
            // Encrypt
            var guid = Guid.NewGuid().ToString();
            var enc = EncryptStringToBytes(guid, key, iv);
            var encrypted = Convert.ToBase64String(enc);
            // Check exist
            var data = await GetEncryptedEntityAsync();
            if (data != null)
            {
                await _dataProtectionRepository.DeleteAsync(data);
            }
            // Save to base
            await _dataProtectionRepository.AddAsync(new DataProtection() { Value = encrypted });
        }
    }
}