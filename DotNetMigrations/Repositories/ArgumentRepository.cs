using System.Collections.Generic;
using DotNetMigrations.Core;

namespace DotNetMigrations.Repositories
{
    internal class ArgumentRepository : IArgumentRepository
    {
        /// <summary>
        /// A list of command line arguments used by commands.
        /// </summary>
        public List<string> Arguments { get; set; }

        /// <summary>
        /// Retrieves the number of arguments in the repository.
        /// </summary>
        /// <returns>The number of arguments in teh repository.</returns>
        public int Count
        {
            get { return Arguments.Count; }
        }

        /// <summary>
        /// Instantiates a new instance of the ArgumentRepository class.
        /// </summary>
        /// <param name="args"></param>
        internal ArgumentRepository(IEnumerable<string> args)
        {
            Arguments = new List<string>(args);
        }

        /// <summary>
        /// Checks to see if a specific string is contained with in the arguments collection.
        /// </summary>
        /// <param name="arg">The argument to search for.</param>
        /// <returns>True if the argument exists in the collection; else false.</returns>
        public bool HasArgument(string arg)
        {
            return Arguments.IndexOf(arg) >= 0;
        }

        /// <summary>
        /// Retrieves an argument from the collection
        /// </summary>
        /// <param name="index">The index of the argument to retrieve</param>
        /// <returns>The specified argument.</returns>
        public string GetArgument(int index)
        {
            return Arguments[index];
        }

        /// <summary>
        /// Retrieves the last argument of the collection
        /// </summary>
        /// <returns>The last argument of the collection</returns>
        public string GetLastArgument()
        {
            return Arguments[Arguments.Count - 1];
        }
    }
}
