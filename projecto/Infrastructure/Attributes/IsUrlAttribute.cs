using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Projecto.Infrastructure.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class IsUrlAttribute : ValidationAttribute, IClientModelValidator
    {
        private string regex = @"^[a-zA-Z0-9]+(-[a-zA-Z0-9]+)*$";

        public void AddValidation(ClientModelValidationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            context.Attributes["data-val"] = "true";
            context.Attributes["data-val-regex"] = $"{context.ModelMetadata.DisplayName} is invalid.";
            context.Attributes["data-val-regex-pattern"] = regex;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string url = value as string;
            Regex r = new Regex(regex);
            if (r.IsMatch(url))
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(ErrorMessage);
        }
    }
}
