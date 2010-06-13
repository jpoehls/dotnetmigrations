using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetMigrations.Core
{
    public class ArgumentSet
    {
        /// <summary>
        /// Pipe delimited list of prefixes that
        /// distinquish a name from a value argument.
        /// </summary>
        public const string NamePrefixes = "-|/";

        private readonly List<string> _anonymousArgs;
        private readonly Dictionary<string, string> _namedArgs;

        private ArgumentSet()
        {
            _namedArgs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _anonymousArgs = new List<string>();
        }

        public IEnumerable<KeyValuePair<string, string>> NamedArgs
        {
            get { return _namedArgs; }
        }

        public IEnumerable<string> AnonymousArgs
        {
            get { return _anonymousArgs; }
        }

        /// <summary>
        /// Returns True/False whether this set contains
        /// and argument with the given name.
        /// </summary>
        public bool ContainsName(string name)
        {
            return _namedArgs.ContainsKey(name);
        }

        /// <summary>
        /// Returns the value of the argument
        /// with the given name.
        /// </summary>
        public string GetByName(string name)
        {
            return _namedArgs[name];
        }

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
                    set._namedArgs.Add(name, args[i]);
                    added = true;
                }
                else if (name != null)
                {
                    set._namedArgs.Add(name, null);
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
                    set._anonymousArgs.Add(args[i]);
                }
            }

            if (name != null)
            {
                set._namedArgs.Add(name, null);
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