using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DotNetMigrations.Core.Data
{
    public static class SqlParser
    {
        /// <summary>
        /// Splits SQL into chunks by the GO keyword.
        /// </summary>
        public static IEnumerable<string> SplitByGoKeyword(string sql)
        {
            if (sql == null || sql.Trim().Length == 0)
                return Enumerable.Empty<string>();

            var chunks = new List<string>();
            var currentChunk = new StringBuilder();

            using (var reader = new StringReader(sql))
            {
                var line = reader.ReadLine();
                while (line != null)
                {
                    if (line.Trim().ToUpper() == "GO")
                    {
                        if (currentChunk.Length > 0)
                        {
                            chunks.Add(currentChunk.ToString());
                            currentChunk.Remove(0, currentChunk.Length);
                        }
                    }
                    else
                    {
                        currentChunk.AppendLine(line);
                    }

                    line = reader.ReadLine();
                }
            }

            return chunks;
        }
    }
}