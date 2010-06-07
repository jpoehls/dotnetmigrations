using System;

namespace DotNetMigrations.Core
{
    public class ArgumentAttribute : Attribute
    {
        public ArgumentAttribute(string name, string shortName, string description)
        {
            Name = name;
            ShortName = shortName;
            Description = description;
            Position = int.MaxValue;
        }

        /// <summary>
        /// Verbose name that is more intuitive for scripting and help text.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Abbreviated name that is friendly for the command line.
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// Used in the syntax help text as the placeholder for the value.
        /// </summary>
        public string ValueName { get; set; }

        /// <summary>
        /// If the argument is not passed in with a name then it will
        /// be matched based on its position in the argument array.
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Description of this argument to show in the help text.
        /// </summary>
        public string Description { get; set; }
    }
}