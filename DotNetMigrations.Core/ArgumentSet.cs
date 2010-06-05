using System;
using System.Collections.Generic;

namespace DotNetMigrations.Core
{
    public class ArgumentSet
    {
        /// <summary>
        /// Pipe delimited list of prefixes that
        /// distinquish a name from a value argument.
        /// </summary>
        public const string NamePrefixes = "-|/";

        private ArgumentSet()
        {
            NamedArgs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            AnonymousArgs = new List<string>();
        }

        public Dictionary<string, string> NamedArgs { get; private set; }
        public IList<string> AnonymousArgs { get; private set; }

        /// <summary>
        /// Parses an array of arguments into
        /// collections of named and anonymous arguments.
        /// </summary>
        public static ArgumentSet Parse(string[] args)
        {
            var set = new ArgumentSet();

            string name = null;
            for (int i = 0; i < args.Length; i++)
            {
                bool isName = IsName(args[i]);
                bool added = false;
                if (name != null && !isName)
                {
                    set.NamedArgs.Add(name, args[i]);
                    added = true;
                }
                else if (name != null)
                {
                    set.NamedArgs.Add(name, null);
                    added = true;
                }

                if (added)
                {
                    name = null;
                }

                if (isName)
                {
                    name = GetName(args[i]);
                }
                else if (!added)
                {
                    set.AnonymousArgs.Add(args[i]);
                }
            }

            if (name != null)
            {
                set.NamedArgs.Add(name, null);
            }

            return set;
        }

        /// <summary>
        /// Returns True/False whether the given
        /// argument is a name or not.
        /// </summary>
        private static bool IsName(string arg)
        {
            foreach (string prefix in NamePrefixes.Split('|'))
            {
                if (arg.StartsWith(prefix) && arg.Length > prefix.Length)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the name without the prefix.
        /// </summary>
        private static string GetName(string arg)
        {
            foreach (string prefix in NamePrefixes.Split('|'))
            {
                if (arg.StartsWith(prefix) && arg.Length > prefix.Length)
                {
                    return arg.Substring(prefix.Length);
                }
            }

            return null;
        }
    }
}