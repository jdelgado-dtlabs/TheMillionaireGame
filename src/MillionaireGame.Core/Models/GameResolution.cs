namespace MillionaireGame.Core.Models;

/// <summary>
/// Represents screen resolution settings for the game
/// </summary>
public class GameResolution
{
    public ResolutionType CurrentResolution { get; set; } = ResolutionType.HD720;
    
    public int Width => CurrentResolution switch
    {
        ResolutionType.HD720 => 1280,
        ResolutionType.FullHD1080 => 1920,
        _ => 1280
    };

    public int Height => CurrentResolution switch
    {
        ResolutionType.HD720 => 720,
        ResolutionType.FullHD1080 => 1080,
        _ => 720
    };
}

/// <summary>
/// Available resolution types
/// </summary>
public enum ResolutionType
{
    HD720 = 720,
    FullHD1080 = 1080
}

/// <summary>
/// Screen types in the application
/// </summary>
public enum ScreenType
{
    TV = 1,
    Host = 2,
    Guest = 3
}
