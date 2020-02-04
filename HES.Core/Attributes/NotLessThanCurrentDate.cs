using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Attributes
{
    public class NotLessThanCurrentDate : ValidationAttribute, IClientModelValidator
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if ((DateTime.UtcNow.Date).Subtract((DateTime)value).Days > 0)
            {
                return new ValidationResult(GetErrorMessage(validationContext.DisplayName));
            }

            return ValidationResult.Success;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-required", GetErrorMessage(context.ModelMetadata.GetDisplayName()));
        }

        protected string GetErrorMessage(string name)
        {
            return $"{name} must be at least current date.";
        }
    }
}