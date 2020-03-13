using HES.Core.Services;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDataProtectionService
    {
        Task InitializeAsync();
        ProtectionStatus Status();       
        void Validate();
        Task EnableProtectionAsync(string password);
        Task DisableProtectionAsync(string password);
        Task<bool> ActivateProtectionAsync(string password);
        Task ChangeProtectionPasswordAsync(string oldPassword, string newPassword);
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }
}