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
    public static string Resolve(IHostEnvironment environment)
    {
        // No Azure App Service a variável HOME aponta para o armazenamento
        // persistente (D:\home). Localmente ela normalmente não existe.
        var home = Environment.GetEnvironmentVariable("HOME");

        string dataDir = !string.IsNullOrWhiteSpace(home)
            ? Path.Combine(home, "data")
            : Path.Combine(environment.ContentRootPath, "App_Data");

        Directory.CreateDirectory(dataDir);

        var dbPath = Path.Combine(dataDir, "tdstecnologia2027.db");
        return $"Data Source={dbPath}";
    }
}
