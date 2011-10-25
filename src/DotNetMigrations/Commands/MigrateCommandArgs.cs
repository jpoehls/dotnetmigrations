using DotNetMigrations.Core;

namespace DotNetMigrations.Commands
{
	public class MigrateCommandArgs : DatabaseCommandArguments
	{
		public MigrateCommandArgs()
			: this(new ConfigurationManagerWrapper())
		{
		}

		public MigrateCommandArgs(IConfigurationManager configurationManager)
		{
			TargetVersion = -1;
			MigrationsPath = configurationManager.AppSettings[AppSettingKeys.MigrateFolder];
		}

		[Argument("version", "v", "Target version to migrate up or down to.",
			Position = 2)]
		public long TargetVersion { get; set; }

		[Argument("migrations", "m", "The path to the migration files.",
			Position = 3)]
		public string MigrationsPath { get; set; }
	}
}