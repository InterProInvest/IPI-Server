﻿using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API
{
    public class SetAsWorkstationAccountDto
    {
        [Required]
        public string EmployeeId { get; set; }
        [Required]
        public string AccountId { get; set; }
    }
}