using MathieuShop.Avalonia.Services;
using Npgsql;

try
{
    var projectRoot = ResolveProjectRoot(args);
    var appProjectDirectory = Path.Combine(projectRoot, "MathieuShop.Avalonia");
    var options = AppOptionsLoader.Load(appProjectDirectory) with { FallbackToInMemory = false };

    Console.WriteLine("Настройка PostgreSQL для MathieuShop...");
    Console.WriteLine($"Файл конфигурации: {Path.Combine(appProjectDirectory, "appsettings.json")}");

    EnsureDatabaseExists(options.PostgreSqlConnectionString);

    using var context = DatabaseBootstrapper.CreateContext(options).Context;
    Console.WriteLine("База PostgreSQL готова: миграции применены, демо-данные добавлены.");
    return;
}
catch (Exception exception)
{
    Console.Error.WriteLine("Ошибка настройки PostgreSQL:");
    Console.Error.WriteLine(exception.Message);
    Environment.ExitCode = 1;
}

static string ResolveProjectRoot(string[] args)
{
    if (args.Length > 0 && Directory.Exists(args[0]))
    {
        return Path.GetFullPath(args[0]);
    }

    return Directory.GetCurrentDirectory();
}

static void EnsureDatabaseExists(string connectionString)
{
    var targetBuilder = new NpgsqlConnectionStringBuilder(connectionString);
    if (string.IsNullOrWhiteSpace(targetBuilder.Database))
    {
        throw new InvalidOperationException("В строке подключения не указано имя базы данных.");
    }

    var databaseName = targetBuilder.Database;
    var adminBuilder = new NpgsqlConnectionStringBuilder(connectionString)
    {
        Database = "postgres",
        Pooling = false
    };

    using var connection = new NpgsqlConnection(adminBuilder.ConnectionString);
    connection.Open();

    using var checkCommand = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @dbName;", connection);
    checkCommand.Parameters.AddWithValue("dbName", databaseName);

    if (checkCommand.ExecuteScalar() is not null)
    {
        Console.WriteLine($"База \"{databaseName}\" уже существует.");
        return;
    }

    using var createCommand = new NpgsqlCommand($"CREATE DATABASE {QuoteIdentifier(databaseName)};", connection);
    createCommand.ExecuteNonQuery();
    Console.WriteLine($"База \"{databaseName}\" создана.");
}

static string QuoteIdentifier(string value)
{
    return "\"" + value.Replace("\"", "\"\"") + "\"";
}
