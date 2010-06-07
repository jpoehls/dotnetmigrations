using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DotNetMigrations.Core;

namespace DotNetMigrations.Commands
{
    internal class GenerateScriptCommandArgs : CommandArguments
    {
        [Required(ErrorMessage = "-name is required")]
        [Argument("name", "n", "Name of the migration script to generate",
            Position = 1,
            ValueName = "migration_name")]
        public string MigrationName { get; set; }
    }
}