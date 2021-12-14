using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;
using Tolitech.CodeGenerator.Logging.Database.Tests.Providers;

namespace Tolitech.CodeGenerator.Logging.Database.Tests
{
    public class LoggingDatabaseTest
    {
        private readonly ILogger<LoggingDatabaseTest> _logger;

        public LoggingDatabaseTest()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .Build();

            var logLevel = (LogLevel)config.GetSection("Logging:Database:LogLevel").GetValue(typeof(LogLevel), "Default");

            var loggerFactory = LoggerFactory.Create(logger =>
            {
                logger
                    .AddConfiguration(config.GetSection("Logging"))
                    .AddDatabaseLogger(x =>
                    {
                        x.LogLevel = logLevel;
                    });

                var options = new DatabaseLoggerOptions()
                {
                    LogLevel = logLevel,
                    ConnectionString = ""
                };

                var provider = new LogDatabaseProvider(options);
                logger.AddProvider(provider);
            });

            _logger = loggerFactory.CreateLogger<LoggingDatabaseTest>();
        }

        [Fact(DisplayName = "LoggingDatabase - Log - Valid")]
        public void LoggingDatabase_Log_Valid()
        {
            _logger.LogInformation("test");

            bool b = _logger.IsEnabled(LogLevel.Trace);
            Assert.True(b);
        }

        [Fact(DisplayName = "LoggingDatabase - LogEntry - Valid")]
        public void LoggingDatabase_LogEntry_Valid()
        {
            var options = new DatabaseLoggerOptions()
            {
                LogLevel = LogLevel.Trace,
                ConnectionString = ""
            };

            var stateProperties = new Dictionary<string, object>();
            stateProperties.Add("text1", "text1");
            stateProperties.Add("text2", "text2");

            var scopes = new List<LogScopeInfo>();
            scopes.Add(new LogScopeInfo() { Text = "text", Properties = stateProperties });

            var filePath = new List<string>();
            filePath.Add("path1");
            filePath.Add("path2");

            var provider = new LogDatabaseProvider(options);
            var logEntry = new LogEntry()
            {
                Text = "text",
                StateProperties = stateProperties,
                Scopes = scopes,
                FilePath = filePath
            };

            provider.WriteLog(logEntry);

            bool b = provider.IsEnabled(LogLevel.Trace);
            Assert.True(b);
        }
    }
}
