See https://github.com/jpoehls/dotnetmigrations for full documentation and available commands.

To use DotNetMigrations (DNM) effectively as a installed NuGet package with a project, open a command line 
in the root of your project (where your app.config/web.config is) and run db.exe to perform commands using
a relative path e.g.

	..\..\packages\DotNetMigrations.0.85.0\tools\db.exe generate "Initial setup"

If you use it in this manner, db.exe will use YOUR project's app.config or web.config and corresponding
connection strings, eliminating the need to duplicate connection strings in two locations and keeping your
settings where you want them.