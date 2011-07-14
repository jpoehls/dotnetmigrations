namespace DotNetMigrations.Core
{
	public enum MigrationTransactionMode
	{
		/// <summary>
		/// Wraps all migrations in a single transaction.
		/// </summary>
		PerRun, 
		/// <summary>
		/// Wraps each individual migration in a transaction.
		/// </summary>
		PerMigration,
		/// <summary>
		/// Does not wrap migrations in any transactions at all.
		/// </summary>
		None
	}
}