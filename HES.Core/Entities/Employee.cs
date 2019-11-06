using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class Employee
    {
        [Key]
        public string Id { get; set; }
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Remote(action: "VerifyEmail", controller: "Validation", AdditionalFields = "Id")]
        [EmailAddress]
        [Required]
        public string Email { get; set; }
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "Field is required.")]
        [Display(Name = "Department")]
        public string DepartmentId { get; set; }
        [Required(ErrorMessage = "Field is required.")]
        [Display(Name = "Position")]
        public string PositionId { get; set; }
        [Display(Name = "Last Seen")]
        public DateTime? LastSeen { get; set; }
        public List<Device> Devices { get; set; }

        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }
        [ForeignKey("PositionId")]
        public Position Position { get; set; }

        [NotMapped]
        [Display(Name = "Name")]
        public string FullName => $"{FirstName} {LastName}";
        [NotMapped]
        [Display(Name = "Company")]
        public string EmpCompany => Department?.Company?.Name;
        [NotMapped]
        [Display(Name = "Department")]
        public string EmpDepartment => Department?.Name;
        [NotMapped]
        public string CurrentDevice { get; set; }
    }
}