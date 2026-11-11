namespace TdsTecnologia2027.Web.Services;

/// <summary>
/// Resolve a pasta de armazenamento de arquivos enviados (materiais de apoio,
/// fundos de certificado), usando o mesmo diretório persistente do banco:
/// %HOME%\data no Azure (sobrevive a deploys) ou App_Data localmente.
/// </summary>
public static class Armazenamento
{
    private static string BaseDir(IHostEnvironment env)
    {
        var home = Environment.GetEnvironmentVariable("HOME");
        return !string.IsNullOrWhiteSpace(home)
            ? Path.Combine(home, "data")
            : Path.Combine(env.ContentRootPath, "App_Data");
    }

    public static string PastaMateriais(IHostEnvironment env)
    {
        var dir = Path.Combine(BaseDir(env), "materiais");
        Directory.CreateDirectory(dir);
        return dir;
    }

    public static string CaminhoArquivo(IHostEnvironment env, string nomeFisico)
        => Path.Combine(PastaMateriais(env), nomeFisico);

    /// <summary>Pasta persistente das imagens de fundo de certificado enviadas.</summary>
    public static string PastaCertificados(IHostEnvironment env)
    {
        var dir = Path.Combine(BaseDir(env), "certificados");
        Directory.CreateDirectory(dir);
        return dir;
    }

    public static string CaminhoCertificado(IHostEnvironment env, string nomeFisico)
        => Path.Combine(PastaCertificados(env), nomeFisico);
}
