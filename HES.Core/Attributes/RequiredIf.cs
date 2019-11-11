using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Attributes
{
    public class RequiredIf : ValidationAttribute, IClientModelValidator
    {
        private readonly string _skipProperty;

        public RequiredIf(string skipProperty)
        {
            _skipProperty = skipProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(_skipProperty);
            var skipValue = (bool)property.GetValue(validationContext.ObjectInstance);

            if (value == null && skipValue == false)
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

        //public void AddValidation(ClientModelValidationContext context)
        //{
        //    if (context == null)
        //    {
        //        throw new ArgumentNullException(nameof(context));
        //    }

        //    MergeAttribute(context.Attributes, "data-val", "true");
        //    MergeAttribute(context.Attributes, "data-val-required", GetErrorMessage(context.ModelMetadata.GetDisplayName()));
        //}

        //private bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        //{
        //    if (attributes.ContainsKey(key))
        //    {
        //        return false;
        //    }

        //    attributes.Add(key, value);
        //    return true;
        //}

        protected string GetErrorMessage(string name)
        {
            return $"The {name} field is required.";
        }
    }
}
