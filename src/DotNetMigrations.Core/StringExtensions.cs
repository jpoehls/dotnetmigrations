using System;
using System.Linq;

namespace DotNetMigrations.Core
{
    public static class StringExtensions
    {
        public static string Replace(this string input, string oldValue, string newValue, StringComparison comparisonType)
        {
            var index = 0;
            while ((index = input.IndexOf(oldValue, index, comparisonType)) > -1)
            {
                // out with the old
                input = input.Remove(index, oldValue.Length);

                // in with the new
                input = input.Insert(index, newValue);

                // start looking for the next occurance after the
                // replacement value that we just inserted
                index += newValue.Length;
            }

            return input;
        }
    }
}
