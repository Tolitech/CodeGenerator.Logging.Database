using System;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Tolitech.CodeGenerator.Logging.Database
{
    [ProviderAlias("Database")]
    public class DatabaseLoggerProvider : LoggerProvider
    {
        public delegate DbConnection GetConnectionDelegate();
        public static GetConnectionDelegate GetNewConnection = null;

        async Task WriteLogLine(LogEntry info)
        {
            string scopeText = "";
            string scopeProperties = "";
            string stateProperties = "";

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
                foreach(var properties in info.StateProperties)
                {
                    if (!string.IsNullOrEmpty(stateProperties))
                        stateProperties += " | ";

                    stateProperties += properties.Key + " = " + properties.Value;
                }
            }

            try
            {
                string sql  = "insert into [cg].[logging] " +
                    "(time, userName, hostName, category, level, text, exception, eventId, activityId, userId, loginName, actionId, actionName, requestId, requestPath, methodName, sql, sqlParam, stateText, stateProperties, scopeText, scopeProperties) " +
                    "values " +
                    "(@time, @userName, @hostName, @category, @level, @text, @exception, @eventId, @activityId, @userId, @loginName, @actionId, @actionName, @requestId, @requestPath, @methodName, @sql, @sqlParam, @stateText, @stateProperties, @scopeText, @scopeProperties)";

                object param = new
                {
                    Time = info.TimeStampUtc.ToLocalTime(),
                    info.UserName,
                    info.HostName,
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
                    info.MethodName,
                    info.Sql, 
                    info.SqlParam,
                    info.StateText,
                    StateProperties = stateProperties,
                    ScopeText = scopeText,
                    ScopeProperties = scopeProperties
                };

                using (var conn = GetNewConnection())
                {
                    await conn.OpenAsync();
                    await conn.ExecuteAsync(sql, param);
                    await conn.CloseAsync();
                    await conn.DisposeAsync();
                }
            }
            catch(Exception ex)
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
            bool Result = logLevel != LogLevel.None && this.Settings.LogLevel != LogLevel.None && Convert.ToInt32(logLevel) >= Convert.ToInt32(this.Settings.LogLevel);

            return Result;
        }

        public override void WriteLog(LogEntry Info)
        {
            Task.Run(() => WriteLogLine(Info));
        }

        internal DatabaseLoggerOptions Settings { get; private set; }
    }
}
