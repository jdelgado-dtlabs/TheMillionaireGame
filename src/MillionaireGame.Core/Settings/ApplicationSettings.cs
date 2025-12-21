using MillionaireGame.Core.Database;
using System.Reflection;
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

    // Sound Pack Settings
    public string SelectedSoundPack { get; set; } = "Default";

    // Sound File Paths - General
    public string SoundOpening { get; set; } = "opening_theme.mp3";
    public string SoundCommercialIn { get; set; } = "commercial_in.mp3";
    public string SoundCommercialOut { get; set; } = "commercial_out.mp3";
    public string SoundClosing { get; set; } = "close_theme.mp3";
    public string SoundLifeline1Ping { get; set; } = "lifeline_1_on.mp3";
    public string SoundLifeline2Ping { get; set; } = "lifeline_2_on.mp3";
    public string SoundLifeline3Ping { get; set; } = "lifeline_3_on.mp3";
    public string SoundLifeline4Ping { get; set; } = "lifeline_4_on.mp3";
    public string SoundLifelinePAFStart { get; set; } = "paf_start.mp3";
    public string SoundLifelinePAFCountdown { get; set; } = "paf_countdown.mp3";
    public string SoundLifelinePAFEndEarly { get; set; } = "paf_end_call_early.mp3";
    public string SoundLifelineATAStart { get; set; } = "ata_start.mp3";
    public string SoundLifelineATAVote { get; set; } = "ata_vote.mp3";
    public string SoundLifelineATAEnd { get; set; } = "ata_end.mp3";
    public string SoundRiskModeActive { get; set; } = "risk_mode.mp3";
    public string SoundExplainRules { get; set; } = "explain_rules.mp3";
    public string SoundToHotSeat { get; set; } = string.Empty;
    public string SoundToHotSeatLightsDown { get; set; } = string.Empty;
    public string SoundQuitSmall { get; set; } = "quit_small.mp3";
    public string SoundQuitLarge { get; set; } = "quit_large.mp3";
    public string SoundWalkAway1 { get; set; } = "walk_away_small.mp3";
    public string SoundWalkAway2 { get; set; } = "walk_away_large.mp3";
    public string SoundGameOver { get; set; } = "game_over.mp3";

    // Sound File Paths - Fastest Finger First
    public string SoundFFFMeet2 { get; set; } = "fastest_finger_contestants.mp3";
    public string SoundFFFMeet3 { get; set; } = "fastest_finger_contestants_2.mp3";
    public string SoundFFFMeet4 { get; set; } = "fastest_finger_contestants_3.mp3";
    public string SoundFFFMeet5 { get; set; } = "fastest_finger_contestants_4.mp3";
    public string SoundFFFMeet6 { get; set; } = "fastest_finger_contestants_5.mp3";
    public string SoundFFFMeet7 { get; set; } = "fastest_finger_contestants_6.mp3";
    public string SoundFFFMeet8 { get; set; } = "fastest_finger_contestants_7.mp3";
    public string SoundFFFReadQuestion { get; set; } = "fastest_finger_read_question.mp3";
    public string SoundFFFThreeNotes { get; set; } = "fastest_finger_3_stabs.mp3";
    public string SoundFFFThinking { get; set; } = "fastest_finger_think.mp3";
    public string SoundFFFReadCorrectOrder { get; set; } = "fastest_finger_read_answer_order.mp3";
    public string SoundFFFOrder1 { get; set; } = "fastest_finger_answer_correct_1.mp3";
    public string SoundFFFOrder2 { get; set; } = "fastest_finger_answer_correct_2.mp3";
    public string SoundFFFOrder3 { get; set; } = "fastest_finger_answer_correct_3.mp3";
    public string SoundFFFOrder4 { get; set; } = "fastest_finger_answer_correct_4.mp3";
    public string SoundFFFWhoWasCorrect { get; set; } = "fastest_finger_reveal_times.mp3";
    public string SoundFFFWinner { get; set; } = "fastest_finger_winner.mp3";
    public string SoundRandomContestantPicking { get; set; } = "pick_random_contestant.mp3";
    public string SoundSetSafetyNet { get; set; } = "set_safety_net.mp3";

    // Sound File Paths - Lifelines
    public string SoundATAStart { get; set; } = "ata_start.mp3";
    public string SoundATAVoting { get; set; } = "ata_vote.mp3";
    public string SoundATAEnd { get; set; } = "ata_end.mp3";
    public string SoundPlusOneStart { get; set; } = "paf_start.mp3"; // Phone a Friend
    public string SoundPlusOneClock { get; set; } = "paf_countdown.mp3";
    public string SoundPlusOneEndEarly { get; set; } = "paf_end_call_early.mp3";
    public string SoundDouble1stAnswer { get; set; } = string.Empty; // Not used
    public string SoundDouble1stFinal { get; set; } = string.Empty; // Not used
    public string SoundDouble2ndAnswer { get; set; } = string.Empty; // Not used
    public string SoundDouble2ndFinal { get; set; } = string.Empty; // Not used
    public string SoundSwitchActivate { get; set; } = "stq_start.mp3";
    public string SoundSwitchShowCorrect { get; set; } = "stq_reveal_correct_answer.mp3";
    public string SoundSwitchClear { get; set; } = "stq_new_question_flip.mp3";
    public string Sound5050 { get; set; } = "fifty_fifty.mp3";
    public string SoundHostStart { get; set; } = "host_entrance.mp3";
    public string SoundCloseStart { get; set; } = "close_underscore.mp3";
    public string SoundCloseFinal { get; set; } = "close_theme.mp3";

    // Sound File Paths - Lights Down by Question Level
    public string SoundQ1to5LightsDown { get; set; } = "lights_down_classic.mp3";
    public bool SoundQ1to5LightsDownStop { get; set; } = false;
    public string SoundQ6LightsDown { get; set; } = "lights_down_1.mp3";
    public bool SoundQ6LightsDownStop { get; set; } = false;
    public string SoundQ7LightsDown { get; set; } = "lights_down_2.mp3";
    public bool SoundQ7LightsDownStop { get; set; } = false;
    public string SoundQ8LightsDown { get; set; } = "lights_down_3.mp3";
    public bool SoundQ8LightsDownStop { get; set; } = false;
    public string SoundQ9LightsDown { get; set; } = "lights_down_4.mp3";
    public bool SoundQ9LightsDownStop { get; set; } = false;
    public string SoundQ10LightsDown { get; set; } = "lights_down_5.mp3";
    public bool SoundQ10LightsDownStop { get; set; } = false;
    public string SoundQ11LightsDown { get; set; } = "lights_down_1.mp3";
    public bool SoundQ11LightsDownStop { get; set; } = false;
    public string SoundQ12LightsDown { get; set; } = "lights_down_2.mp3";
    public bool SoundQ12LightsDownStop { get; set; } = false;
    public string SoundQ13LightsDown { get; set; } = "lights_down_3.mp3";
    public bool SoundQ13LightsDownStop { get; set; } = false;
    public string SoundQ14LightsDown { get; set; } = "lights_down_4.mp3";
    public bool SoundQ14LightsDownStop { get; set; } = false;
    public string SoundQ15LightsDown { get; set; } = "lights_down_5.mp3";
    public bool SoundQ15LightsDownStop { get; set; } = false;

    // Sound File Paths - Question Bed Music
    public string SoundQ1to5Bed { get; set; } = "q1_to_q5_bed.mp3";
    public string SoundQ6Bed { get; set; } = "q6_bed.mp3";
    public string SoundQ7Bed { get; set; } = "q7_bed.mp3";
    public string SoundQ8Bed { get; set; } = "q8_bed.mp3";
    public string SoundQ9Bed { get; set; } = "q9_bed.mp3";
    public string SoundQ10Bed { get; set; } = "q10_bed.mp3";
    public string SoundQ11Bed { get; set; } = "q11_bed.mp3";
    public string SoundQ12Bed { get; set; } = "q12_bed.mp3";
    public string SoundQ13Bed { get; set; } = "q13_bed.mp3";
    public string SoundQ14Bed { get; set; } = "q14_bed.mp3";
    public string SoundQ15Bed { get; set; } = "q15_bed.mp3";

    // Sound File Paths - Final Answer Sounds
    public string SoundQ1Final { get; set; } = string.Empty; // Level 1 doesn't use final answer sounds
    public string SoundQ2Final { get; set; } = string.Empty;
    public string SoundQ3Final { get; set; } = string.Empty;
    public string SoundQ4Final { get; set; } = string.Empty;
    public string SoundQ5Final { get; set; } = string.Empty;
    public string SoundQ6Final { get; set; } = "final_answer_1.mp3"; // Level 2, Q1
    public string SoundQ7Final { get; set; } = "final_answer_2.mp3"; // Level 2, Q2
    public string SoundQ8Final { get; set; } = "final_answer_3.mp3"; // Level 2, Q3
    public string SoundQ9Final { get; set; } = "final_answer_4.mp3"; // Level 2, Q4
    public string SoundQ10Final { get; set; } = "final_answer_5.mp3"; // Level 2, Q5
    public string SoundQ11Final { get; set; } = "final_answer_1.mp3"; // Level 3, Q1
    public string SoundQ12Final { get; set; } = "final_answer_2.mp3"; // Level 3, Q2
    public string SoundQ13Final { get; set; } = "final_answer_3.mp3"; // Level 3, Q3
    public string SoundQ14Final { get; set; } = "final_answer_4.mp3"; // Level 3, Q4
    public string SoundQ15Final { get; set; } = "final_answer_5.mp3"; // Level 4

    // Sound File Paths - Correct Answer Sounds
    public string SoundQ1to4Correct { get; set; } = "q1_to_q4_correct.mp3";
    public string SoundQ5Correct { get; set; } = "q5_correct.mp3";
    public string SoundQ5Correct2 { get; set; } = "q5_correct_2.mp3";
    public string SoundQ6Correct { get; set; } = "q6_correct.mp3";
    public string SoundQ7Correct { get; set; } = "q7_correct.mp3";
    public string SoundQ8Correct { get; set; } = "q8_correct.mp3";
    public string SoundQ9Correct { get; set; } = "q9_correct.mp3";
    public string SoundQ10Correct { get; set; } = "q10_correct.mp3";
    public string SoundQ10Correct2 { get; set; } = "q10_correct_2.mp3";
    public string SoundQ11Correct { get; set; } = "q11_correct.mp3";
    public string SoundQ12Correct { get; set; } = "q12_correct.mp3";
    public string SoundQ13Correct { get; set; } = "q13_correct.mp3";
    public string SoundQ14Correct { get; set; } = "q14_correct.mp3";
    public string SoundQ15Correct { get; set; } = "q15_correct.mp3";

    // Sound File Paths - Wrong Answer Sounds
    public string SoundQ1to5Wrong { get; set; } = "q1_to_q5_lose.mp3";
    public string SoundQ6Wrong { get; set; } = "q6_lose.mp3";
    public string SoundQ7Wrong { get; set; } = "q7_lose.mp3";
    public string SoundQ8Wrong { get; set; } = "q8_lose.mp3";
    public string SoundQ9Wrong { get; set; } = "q9_lose.mp3";
    public string SoundQ10Wrong { get; set; } = "q10_lose.mp3";
    public string SoundQ11Wrong { get; set; } = "q11_lose.mp3";
    public string SoundQ12Wrong { get; set; } = "q12_lose.mp3";
    public string SoundQ13Wrong { get; set; } = "q13_lose.mp3";
    public string SoundQ14Wrong { get; set; } = "q14_lose.mp3";
    public string SoundQ15Wrong { get; set; } = "q15_lose.mp3";
}

/// <summary>
/// Manager for application settings persistence
/// </summary>
public class ApplicationSettingsManager
{
    private const string FileName = "config.xml";
    private readonly string _filePath;
    private readonly ApplicationSettingsRepository? _repository;
    private readonly bool _useDatabaseMode;

    public ApplicationSettings Settings { get; private set; }

    public ApplicationSettingsManager(string? basePath = null, string? connectionString = null)
    {
        _filePath = Path.Combine(basePath ?? AppDomain.CurrentDomain.BaseDirectory, FileName);
        Settings = new ApplicationSettings();
        
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            _repository = new ApplicationSettingsRepository(connectionString);
            _useDatabaseMode = true;
        }
    }

    /// <summary>
    /// Load settings from database or XML file
    /// </summary>
    public async Task LoadSettingsAsync()
    {
        if (_useDatabaseMode && _repository != null)
        {
            await LoadFromDatabaseAsync();
        }
        else
        {
            LoadFromXml();
        }
    }

    /// <summary>
    /// Synchronous load for backward compatibility (uses XML only)
    /// </summary>
    public void LoadSettings()
    {
        LoadFromXml();
    }

    private async Task LoadFromDatabaseAsync()
    {
        if (_repository == null)
            return;

        try
        {
            var dbSettings = await _repository.GetAllSettingsAsync();
            Settings = new ApplicationSettings();
            
            var properties = typeof(ApplicationSettings).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (!property.CanWrite)
                    continue;

                if (dbSettings.TryGetValue(property.Name, out var value))
                {
                    // Convert string value to property type
                    object? convertedValue = ConvertValue(value, property.PropertyType);
                    if (convertedValue != null)
                    {
                        property.SetValue(Settings, convertedValue);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading settings from database: {ex.Message}");
            // Fall back to defaults
            Settings = new ApplicationSettings();
        }
    }

    private void LoadFromXml()
    {
        if (!File.Exists(_filePath))
        {
            Settings = new ApplicationSettings();
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
            Console.WriteLine($"Error loading application settings from XML: {ex.Message}");
            Settings = new ApplicationSettings();
        }
    }

    /// <summary>
    /// Save settings to database or XML file
    /// </summary>
    public async Task SaveSettingsAsync()
    {
        if (_useDatabaseMode && _repository != null)
        {
            await SaveToDatabaseAsync();
        }
        else
        {
            SaveToXml();
        }
    }

    /// <summary>
    /// Synchronous save for backward compatibility (uses XML only)
    /// </summary>
    public void SaveSettings()
    {
        SaveToXml();
    }

    private async Task SaveToDatabaseAsync()
    {
        if (_repository == null)
            return;

        try
        {
            var properties = typeof(ApplicationSettings).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (!property.CanRead)
                    continue;

                var value = property.GetValue(Settings);
                string? stringValue = value?.ToString() ?? string.Empty;
                
                await _repository.SaveSettingAsync(property.Name, stringValue);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving settings to database: {ex.Message}");
            throw;
        }
    }

    private void SaveToXml()
    {
        try
        {
            var serializer = new XmlSerializer(typeof(ApplicationSettings));
            using var writer = new StreamWriter(_filePath);
            serializer.Serialize(writer, Settings);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving application settings to XML: {ex.Message}");
            throw;
        }
    }

    private object? ConvertValue(string value, Type targetType)
    {
        if (string.IsNullOrEmpty(value))
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

        try
        {
            if (targetType == typeof(string))
                return value;
            if (targetType == typeof(bool))
                return bool.Parse(value);
            if (targetType == typeof(int))
                return int.Parse(value);
            if (targetType == typeof(double))
                return double.Parse(value);
            
            return Convert.ChangeType(value, targetType);
        }
        catch
        {
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }
    }
}

