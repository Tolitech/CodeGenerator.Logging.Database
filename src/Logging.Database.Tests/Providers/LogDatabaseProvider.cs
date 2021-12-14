using System;
using System.Data.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Tolitech.CodeGenerator.Logging.Database.Tests.Providers
{
    public class LogDatabaseProvider : DatabaseLoggerProvider
    {
        public LogDatabaseProvider(DatabaseLoggerOptions settings) : base(settings)
        {

        }

        public LogDatabaseProvider(IOptionsMonitor<DatabaseLoggerOptions> settings) : this(settings.CurrentValue)
        {

        }

        protected override DbConnection GetNewConnection => throw new NotImplementedException();

        protected override string Sql
        {
            get { return ""; }
        }
    }
}
