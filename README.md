# Logging.Database
Logging database library used in projects created by the Code Generator tool.

This project contains abstract classes for implementing and customizing logging database providers. 

Tolitech Code Generator Tool: [http://www.tolitech.com.br](https://www.tolitech.com.br/)

Examples:

```
[ProviderAlias("Database")]
public class SqlServerDatabaseLoggerProvider : DatabaseLoggerProvider
{
	public SqlServerDatabaseLoggerProvider(IOptionsMonitor<DatabaseLoggerOptions> Settings) : base(Settings.CurrentValue)
{

}

public SqlServerDatabaseLoggerProvider(DatabaseLoggerOptions Settings) : base(Settings)
{

}

protected override DbConnection GetNewConnection
{
	get
	{
		return new SqlConnection(Settings.ConnectionString);
	}
}

protected override string Sql
{
	get
	{
		return "insert into [Cg].[Log] (logId, time, hostName, text) values (@logId, @time, @hostName, @text)";
	}
}
```