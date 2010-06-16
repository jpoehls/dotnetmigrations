using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DotNetMigrations.Commands
{
    /// <summary>
    /// Validator for the Action argument in ConnectionsCommandArgs.
    /// </summary>
    internal class ConnectionsCommandArgsActionValidator : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var validValues = new[] {"list", "add", "set", "remove"};
            var sValue = (value != null) ? value.ToString() : string.Empty;
            return validValues.Contains(sValue, StringComparer.OrdinalIgnoreCase);
        }
    }
}