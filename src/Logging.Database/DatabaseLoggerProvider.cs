using System;
using System.Data.Common;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Tolitech.CodeGenerator.Logging.Database
{
    public abstract class DatabaseLoggerProvider : LoggerProvider
    {
        protected DbConnection? _connection;

        protected abstract DbConnection GetNewConnection { get; }

        protected abstract string Sql { get; }

        async Task WriteLogLine(LogEntry info)
        {
            string? scopeText = null;
            string? scopeProperties = null;
            string? stateProperties = null;
            string? filePath = null;

            if (info.Scopes != null && info.Scopes.Count > 0)
            {
                foreach (var scope in info.Scopes)
                {
                    if (!string.IsNullOrWhiteSpace(scope.Text))
                    {
                        if (!string.IsNullOrEmpty(scopeText))
                            scopeText += " | ";

                        scopeText += scope.Text;
                    }

                    if (scope.Properties != null)
                    {
                        foreach (var properties in scope.Properties)
                        {
                            if (!string.IsNullOrEmpty(scopeProperties))
                                scopeProperties += " | ";

                            scopeProperties += properties.Key + " = " + properties.Value;
                        }
                    }
                }
            }

            if (info.StateProperties != null && info.StateProperties.Count > 0)
            {
                foreach (var properties in info.StateProperties)
                {
                    if (!string.IsNullOrEmpty(stateProperties))
                        stateProperties += " | ";

                    stateProperties += properties.Key + " = " + properties.Value;
                }
            }

            if (info.FilePath != null)
            {
                for (int index = 0; index < info.FilePath.Count; index++)
                {
                    if (!string.IsNullOrEmpty(filePath))
                        filePath += "\n";

                    filePath += info.FilePath[index] + " (" + info.LineNumber?[index] + ")";
                }
            }

            try
            {
                string sql = Sql;

                object param = new
                {
                    LogId = Guid.NewGuid(),
                    Time = info.TimeStampUtc.ToLocalTime(),
                    info.UserName,
                    LogEntry.HostName,
                    info.Category,
                    Level = info.Level.ToString(),
                    info.Text,
                    Exception = info.Exception == null ? null : info.Exception.ToString(),
                    EventId = info.EventId.ToString(),
                    info.ActivityId,
                    info.UserId,
                    info.LoginName,
                    info.ActionId,
                    info.ActionName,
                    info.RequestId,
                    info.RequestPath,
                    filePath,
                    info.Sql,
                    info.Parameters,
                    info.StateText,
                    StateProperties = stateProperties,
                    ScopeText = string.IsNullOrEmpty(scopeText) ? null : scopeText,
                    ScopeProperties = scopeProperties
                };

                if (!string.IsNullOrEmpty(Settings.ConnectionString))
                {
                    using (var conn = GetNewConnection)
                    {
                        try
                        {
                            await conn.OpenAsync();
                            await conn.ExecuteAsync(sql, param);
                            await conn.CloseAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        finally
                        {
                            await conn.DisposeAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public DatabaseLoggerProvider(IOptionsMonitor<DatabaseLoggerOptions> Settings) : this(Settings.CurrentValue)
        {
            SettingsChangeToken = Settings.OnChange(settings => { this.Settings = settings; });
        }

        public DatabaseLoggerProvider(DatabaseLoggerOptions Settings)
        {
            this.Settings = Settings;
        }

        public override bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public override void WriteLog(LogEntry Info)
        {
            Task.Run(() => WriteLogLine(Info));
        }

        protected DatabaseLoggerOptions Settings { get; private set; }
    }
}
