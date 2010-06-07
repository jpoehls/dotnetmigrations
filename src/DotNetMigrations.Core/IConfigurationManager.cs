using System;
using System.Collections.Specialized;
using System.Configuration;

namespace DotNetMigrations.Core
{
    public interface IConfigurationManager
    {
        NameValueCollection AppSettings { get; }
        ConnectionStringSettingsCollection ConnectionStrings { get; }
    }
}