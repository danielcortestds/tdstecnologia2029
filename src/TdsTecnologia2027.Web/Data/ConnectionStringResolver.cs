namespace TdsTecnologia2027.Web.Data;

/// <summary>
/// Resolve a connection string do MySQL conforme o ambiente:
///   1) Desenvolvimento local  → ConnectionStrings:DefaultConnection (appsettings.json)
///   2) Azure "MySQL no Aplicativo" → variável MYSQLCONNSTR_localdb injetada pelo Azure
///
/// O MySQL no Aplicativo expõe a string no formato:
///   "Database=localdb;Data Source=127.0.0.1:PORTA;User Id=azure;Password=xxxxx"
/// que precisa ser convertido para o formato esperado pelo Pomelo/MySqlConnector
/// ("Server=...;Port=...;Database=...;User=...;Password=...").
/// </summary>
public static class ConnectionStringResolver
{
    public static string Resolve(IConfiguration configuration)
    {
        // 1) Connection string explícita do appsettings (dev local) — já no formato correto.
        var direct = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(direct))
            return direct;

        // 2) MySQL no Aplicativo — Azure injeta MYSQLCONNSTR_localdb.
        //    O provedor de configuração do ASP.NET Core mapeia MYSQLCONNSTR_localdb
        //    para ConnectionStrings:localdb automaticamente.
        var azure = configuration.GetConnectionString("localdb")
                    ?? Environment.GetEnvironmentVariable("MYSQLCONNSTR_localdb");
        if (!string.IsNullOrWhiteSpace(azure))
            return ConvertMySqlInApp(azure);

        throw new InvalidOperationException(
            "Nenhuma connection string MySQL encontrada (DefaultConnection ou MYSQLCONNSTR_localdb).");
    }

    /// <summary>
    /// Devolve a connection string com a senha substituída por *** para log seguro.
    /// </summary>
    public static string Mask(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return "(vazia)";

        var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        for (var i = 0; i < parts.Length; i++)
        {
            var kv = parts[i].Split('=', 2);
            if (kv.Length == 2)
            {
                var key = kv[0].Trim().ToLowerInvariant();
                if (key is "password" or "pwd")
                    parts[i] = $"{kv[0]}=***";
            }
        }
        return string.Join(";", parts);
    }

    private static string ConvertMySqlInApp(string raw)
    {
        string database = "", server = "127.0.0.1", port = "3306", user = "", password = "";

        foreach (var part in raw.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var kv = part.Split('=', 2);
            if (kv.Length != 2) continue;

            var key = kv[0].Trim().ToLowerInvariant();
            var value = kv[1].Trim();

            switch (key)
            {
                case "database":
                case "initial catalog":
                    database = value;
                    break;

                case "server":
                case "host":
                case "data source":
                case "datasource":
                case "address":
                    // Pode vir como "127.0.0.1:PORTA" — separa host e porta.
                    var hostPort = value.Split(':', 2);
                    server = hostPort[0];
                    if (hostPort.Length == 2 && !string.IsNullOrWhiteSpace(hostPort[1]))
                        port = hostPort[1];
                    break;

                case "port":
                    port = value;
                    break;

                case "user id":
                case "uid":
                case "user":
                case "username":
                    user = value;
                    break;

                case "password":
                case "pwd":
                    password = value;
                    break;
            }
        }

        return $"Server={server};Port={port};Database={database};User={user};Password={password};" +
               "AllowPublicKeyRetrieval=True;SslMode=None;";
    }
}
