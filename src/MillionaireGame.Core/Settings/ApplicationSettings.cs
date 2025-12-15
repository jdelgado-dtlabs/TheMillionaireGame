using System.Xml.Serialization;

namespace MillionaireGame.Core.Settings;

/// <summary>
/// Main application settings and profile configuration
/// </summary>
[XmlRoot("AppSettings")]
public class ApplicationSettings
{
    // Lifeline Configuration
    public int TotalLifelines { get; set; } = 4;
    public string Lifeline1 { get; set; } = "5050";
    public string Lifeline2 { get; set; } = "plusone";
    public string Lifeline3 { get; set; } = "ata";
    public string Lifeline4 { get; set; } = "switch";
    public int Lifeline1Available { get; set; } = 0; // 0 = Always
    public int Lifeline2Available { get; set; } = 0;
    public int Lifeline3Available { get; set; } = 0;
    public int Lifeline4Available { get; set; } = 0;

    // Graphics Settings
    public int WinningStrapTexture { get; set; } = 0;
    public int QuestionsTexture { get; set; } = 0;

    // Screen Display Settings
    public bool AutoShowHostScreen { get; set; } = false;
    public bool AutoShowGuestScreen { get; set; } = false;
    public bool AutoShowTVScreen { get; set; } = false;
    public string ResolutionHostScreen { get; set; } = "720";
    public string ResolutionGuestScreen { get; set; } = "720";
    public string ResolutionTVScreen { get; set; } = "720";
    public bool FullScreenHostScreenEnable { get; set; } = false;
    public int FullScreenHostScreenMonitor { get; set; } = 0;
    public bool FullScreenGuestScreenEnable { get; set; } = false;
    public int FullScreenGuestScreenMonitor { get; set; } = 0;
    public bool FullScreenTVScreenEnable { get; set; } = false;
    public int FullScreenTVScreenMonitor { get; set; } = 0;

    // Game Behavior Settings
    public bool ClearHostMessagesAtNewQuestion { get; set; } = false;
    public bool ShowAnswerOnlyOnHostScreenAtFinal { get; set; } = false;
    public bool AutoHideQuestionAtPlusOne { get; set; } = false;
    public bool AutoShowTotalWinnings { get; set; } = false;
    public bool AutoHideQuestionAtWalkAway { get; set; } = false;
    public bool HideAnswerInControlPanelAtNewQ { get; set; } = false;
    public bool ATAIsAlwaysCorrect { get; set; } = false;

    // Fastest Finger First Settings
    public int FFFPort { get; set; } = 3818;
    public string FFFPlayer1Name { get; set; } = "Player 1";
    public string FFFPlayer2Name { get; set; } = "Player 2";
    public string FFFPlayer3Name { get; set; } = "Player 3";
    public string FFFPlayer4Name { get; set; } = "Player 4";
    public string FFFPlayer5Name { get; set; } = "Player 5";
    public string FFFPlayer6Name { get; set; } = "Player 6";
    public string FFFPlayer7Name { get; set; } = "Player 7";
    public string FFFPlayer8Name { get; set; } = "Player 8";

    // Sound File Paths - General
    public string SoundOpening { get; set; } = string.Empty;
    public string SoundCommercialIn { get; set; } = string.Empty;
    public string SoundCommercialOut { get; set; } = string.Empty;
    public string SoundClosing { get; set; } = string.Empty;
    public string SoundLifeline1Ping { get; set; } = string.Empty;
    public string SoundLifeline2Ping { get; set; } = string.Empty;
    public string SoundLifeline3Ping { get; set; } = string.Empty;
    public string SoundLifeline4Ping { get; set; } = string.Empty;
    public string SoundRiskModeActive { get; set; } = string.Empty;
    public string SoundExplainRules { get; set; } = string.Empty;
    public string SoundToHotSeat { get; set; } = string.Empty;
    public string SoundToHotSeatLightsDown { get; set; } = string.Empty;
    public string SoundWalkAway1 { get; set; } = string.Empty;
    public string SoundWalkAway2 { get; set; } = string.Empty;
    public string SoundGameOver { get; set; } = string.Empty;

    // Sound File Paths - Fastest Finger First
    public string SoundFFFMeet2 { get; set; } = string.Empty;
    public string SoundFFFMeet3 { get; set; } = string.Empty;
    public string SoundFFFMeet4 { get; set; } = string.Empty;
    public string SoundFFFMeet5 { get; set; } = string.Empty;
    public string SoundFFFMeet6 { get; set; } = string.Empty;
    public string SoundFFFMeet7 { get; set; } = string.Empty;
    public string SoundFFFMeet8 { get; set; } = string.Empty;
    public string SoundFFFReadQuestion { get; set; } = string.Empty;
    public string SoundFFFThreeNotes { get; set; } = string.Empty;
    public string SoundFFFThinking { get; set; } = string.Empty;
    public string SoundFFFReadCorrectOrder { get; set; } = string.Empty;
    public string SoundFFFOrder1 { get; set; } = string.Empty;
    public string SoundFFFOrder2 { get; set; } = string.Empty;
    public string SoundFFFOrder3 { get; set; } = string.Empty;
    public string SoundFFFOrder4 { get; set; } = string.Empty;
    public string SoundFFFWhoWasCorrect { get; set; } = string.Empty;
    public string SoundFFFWinner { get; set; } = string.Empty;
    public string SoundRandomContestantPicking { get; set; } = string.Empty;
    public string SoundSetSafetyNet { get; set; } = string.Empty;

    // Sound File Paths - Lifelines
    public string SoundATAStart { get; set; } = string.Empty;
    public string SoundATAVoting { get; set; } = string.Empty;
    public string SoundATAEnd { get; set; } = string.Empty;
    public string SoundPlusOneStart { get; set; } = string.Empty;
    public string SoundPlusOneClock { get; set; } = string.Empty;
    public string SoundPlusOneEndEarly { get; set; } = string.Empty;
    public string SoundDouble1stAnswer { get; set; } = string.Empty;
    public string SoundDouble1stFinal { get; set; } = string.Empty;
    public string SoundDouble2ndAnswer { get; set; } = string.Empty;
    public string SoundDouble2ndFinal { get; set; } = string.Empty;
    public string SoundSwitchActivate { get; set; } = string.Empty;
    public string SoundSwitchShowCorrect { get; set; } = string.Empty;
    public string SoundSwitchClear { get; set; } = string.Empty;
    public string Sound5050 { get; set; } = string.Empty;
    public string SoundHostStart { get; set; } = string.Empty;
    public string SoundHostEnd { get; set; } = string.Empty;

    // Sound File Paths - Lights Down by Question Level
    public string SoundQ1to5LightsDown { get; set; } = string.Empty;
    public bool SoundQ1to5LightsDownStop { get; set; } = false;
    public string SoundQ6LightsDown { get; set; } = string.Empty;
    public bool SoundQ6LightsDownStop { get; set; } = false;
    public string SoundQ7LightsDown { get; set; } = string.Empty;
    public bool SoundQ7LightsDownStop { get; set; } = false;
    public string SoundQ8LightsDown { get; set; } = string.Empty;
    public bool SoundQ8LightsDownStop { get; set; } = false;
    public string SoundQ9LightsDown { get; set; } = string.Empty;
    public bool SoundQ9LightsDownStop { get; set; } = false;
    public string SoundQ10LightsDown { get; set; } = string.Empty;
    public bool SoundQ10LightsDownStop { get; set; } = false;
    public string SoundQ11LightsDown { get; set; } = string.Empty;
    public bool SoundQ11LightsDownStop { get; set; } = false;
    public string SoundQ12LightsDown { get; set; } = string.Empty;
    public bool SoundQ12LightsDownStop { get; set; } = false;
    public string SoundQ13LightsDown { get; set; } = string.Empty;
    public bool SoundQ13LightsDownStop { get; set; } = false;
    public string SoundQ14LightsDown { get; set; } = string.Empty;
    public bool SoundQ14LightsDownStop { get; set; } = false;
    public string SoundQ15LightsDown { get; set; } = string.Empty;
    public bool SoundQ15LightsDownStop { get; set; } = false;

    // Sound File Paths - Question Bed Music
    public string SoundQ1to5Bed { get; set; } = string.Empty;
    public string SoundQ6Bed { get; set; } = string.Empty;
    public string SoundQ7Bed { get; set; } = string.Empty;
    public string SoundQ8Bed { get; set; } = string.Empty;
    public string SoundQ9Bed { get; set; } = string.Empty;
    public string SoundQ10Bed { get; set; } = string.Empty;
    public string SoundQ11Bed { get; set; } = string.Empty;
    public string SoundQ12Bed { get; set; } = string.Empty;
    public string SoundQ13Bed { get; set; } = string.Empty;
    public string SoundQ14Bed { get; set; } = string.Empty;
    public string SoundQ15Bed { get; set; } = string.Empty;

    // Sound File Paths - Final Answer Sounds
    public string SoundQ1Final { get; set; } = string.Empty;
    public string SoundQ2Final { get; set; } = string.Empty;
    public string SoundQ3Final { get; set; } = string.Empty;
    public string SoundQ4Final { get; set; } = string.Empty;
    public string SoundQ5Final { get; set; } = string.Empty;
    public string SoundQ6Final { get; set; } = string.Empty;
    public string SoundQ7Final { get; set; } = string.Empty;
    public string SoundQ8Final { get; set; } = string.Empty;
    public string SoundQ9Final { get; set; } = string.Empty;
    public string SoundQ10Final { get; set; } = string.Empty;
    public string SoundQ11Final { get; set; } = string.Empty;
    public string SoundQ12Final { get; set; } = string.Empty;
    public string SoundQ13Final { get; set; } = string.Empty;
    public string SoundQ14Final { get; set; } = string.Empty;
    public string SoundQ15Final { get; set; } = string.Empty;

    // Sound File Paths - Correct Answer Sounds
    public string SoundQ1to4Correct { get; set; } = string.Empty;
    public string SoundQ5Correct { get; set; } = string.Empty;
    public string SoundQ5CorrectRisk { get; set; } = string.Empty;
    public string SoundQ6Correct { get; set; } = string.Empty;
    public string SoundQ7Correct { get; set; } = string.Empty;
    public string SoundQ8Correct { get; set; } = string.Empty;
    public string SoundQ9Correct { get; set; } = string.Empty;
    public string SoundQ10Correct { get; set; } = string.Empty;
    public string SoundQ10CorrectRisk { get; set; } = string.Empty;
    public string SoundQ11Correct { get; set; } = string.Empty;
    public string SoundQ12Correct { get; set; } = string.Empty;
    public string SoundQ13Correct { get; set; } = string.Empty;
    public string SoundQ14Correct { get; set; } = string.Empty;
    public string SoundQ15Correct { get; set; } = string.Empty;

    // Sound File Paths - Wrong Answer Sounds
    public string SoundQ1to5Wrong { get; set; } = string.Empty;
    public string SoundQ6Wrong { get; set; } = string.Empty;
    public string SoundQ7Wrong { get; set; } = string.Empty;
    public string SoundQ8Wrong { get; set; } = string.Empty;
    public string SoundQ9Wrong { get; set; } = string.Empty;
    public string SoundQ10Wrong { get; set; } = string.Empty;
    public string SoundQ11Wrong { get; set; } = string.Empty;
    public string SoundQ12Wrong { get; set; } = string.Empty;
    public string SoundQ13Wrong { get; set; } = string.Empty;
    public string SoundQ14Wrong { get; set; } = string.Empty;
    public string SoundQ15Wrong { get; set; } = string.Empty;
}

/// <summary>
/// Manager for application settings persistence
/// </summary>
public class ApplicationSettingsManager
{
    private const string FileName = "config.xml";
    private readonly string _filePath;

    public ApplicationSettings Settings { get; private set; }

    public ApplicationSettingsManager(string? basePath = null)
    {
        _filePath = Path.Combine(basePath ?? AppDomain.CurrentDomain.BaseDirectory, FileName);
        Settings = new ApplicationSettings();
    }

    public void LoadSettings()
    {
        if (!File.Exists(_filePath))
        {
            SaveDefaultSettings();
            return;
        }

        try
        {
            var serializer = new XmlSerializer(typeof(ApplicationSettings));
            using var reader = new StreamReader(_filePath);
            var loadedSettings = (ApplicationSettings?)serializer.Deserialize(reader);
            if (loadedSettings != null)
            {
                Settings = loadedSettings;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading application settings: {ex.Message}");
            SaveDefaultSettings();
        }
    }

    public void SaveSettings()
    {
        try
        {
            var serializer = new XmlSerializer(typeof(ApplicationSettings));
            using var writer = new StreamWriter(_filePath);
            serializer.Serialize(writer, Settings);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving application settings: {ex.Message}");
            throw;
        }
    }

    private void SaveDefaultSettings()
    {
        Settings = new ApplicationSettings();
        SaveSettings();
    }
}
