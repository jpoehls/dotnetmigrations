using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace DotNetMigrations.Core
{
    public abstract class CommandArguments : IArguments
    {
        private readonly List<string> _errors;

        protected CommandArguments()
        {
            _errors = new List<string>();
        }

        public IEnumerable<string> Errors
        {
            get { return _errors; }
        }

        public bool IsValid
        {
            get { return _errors.Count() == 0; }
        }

        private void Validate(IEnumerable<PropertyInfo> properties)
        {
            _errors.Clear();

            foreach (PropertyInfo prop in properties)
            {
                IEnumerable<ValidationAttribute> validationAttrs =
                    prop.GetCustomAttributes(typeof(ValidationAttribute), true)
                        .OfType<ValidationAttribute>();

                foreach (ValidationAttribute attr in validationAttrs)
                {
                    if (!attr.IsValid(prop.GetValue(this, null)))
                    {
                        AddErrorMessage(attr.ErrorMessage);
                    }
                }
            }
        }

        private void AddErrorMessage(string errorMessage)
        {
            _errors.Add(errorMessage);
        }

        public void Parse(ArgumentSet args)
        {
            Dictionary<PropertyInfo, ArgumentAttribute> props = GetArgumentProperties(GetType());

            //  set property values for anonymous arguments
            AssignAnonymousArguments(args.AnonymousArgs, props);

            //  set property values for each named argument
            AssignNamedArguments(args.NamedArgs, props);

            Validate(props.Keys);
        }

        /// <summary>
        /// Gets all properties that have ArgumentAttribute's and
        /// returns then in a dictionary.
        /// </summary>
        public static Dictionary<PropertyInfo, ArgumentAttribute> GetArgumentProperties(Type type)
        {
            Dictionary<PropertyInfo, ArgumentAttribute> p = type.GetProperties(BindingFlags.Public |
                                                                                    BindingFlags.Instance)
                .Select(x => new
                                 {
                                     Property = x,
                                     Attribute =
                                 (ArgumentAttribute)x.GetCustomAttributes(typeof(ArgumentAttribute), false)
                                                         .FirstOrDefault()
                                 })
                .Where(x => x.Attribute != null)
                .ToDictionary(x => x.Property, x => x.Attribute);
            return p;
        }

        private void AssignAnonymousArguments(IList<string> anonymousArgs,
                                              Dictionary<PropertyInfo, ArgumentAttribute> properties)
        {
            for (int i = 0; i < anonymousArgs.Count; i++)
            {
                int argumentPosition = i + 1;
                PropertyInfo matchingProp = properties
                    .Where(x => x.Value.Position == argumentPosition)
                    .Select(x => x.Key)
                    .FirstOrDefault();

                string value = anonymousArgs[i];
                SetPropertyValue(matchingProp, value);
            }
        }

        private void AssignNamedArguments(Dictionary<string, string> namedArgs,
                                          Dictionary<PropertyInfo, ArgumentAttribute> properties)
        {
            foreach (string key in namedArgs.Keys)
            {
                string argumentName = key;
                PropertyInfo matchingProp = properties
                    .Where(x => string.Equals(x.Value.Name, argumentName, StringComparison.OrdinalIgnoreCase)
                                || string.Equals(x.Value.ShortName, argumentName, StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.Key)
                    .FirstOrDefault();

                string value = namedArgs[key];
                SetPropertyValue(matchingProp, value);
            }
        }

        private void SetPropertyValue(PropertyInfo property, string value)
        {
            if (property != null)
            {
                if (property.PropertyType == typeof(string))
                {
                    property.SetValue(this, value, null);
                }
                else
                {
                    property.SetValue(this, Convert.ChangeType(value, property.PropertyType), null);
                }
            }
        }
    }
}