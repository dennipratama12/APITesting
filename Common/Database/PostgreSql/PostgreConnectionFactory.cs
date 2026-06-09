using APITesting.Common.Constants;
using Npgsql;

namespace APITesting.Common.Database.PostgreSql;

public interface IPostgreConnectionFactory
{
    NpgsqlConnection CreateConnection(string? databaseKey = null);
}

public sealed class PostgreConnectionFactory(IConfiguration configuration) : IPostgreConnectionFactory
{
    public NpgsqlConnection CreateConnection(string? databaseKey = null)
    {
        var selectedKey = string.IsNullOrWhiteSpace(databaseKey)
            ? GlobalParams.Database.Main
            : databaseKey;

        var connectionString = configuration.GetConnectionString(selectedKey);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(GlobalParams.Errors.ConnectionNotConfigured(selectedKey));
        }

        return new NpgsqlConnection(connectionString);
    }
}
