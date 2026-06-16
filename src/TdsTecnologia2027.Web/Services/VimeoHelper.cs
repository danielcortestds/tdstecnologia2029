using System.Text.RegularExpressions;

namespace TdsTecnologia2027.Web.Services;

/// <summary>
/// Converte URLs do Vimeo (em qualquer formato) para a URL de incorporação do player.
/// Aceita, entre outros:
///   https://vimeo.com/123456789
///   https://vimeo.com/123456789/abcdef0123              (vídeo privado, com hash)
///   https://vimeo.com/video/123456789
///   https://vimeo.com/manage/videos/123456789/abcdef    (link da tela de gerência)
///   https://player.vimeo.com/video/123456789?h=abcdef
///   código &lt;iframe ...&gt; colado inteiro
/// </summary>
public static class VimeoHelper
{
    /// <summary>
    /// Devolve a URL de incorporação (player.vimeo.com/video/ID[?h=HASH]) ou null
    /// se nenhum ID de vídeo for encontrado.
    /// </summary>
    public static string? ObterUrlEmbed(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        url = url.Trim();

        // 1) ID do vídeo = primeira sequência de 6+ dígitos.
        var idMatch = Regex.Match(url, @"\d{6,}");
        if (!idMatch.Success) return null;
        var id = idMatch.Value;

        // 2) Hash de privacidade: como parâmetro (?h=/&h=) ou como segmento após o ID.
        string? hash = null;
        var hParam = Regex.Match(url, @"[?&]h=([0-9a-zA-Z]+)");
        if (hParam.Success)
        {
            hash = hParam.Groups[1].Value;
        }
        else
        {
            var pathHash = Regex.Match(url, Regex.Escape(id) + @"/([0-9a-zA-Z]+)");
            if (pathHash.Success) hash = pathHash.Groups[1].Value;
        }

        var embed = $"https://player.vimeo.com/video/{id}";
        if (!string.IsNullOrEmpty(hash))
            embed += $"?h={hash}";
        return embed;
    }
}
