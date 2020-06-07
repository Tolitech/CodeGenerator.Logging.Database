using System;
using Microsoft.Extensions.Logging;

namespace Tolitech.CodeGenerator.Logging.Database
{
    public class DatabaseLoggerOptions
    {
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
    }
}
