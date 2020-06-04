using System;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace Tolitech.CodeGenerator.Logging.Database
{
    internal class DatabaseLoggerOptionsSetup : ConfigureFromConfigurationOptions<DatabaseLoggerOptions>
    {
        public DatabaseLoggerOptionsSetup(ILoggerProviderConfiguration<DatabaseLoggerProvider> providerConfiguration) : base(providerConfiguration.Configuration)
        {

        }
    }
}
