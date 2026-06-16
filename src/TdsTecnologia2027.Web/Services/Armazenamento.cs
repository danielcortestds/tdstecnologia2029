namespace TdsTecnologia2027.Web.Services;

/// <summary>
/// Resolve a pasta de armazenamento de arquivos enviados (materiais de apoio),
/// usando o mesmo diretório persistente do banco: %HOME%\data no Azure
/// (sobrevive a deploys) ou App_Data localmente.
/// </summary>
public static class Armazenamento
{
    public static string PastaMateriais(IHostEnvironment env)
    {
        var home = Environment.GetEnvironmentVariable("HOME");
        var baseDir = !string.IsNullOrWhiteSpace(home)
            ? Path.Combine(home, "data")
            : Path.Combine(env.ContentRootPath, "App_Data");

        var dir = Path.Combine(baseDir, "materiais");
        Directory.CreateDirectory(dir);
        return dir;
    }

    public static string CaminhoArquivo(IHostEnvironment env, string nomeFisico)
        => Path.Combine(PastaMateriais(env), nomeFisico);
}
