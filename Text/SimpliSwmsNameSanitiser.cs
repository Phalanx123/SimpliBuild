using System.Text.RegularExpressions;

namespace SimpliBuild.Text;

/// <summary>
/// Applies SimpliSWMS name validation rules:
/// - letters only
/// - allows hyphen, apostrophe, space
/// - max length 40
/// - removes all other characters
/// </summary>
public static partial class SimpliSwmsNameSanitiser
{
    [GeneratedRegex("[^a-zA-Z\\-\'\\s]")]
    private static partial Regex InvalidCharactersRegex();

    [GeneratedRegex("\\s+")]
    private static partial Regex MultiSpaceRegex();

    private const int MaxLength = 40;

    public static string Sanitise(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var cleaned = value.Trim();

        cleaned = InvalidCharactersRegex()
            .Replace(cleaned, string.Empty);

        cleaned = MultiSpaceRegex()
            .Replace(cleaned, " ");

        if (cleaned.Length > MaxLength)
            cleaned = cleaned[..MaxLength];

        return cleaned;
    }
}