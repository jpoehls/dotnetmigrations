using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DotNetMigrations.Core
{
    public class DatabaseCommandArguments : CommandArguments
    {
        [Required(ErrorMessage = "-connection is required")]
        [Argument("connection", "c", "Connection string to use, or the name of the connection from app.config to use.",
            Position = 1)]
        public string Connection { get; set; }
    }
}