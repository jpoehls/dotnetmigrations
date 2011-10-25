using System.Collections.Generic;
using DotNetMigrations.Core;

namespace DotNetMigrations.Migrations
{
	public interface IMigrationDirectory
	{
		/// <summary>
		/// Provides a way of getting and setting the path to place the migration scripts.
		/// </summary>
		string Path { get; set; }

		/// <summary>
		/// Returns the migration script path from the
		/// config file (if available) or the default path.
		/// </summary>
		string GetPath(ILogger log);

		/// <summary>
		/// Returns a list of all the migration script file paths
		/// sorted by version number (ascending).
		/// </summary>
		IEnumerable<IMigrationScriptFile> GetScripts();

		/// <summary>
		/// Creates a blank migration script with the given name.
		/// </summary>
		/// <param name="migrationName">name of the migration script</param>
		/// <returns>The path to the new migration script.</returns>
		string CreateBlankScript(string migrationName);
	}
}