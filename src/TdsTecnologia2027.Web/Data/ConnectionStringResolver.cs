namespace TdsTecnologia2027.Web.Data;

/// <summary>
/// Resolve o caminho do arquivo de banco SQLite conforme o ambiente:
///   1) Azure App Service → usa a pasta persistente %HOME%\data
///      (D:\home\data no Windows). Esse diretório SOBREVIVE a deploys e
///      reinicializações, então os dados não se perdem.
///   2) Desenvolvimento local → usa a pasta App_Data dentro do projeto.
///
/// Em ambos os casos a pasta é criada se ainda não existir.
/// </summary>
public static class ConnectionStringResolver
{
    public const string DbFileName = "tdstecnologia2027.db";

    /// <summary>Pasta persistente onde ficam o banco e os materiais.</summary>
    public static string DataDir(IHostEnvironment environment)
    {
        // No Azure App Service a variável HOME aponta para o armazenamento
        // persistente (D:\home). Localmente ela normalmente não existe.
        var home = Environment.GetEnvironmentVariable("HOME");

        string dataDir = !string.IsNullOrWhiteSpace(home)
            ? Path.Combine(home, "data")
            : Path.Combine(environment.ContentRootPath, "App_Data");

        Directory.CreateDirectory(dataDir);
        return dataDir;
    }

    public static string Resolve(IHostEnvironment environment)
    {
        var dbPath = Path.Combine(DataDir(environment), DbFileName);
        return $"Data Source={dbPath}";
    }
}
