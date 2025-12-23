using System.Text.RegularExpressions;

namespace MillionaireGame.Web.Services;

/// <summary>
/// Validates participant display names with profanity filtering and format rules
/// </summary>
public class NameValidationService
{
    private static readonly HashSet<string> ProfanityList = new(StringComparer.OrdinalIgnoreCase)
    {
        // Common profanity - basic list (expand as needed)
        "damn", "hell", "crap", "shit", "fuck", "ass", "bitch", "bastard",
        "dick", "cock", "pussy", "cunt", "whore", "slut", "fag", "nigger",
        "retard", "idiot", "stupid", "dumb", "moron", "imbecile"
    };

    private const int MaxNameLength = 35;
    private const int MinNameLength = 1;

    /// <summary>
    /// Validates a display name according to game rules
    /// </summary>
    public NameValidationResult ValidateName(string? name)
    {
        // Check for null or empty
        if (string.IsNullOrWhiteSpace(name))
        {
            return NameValidationResult.Fail("Name cannot be empty");
        }

        // Trim and normalize whitespace
        name = Regex.Replace(name.Trim(), @"\s+", " ");

        // Check length
        if (name.Length < MinNameLength)
        {
            return NameValidationResult.Fail("Name is too short");
        }

        if (name.Length > MaxNameLength)
        {
            return NameValidationResult.Fail($"Name must be {MaxNameLength} characters or less");
        }

        // Check for emojis and other Unicode symbols
        if (ContainsEmojis(name))
        {
            return NameValidationResult.Fail("Name cannot contain emojis or special symbols");
        }

        // Check for profanity
        if (ContainsProfanity(name))
        {
            return NameValidationResult.Fail("Name contains inappropriate language");
        }

        // Check for valid characters (letters, numbers, spaces, basic punctuation)
        if (!Regex.IsMatch(name, @"^[a-zA-Z0-9\s\.\-_']+$"))
        {
            return NameValidationResult.Fail("Name contains invalid characters. Only letters, numbers, spaces, and basic punctuation allowed");
        }

        return NameValidationResult.Success(name);
    }

    /// <summary>
    /// Checks if text contains emojis or Unicode symbols beyond basic Latin
    /// </summary>
    private bool ContainsEmojis(string text)
    {
        // Check for emoji ranges and other Unicode symbols
        foreach (char c in text)
        {
            // Allow basic Latin, extended Latin, numbers, and basic punctuation
            if (c < 0x0020 || // Control characters
                (c >= 0x007F && c <= 0x00A0) || // More control characters and non-breaking space
                c >= 0x2000) // Unicode symbols, emojis, etc.
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if text contains profanity from the filter list
    /// </summary>
    private bool ContainsProfanity(string text)
    {
        // Normalize the text for checking
        string normalized = text.ToLowerInvariant();
        
        // Check each word in the profanity list
        foreach (var profanity in ProfanityList)
        {
            // Check for exact word match (with word boundaries)
            if (Regex.IsMatch(normalized, $@"\b{Regex.Escape(profanity)}\b"))
            {
                return true;
            }
            
            // Check for leetspeak and common substitutions
            string pattern = CreateLeetspeakPattern(profanity);
            if (Regex.IsMatch(normalized, pattern))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Creates a regex pattern to catch common leetspeak substitutions
    /// </summary>
    private string CreateLeetspeakPattern(string word)
    {
        // Replace common letter substitutions
        var pattern = word
            .Replace("a", "[a@4]")
            .Replace("e", "[e3]")
            .Replace("i", "[i1!]")
            .Replace("o", "[o0]")
            .Replace("s", "[s5$]")
            .Replace("t", "[t7+]")
            .Replace("l", "[l1!]");

        return $@"\b{pattern}\b";
    }

    /// <summary>
    /// Checks if a name is unique within a session
    /// </summary>
    public bool IsNameUnique(string name, IEnumerable<string> existingNames)
    {
        return !existingNames.Any(n => 
            string.Equals(n, name, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Result of name validation
/// </summary>
public class NameValidationResult
{
    public bool IsValid { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? SanitizedName { get; private set; }

    private NameValidationResult() { }

    public static NameValidationResult Success(string sanitizedName)
    {
        return new NameValidationResult
        {
            IsValid = true,
            SanitizedName = sanitizedName
        };
    }

    public static NameValidationResult Fail(string errorMessage)
    {
        return new NameValidationResult
        {
            IsValid = false,
            ErrorMessage = errorMessage
        };
    }
}
