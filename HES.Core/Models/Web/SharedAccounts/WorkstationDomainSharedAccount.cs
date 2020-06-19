using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Web.SharedAccounts
{
    public class WorkstationDomainSharedAccount : WorkstationSharedAccount
    {
        [Required]
        public string Domain { get; set; }
    }
}