using System.Reflection;
using MillionaireGame.Utilities;

namespace MillionaireGame.Graphics;

/// <summary>
/// Manages loading and caching of texture images for game screens
/// </summary>
public class TextureManager
{
    private static TextureManager? _instance;
    private readonly Dictionary<string, Image> _textureCache = new();
    private readonly Assembly _assembly;

    private TextureManager()
    {
        _assembly = Assembly.GetExecutingAssembly();
    }

    public static TextureManager Instance
    {
        get
        {
            _instance ??= new TextureManager();
            return _instance;
        }
    }

    /// <summary>
    /// Texture set types available
    /// </summary>
    public enum TextureSet
    {
        Default = 0,      // White border
        US2020 = 1,       // Blue border
        Gold = 2,         // Gold border
        Blue = 3,         // Blue theme
        Purple = 4        // Purple theme
    }

    /// <summary>
    /// Element types for textures
    /// </summary>
    public enum ElementType
    {
        QuestionStrap,
        AnswerLeftNormal,
        AnswerRightNormal,
        AnswerLeftFinal,
        AnswerRightFinal,
        AnswerLeftCorrect,
        AnswerRightCorrect,
        MoneyTreeBase,
        MoneyTreePosition
    }

    /// <summary>
    /// Load a texture from embedded resources
    /// </summary>
    public Image? GetTexture(ElementType element, TextureSet textureSet = TextureSet.Default)
    {
        string cacheKey = $"{textureSet}_{element}";
        
        if (_textureCache.TryGetValue(cacheKey, out var cachedImage))
        {
            return cachedImage;
        }

        string resourceName = GetResourceName(element, textureSet);
        string fullResourceName = $"MillionaireGame.lib.textures.{resourceName}";

        try
        {
            using var stream = _assembly.GetManifestResourceStream(fullResourceName);
            if (stream != null)
            {
                var image = Image.FromStream(stream);
                _textureCache[cacheKey] = image;
                return image;
            }
        }
        catch (Exception ex)
        {
            GameConsole.Debug($"[TextureManager] Error loading texture '{fullResourceName}': {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Get the filename for a specific element and texture set
    /// </summary>
    private string GetResourceName(ElementType element, TextureSet textureSet)
    {
        int setNumber = (int)textureSet + 1; // Files are numbered 01-05

        return element switch
        {
            ElementType.QuestionStrap => $"{setNumber:D2}_Question_Strap.png",
            ElementType.AnswerLeftNormal => $"{setNumber:D2}_Answer_L_Normal.png",
            ElementType.AnswerRightNormal => $"{setNumber:D2}_Answer_R_Normal.png",
            ElementType.AnswerLeftFinal => $"{setNumber:D2}_Answer_L_Final.png",
            ElementType.AnswerRightFinal => $"{setNumber:D2}_Answer_R_Final.png",
            ElementType.AnswerLeftCorrect => $"{setNumber:D2}_Answer_L_Correct.png",
            ElementType.AnswerRightCorrect => $"{setNumber:D2}_Answer_R_Correct.png",
            ElementType.MoneyTreeBase => $"{setNumber:D2}_Tree.png",
            ElementType.MoneyTreePosition => throw new ArgumentException("Use GetMoneyTreePosition() instead"),
            _ => throw new ArgumentException($"Unknown element type: {element}")
        };
    }

    /// <summary>
    /// Get money tree position highlight image for a specific level (1-15)
    /// </summary>
    public Image? GetMoneyTreePosition(int level)
    {
        if (level < 0 || level > 15)
        {
            throw new ArgumentOutOfRangeException(nameof(level), "Level must be between 0 and 15");
        }

        string cacheKey = $"MoneyTree_Position_{level:D2}";
        
        if (_textureCache.TryGetValue(cacheKey, out var cachedImage))
        {
            return cachedImage;
        }

        string resourceName = level == 0 ? "mg_tree_00.png" : $"999_Tree_{level:D2}.png";
        string fullResourceName = $"MillionaireGame.lib.textures.{resourceName}";

        try
        {
            using var stream = _assembly.GetManifestResourceStream(fullResourceName);
            if (stream != null)
            {
                var image = Image.FromStream(stream);
                _textureCache[cacheKey] = image;
                return image;
            }
        }
        catch (Exception ex)
        {
            GameConsole.Debug($"[TextureManager] Error loading money tree position '{fullResourceName}': {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Get money tree position alternate lock-in graphic for safety net levels (5, 10)
    /// </summary>
    public Image? GetMoneyTreePositionLockAlt(int level)
    {
        if (level < 0 || level > 15)
        {
            throw new ArgumentOutOfRangeException(nameof(level), "Level must be between 0 and 15");
        }

        string cacheKey = $"MoneyTree_Position_LockAlt_{level:D2}";
        
        if (_textureCache.TryGetValue(cacheKey, out var cachedImage))
        {
            return cachedImage;
        }

        string resourceName = $"999_Tree_{level:D2}_lck_alt.png";
        string fullResourceName = $"MillionaireGame.lib.textures.{resourceName}";

        try
        {
            using var stream = _assembly.GetManifestResourceStream(fullResourceName);
            if (stream != null)
            {
                var image = Image.FromStream(stream);
                _textureCache[cacheKey] = image;
                return image;
            }
        }
        catch (Exception ex)
        {
            GameConsole.Debug($"[TextureManager] Error loading money tree lock-in alt '{fullResourceName}': {ex.Message}");
        }

        // Fall back to regular position graphic if alternate not found
        return GetMoneyTreePosition(level);
    }

    /// <summary>
    /// List all available embedded texture resources (for debugging)
    /// </summary>
    public List<string> ListEmbeddedTextures()
    {
        return _assembly.GetManifestResourceNames()
            .Where(name => name.Contains(".lib.textures."))
            .ToList();
    }

    /// <summary>
    /// Clear the texture cache to free memory
    /// </summary>
    public void ClearCache()
    {
        foreach (var image in _textureCache.Values)
        {
            image.Dispose();
        }
        _textureCache.Clear();
    }
}
