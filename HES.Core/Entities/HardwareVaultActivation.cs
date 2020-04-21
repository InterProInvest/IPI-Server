using HES.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class HardwareVaultActivation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [Required]
        public string DeviceId { get; set; }
        [Required]
        public string AcivationCode { get; set; }
        public int WrongAttemptsCount { get; set; }
        public DateTime? LastWrongAttempt { get; set; }
        public DateTime CreatedAt { get; set; }
        public HardwareVaultActivationStatus Status { get; set; }
    }
}