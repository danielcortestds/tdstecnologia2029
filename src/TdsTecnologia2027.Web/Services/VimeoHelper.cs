using System.Text.RegularExpressions;

namespace TdsTecnologia2027.Web.Services;

/// <summary>
/// Converte URLs do Vimeo (em vários formatos) para a URL de incorporação do player.
/// Aceita:
///   https://vimeo.com/123456789
///   https://vimeo.com/123456789/abcdef0123   (vídeo privado, com hash)
///   https://player.vimeo.com/video/123456789
///   https://player.vimeo.com/video/123456789?h=abcdef0123
/// </summary>
public static partial class VimeoHelper
{
    [GeneratedRegex(@"(?:vimeo\.com/(?:video/)?|player\.vimeo\.com/video/)(\d+)(?:[/?].*?h=|/)?([0-9a-zA-Z]+)?")]
    private static partial Regex VimeoRegex();

    /// <summary>
    /// Devolve a URL de incorporação (player.vimeo.com/video/ID[?h=HASH]) ou null
    /// se a URL não for reconhecida.
    /// </summary>
    public static string? ObterUrlEmbed(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        var match = VimeoRegex().Match(url.Trim());
        if (!match.Success) return null;

        var id = match.Groups[1].Value;
        var hash = match.Groups[2].Success ? match.Groups[2].Value : null;

        var embed = $"https://player.vimeo.com/video/{id}";
        if (!string.IsNullOrEmpty(hash))
            embed += $"?h={hash}";
        return embed;
    }
}
