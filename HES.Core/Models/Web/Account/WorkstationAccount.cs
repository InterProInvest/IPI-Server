﻿using HES.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Web.Account
{
    public class WorkstationAccount
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string EmployeeId { get; set; }

        public WorkstationAccountType Type { get; set; }
    }
}