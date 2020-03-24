using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Attributes
{
    public class DateMustNotBeLessThan : ValidationAttribute, IClientModelValidator
    {
        private readonly string _dateProperty;

        public DateMustNotBeLessThan(string dateProperty)
        {
            _dateProperty = dateProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(_dateProperty);
            var dateValue = (DateTime)property.GetValue(validationContext.ObjectInstance);

            if ((dateValue).Subtract((DateTime)value).Days > 0)
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
            return $"{name} must not be less than Start Date.";
        }
    }
}