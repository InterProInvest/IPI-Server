using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Web.Accounts
{
    public class WorkstationDomain : WorkstationAccount
    {
        [Required]
        public string Domain { get; set; }

        public bool UpdateActiveDirectoryPassword { get; set; }

        // Temporary solution, for setting a flag for an account
        public bool UpdateInActiveDirectory { get; set; }
    }
}