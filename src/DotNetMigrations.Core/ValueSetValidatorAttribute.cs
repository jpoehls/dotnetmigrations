using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DotNetMigrations.Core
{
    /// <summary>
    /// Validates that the given value matches one of the valid values in the set.
    /// </summary>
    public class ValueSetValidatorAttribute : ValidationAttribute
    {
        public string[] ValidValues { get; private set; }

        public ValueSetValidatorAttribute(params string[] validValues)
        {
            ValidValues = validValues;
        }

        public override bool IsValid(object value)
        {
            var sValue = (value != null) ? value.ToString() : string.Empty;
            return ValidValues.Contains(sValue, StringComparer.OrdinalIgnoreCase);
        }
    }
}