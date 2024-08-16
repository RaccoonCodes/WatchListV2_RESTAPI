﻿using System.ComponentModel.DataAnnotations;

namespace WatchListV2.Attribute
{
    //Validation for choosing predefined values
    public class SortOrderValidatorAttribute : ValidationAttribute
    {
        public string[] AllowedValues { get; set; } = new[] { "ASC", "DESC" };
        public SortOrderValidatorAttribute():base ("Values must be one of the following {0}") { }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var strValue = value as string;
            if (!string.IsNullOrEmpty(strValue) && AllowedValues.Contains(strValue)) 
            { 
                return ValidationResult.Success;
            }
            return new ValidationResult(FormatErrorMessage(string.Join(", ", AllowedValues)));
        }
    }
}