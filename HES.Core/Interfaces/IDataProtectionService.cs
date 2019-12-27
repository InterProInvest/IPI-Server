using HES.Core.Services;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDataProtectionService
    {
        Task Initialize();
        ProtectionStatus Status();       
        void Validate();
        Task EnableProtectionAsync(string secret);
        Task DisableProtectionAsync(string secret);
        Task<bool> ActivateProtectionAsync(string secret);
        Task ChangeProtectionSecretAsync(string oldSecret, string newSecret);
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }
}