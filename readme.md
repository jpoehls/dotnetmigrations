     _____        _   _   _      _   __  __ _                 _   _                 
    |  __ \      | | | \ | |    | | |  \/  (_)               | | (_)                
    | |  | | ___ | |_|  \| | ___| |_| \  / |_  __ _ _ __ __ _| |_ _  ___  _ __  ___ 
    | |  | |/ _ \| __| . ` |/ _ \ __| |\/| | |/ _` | '__/ _` | __| |/ _ \| '_ \/ __|
    | |__| | (_) | |_| |\  |  __/ |_| |  | | | (_| | | | (_| | |_| | (_) | | | \__ \
    |_____/ \___/ \__|_| \_|\___|\__|_|  |_|_|\__, |_|  \__,_|\__|_|\___/|_| |_|___/
                                               __/ |                                
                                              |___/                                 

**DotNetMigrations** - http://github.com/jpoehls/dotnetmigrations

DotNetMigrations is a database migration framework that assists in managing and versioning database changes. It was originally designed as a straight port of the rails functionality located in the Ruby on Rails framework; however, it
has since grown wings and taken its own path in several areas.

**View our CI builds on the [CodeBetter TeamCity server](http://teamcity.codebetter.com/project.html?projectId=project97]).**

**Available on NuGet at http://www.nuget.org/List/Packages/DotNetMigrations**

# Build prerequisites

- PowerShell 2.0
- .NET Framework 4.0
- Git must be installed and in your path

# Release checklist

1. Make sure the `$public_version` number is correct in the `.\build.ps1` script.
2. Make sure the changelog is up-to-date in the `.\readme.md` file.
3. Commit any changes up to this point.
4. Tag the release in Git with `git tag vX.Y.Z` where `X.Y.Z` is the `$public_version` from the build script.
5. Run `.\build.bat`.
6. Add the hash of the tagged release from Git to the changelog entry for the release.  
   Use this to get the hash: `git log -n1 -r "vX.Y.Z" --pretty=%H`
7. Commit the changelog update.
8. Look in the `.\@artifacts` folder for the goods.
9. Run the `.\@artifacts\PublishNuGetPackage.bat` script to publish to the NuGet Gallery.
10. Uploaded the zipped binaries from `.\@artifacts` to Github.
11. Spread the word!

## Contributors

`git log --format='%aN' | sort -u`

* Darren Cauthon
* James Eggers
* Joshua Poehls
* Kieran Benton

## Changelog

- **0.85** (2011-08-27) `6ff0a1887128dded5533441be4b1de4658e6e223`

    This releases fixes the breaks in versions 0.83 and 0.84.

    **WARNING!** You must manually modify your `[schema_migrations]` table
    in order for this release to run. You need to make the following modifications.

    1. DROP the primary key constraint from the `[schema_migrations].[version]` column.
    2. Run this SQL: `ALTER TABLE [schema_migrations] ADD [id] INT NOT NULL IDENTITY(1,1) CONSTRAINT [PK_schema_migrations] PRIMARY KEY`

    A new `[id]` column will be added and used to locate the max version number in the migration table.  
    This works because migrations are always inserted in sequential order so the IDs will also be in
    the correct order.

    This release also removes support for upgrading from the legacy `[schema_info]` table used by
    very early versions of DotNetMigrations. This really shouldn't matter to anyone at this point but if
    it does then just run an older version of DNM to perform the upgrade and then switch to the latest DNM release.

- **0.84** (2011-08-27) `4834af1d7a41be0175083afcca5231b8dcb1713f`

  * First release to be published to NuGet! (thanks Darren Cauthon!)

- **0.83** (2011-08-26) `a86b7318122251fe54930f2ebce4155b466ac1a5`
  
    **WARNING!** This release breaks support for the `uct_time` and `local_time` versioning strategies.
    Only `seq_num` will work. If you use one of the time based strategies wait for **v0.85** before upgrading.         

  * Added preliminary support for Mono. (Thanks James Eggers!)
  * Fixed issue #21 with migrations not working past version 9.
  * **Breaking change!** The `##DNM:PROVIDER##` token is now `/*DNM:PROVIDER*/`
    The new token format will help ensure that your SQL scripts will run without errors
    outside of DNM.
  * Added support for a `CommandTimeout` parameter in the connection strings.
    This value will be used as the `DbCommand.CommandTimeout` value when the migration scripts are executed.

    **Upgrade Notes:**  
    You must manually run the following SQL against your database to ensure issue #21 is fixed.

    `ALTER TABLE [schema_migrations] ALTER COLUMN [version] [int] NOT NULL`
    
    ***Note** that this is the change that will break the `utc_time` and `local_time` versioning strategies
    since timestamps are too large to fit into an `[int]` column. If you are not using `seq_num` then
    **DO NOT** run this alter command and your migrations should continue to execute like they did in
    the previous version.

- **0.82** (2011-04-12) `5efb6c28dfdfd4a43e3014985ae9d8a74e1cb5e0`

    * Added support for post migration actions that plugins can use
    to inject functionality that should run after a migration completes.
    (This was added to support the new PersistentMigrate [pmigrate] command
    in the dotnetmigrations-contrib project.)

- **0.81** (2011-04-11) `f762dce9614150a7e159238a8860ef9b76fb82d2`

  * New `seed` command that executes scripts in the `.\seeds\` folder.

  * New `setup` command that migrates a database to the latest version
    and executes scripts in the seed folder.  
    This is the same as running:

            > db migrate myConnection
            > db seed myConnection

- **0.80** (2011-03-30) `896915d7a75df1c4939fbcc4b01bc0efe3cbadf4`

  * New sequential number `seq_num` versioning strategy

  * Added support for a `##DNM:PROVIDER##` token in migration scripts that is
    replaced with the connection string's provider name.

  * Added support for migration scripts without placeholders. When the script
    doesn't have any `BEGIN_SETUP`, `END_SETUP`, `BEGIN_TEARDOWN`, or `END_TEARDOWN`
    tokens then the entire script is assumed to be a Setup with no Teardown.

- **0.70** (2010-08-28) `950f6d5a6ca99a175a4d176208d3cdd08f52dd80`

    This release brings yet another significant rewrite. Building
    on the MEF integration and innovations of 0.6, many improvements
    have been made to make writing commands easier and safer.

  * New 'connections' command that provides a command line interface
    for viewing, adding, editing and removing stored connection
    strings in the config file.

  * Revamped help system that provides detailed info on the
    usage of all commands, including custom commands.

  * Smarter, more robust parsing of migrations scripts that contain
    GO keywords. Special thanks to the Subtext project who we
    borrowed these improvements from!
  
  * New strongly-typed parsing of command line arguments with
    support DataAnnotation attributes for validation and
    automatic integration with the help system.
  
  * New CommandBase and DatabaseCommandBase classes for building
    custom commands.
    
  * Rewritten data access routines that wrap all bulk operations
    in transactions. Also much smarter database connection
    handling.
  
  * Beefed up unit tests suite with full coverage of all critical
    routines.

  * Changed unit tests to use SQL Server CE 3.5.1 instead of
    SQL Server Express. This means anyone can run the unit tests
    now without having to setup a database first.

  * Major updates to the build script to use Psake instead of NAnt
    to run the unit tests after compilation and improvements for
    integrating with TeamCity.
    
  * Lots of bug fixes.

- **0.60** (2010-01-28)

    This version has been completely rewritten from the ground up using
    .Net 3.5 and the Managed Extensibility Framework.
  
  * Completely new directory structure and codebase
  
  * MEF has been used to assist in the inner workings of the application
    as well as allow for new logs and commands to be created.
  
  * Unit Tests Project has been added - tests use the NUnit framework
  
  * An automated build file has been created for NAnt v0.86 b1
  
  * Core project has been created to allow for easy access to classes
    required for extending DotNetMigrations
  
  * **Breaking Change!** The BulkCopy command has been removed.
    The BulkCopy command has temporarily been removed from the bundled
    commands of the application. This command will be moved to the
    DotNetMigrations-Contrib project as soon as it's ready. If you use
    the bulkcopy command, please continue using v0.5.
    
    **About the DotNetMigrations-Contrib Project**  
    Thanks to the Managed Extensibility Framework, DotNetMigrations is
    now able to have new commands and logging mechanisms to be created
    and added by anyone who wants. Because of this, the
    DotNetMigrations-Contrib project is being launched in the very near
    future (target launch date is Feb 01, 2010). The goal of this sister
    project is to provide a location for people to share new commands
    and logs to the application without having to worry about getting a
    different version of the core DotNetMigrations application from
    this project.

- **0.50** (2008-05-21)

  * fixed bug where blank lines before a `GO` in the migration script would
    cause an exception to be thrown

  * refactored the code for each 'command' into individual classes that inherit
    from a base `ConsoleCommand` class.

  * fixed bug that caused a sql error to be thrown whenever the `schema_info`
    table did not exist

  * added the `bulkload` command

  * added return codes to the application

- **0.40** (2008-05-08)

  * fixed bug where lines in the SQL migration script were being
    concatenated together without any whitespace to separate them

- **0.30**
  * uses DbProviderFactories to perform all database connections
    default provider is `System.Data.SqlClient`
    you can specify a specific provider by adding a `PROVIDER=` setting
    to your connection string
    ex. `PROVIDER=System.Data.SqlClient;SERVER=(local);DATABASE=TEST123`

  * misc updates made to specifically support SQL Compact 3.5 databases

- **0.20**
  * added the `version` command

- **0.10**
  * initial release!

## License

    Copyright (c) 2008-2011, Joshua Poehls
    All rights reserved.

    Redistribution and use in source and binary forms, with or without modification,
    are permitted provided that the following conditions are met:

      * Redistributions of source code must retain the above copyright notice,
        this list of conditions and the following disclaimer.

      * Redistributions in binary form must reproduce the above copyright notice,
        this list of conditions and the following disclaimer in the documentation
        and/or other materials provided with the distribution.

      * Neither the name of DotNetMigrations nor the names of its contributors
        may be used to endorse or promote products derived from this software
        without specific prior written permission.

    THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
    ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
    WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
    IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
    INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
    BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
    DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY
    OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE
    OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
    OF THE POSSIBILITY OF SUCH DAMAGE.


    ==================================================================================
    Portions of the source were taken from the Subtext project and are covered
    under the following license. These portions also contain this license in the
    header of their source code files.
    ==================================================================================

    Copyright (c) 2005 - 2010, Phil Haack
    All rights reserved.

    Redistribution and use in source and binary forms, with or without modification, 
    are permitted provided that the following conditions are met:

        * Redistributions of source code must retain the above copyright notice, 
        this list of conditions and the following disclaimer.
        * Redistributions in binary form must reproduce the above copyright notice, 
        this list of conditions and the following disclaimer in the documentation 
        and/or other materials provided with the distribution.
        * Neither the name of the Subtext nor the names of its contributors 
        may be used to endorse or promote products derived from this software 
        without specific prior written permission.

    THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
    ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
    WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
    IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
    INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, 
    BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
    DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY 
    OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE 
    OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED 
    OF THE POSSIBILITY OF SUCH DAMAGE.