using System;
using DotNetMigrations.Core;

namespace DotNetMigrations.UnitTests.Mocks
{
    internal class MockDatabaseCommand1 : DatabaseCommandBase
    {
        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}