using System.Xml.Linq;

namespace MillionaireGame.Services;

public class SoundPackManager
{
    private readonly string _soundsDirectory;
    private Dictionary<string, string> _currentSoundPack = new();
    private string _currentPackName = "Default";

    public string CurrentPackName => _currentPackName;
    public IReadOnlyDictionary<string, string> CurrentSounds => _currentSoundPack;

    public SoundPackManager()
    {
        var appDir = AppDomain.CurrentDomain.BaseDirectory;
        _soundsDirectory = Path.Combine(appDir, "lib", "sounds");
        
        // Ensure Default directory exists
        var defaultDir = Path.Combine(_soundsDirectory, "Default");
        if (!Directory.Exists(defaultDir))
        {
            Directory.CreateDirectory(defaultDir);
        }
    }

    /// <summary>
    /// Load a sound pack by name
    /// </summary>
    public bool LoadSoundPack(string packName)
    {
        try
        {
            var packDir = Path.Combine(_soundsDirectory, packName);
            var xmlPath = Path.Combine(packDir, "soundpack.xml");

            if (!File.Exists(xmlPath))
            {
                if (Program.DebugMode)
                {
                    Console.WriteLine($"[SoundPack] XML file not found: {xmlPath}");
                }
                return false;
            }

            var sounds = LoadSoundPackFromXml(xmlPath, packDir);
            if (sounds == null || sounds.Count == 0)
            {
                return false;
            }

            _currentSoundPack = sounds;
            _currentPackName = packName;

            if (Program.DebugMode)
            {
                Console.WriteLine($"[SoundPack] Loaded '{packName}' with {sounds.Count} sounds");
            }

            return true;
        }
        catch (Exception ex)
        {
            if (Program.DebugMode)
            {
                Console.WriteLine($"[SoundPack] Error loading pack '{packName}': {ex.Message}");
            }
            return false;
        }
    }

    /// <summary>
    /// Get list of available sound packs
    /// </summary>
    public List<string> GetAvailableSoundPacks()
    {
        var packs = new List<string>();

        if (!Directory.Exists(_soundsDirectory))
        {
            return packs;
        }

        // Add Default first
        var defaultDir = Path.Combine(_soundsDirectory, "Default");
        if (Directory.Exists(defaultDir) && File.Exists(Path.Combine(defaultDir, "soundpack.xml")))
        {
            packs.Add("Default");
        }

        // Add other packs alphabetically
        var dirs = Directory.GetDirectories(_soundsDirectory)
            .Select(d => Path.GetFileName(d))
            .Where(name => !string.Equals(name, "Default", StringComparison.OrdinalIgnoreCase))
            .OrderBy(name => name);

        foreach (var dir in dirs)
        {
            var xmlPath = Path.Combine(_soundsDirectory, dir, "soundpack.xml");
            if (File.Exists(xmlPath))
            {
                packs.Add(dir);
            }
        }

        return packs;
    }

    /// <summary>
    /// Get the file path for a sound key
    /// </summary>
    public string? GetSoundFile(string key)
    {
        if (_currentSoundPack.TryGetValue(key, out var filePath))
        {
            return filePath;
        }
        return null;
    }

    /// <summary>
    /// Import a sound pack from a zip file
    /// </summary>
    public (bool Success, string Message) ImportSoundPack(string zipPath)
    {
        try
        {
            // Validate zip file exists
            if (!File.Exists(zipPath))
            {
                return (false, "Zip file not found.");
            }

            // Extract to temp directory first to validate
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, tempDir);

                // Look for soundpack.xml
                var xmlPath = Path.Combine(tempDir, "soundpack.xml");
                if (!File.Exists(xmlPath))
                {
                    return (false, "No soundpack.xml found in zip file.");
                }

                // Load and validate XML
                var doc = XDocument.Load(xmlPath);
                var packNameElement = doc.Root?.Element("PackName");
                if (packNameElement == null || string.IsNullOrWhiteSpace(packNameElement.Value))
                {
                    return (false, "PackName not found or empty in soundpack.xml");
                }

                var packName = packNameElement.Value.Trim();

                // Validate pack name
                if (string.Equals(packName, "Default", StringComparison.OrdinalIgnoreCase))
                {
                    return (false, "Pack name 'Default' is reserved and cannot be used.");
                }

                // Validate all required keys are present
                var requiredKeys = GetRequiredSoundKeys();
                var sounds = doc.Root?.Element("Sounds")?.Elements("Sound");
                if (sounds == null)
                {
                    return (false, "No sounds defined in soundpack.xml");
                }

                var definedKeys = sounds
                    .Select(s => s.Attribute("Key")?.Value)
                    .Where(k => !string.IsNullOrWhiteSpace(k))
                    .ToHashSet();

                var missingKeys = requiredKeys.Except(definedKeys).ToList();
                if (missingKeys.Any())
                {
                    return (false, $"Missing required sound keys: {string.Join(", ", missingKeys)}");
                }

                // Create pack directory
                var packDir = Path.Combine(_soundsDirectory, packName);
                if (Directory.Exists(packDir))
                {
                    // Ask if user wants to overwrite (handled by caller)
                    Directory.Delete(packDir, true);
                }

                Directory.CreateDirectory(packDir);

                // Copy all files from temp to pack directory
                foreach (var file in Directory.GetFiles(tempDir, "*.*", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(tempDir, file);
                    var destPath = Path.Combine(packDir, relativePath);
                    var destDir = Path.GetDirectoryName(destPath);
                    if (!string.IsNullOrEmpty(destDir))
                    {
                        Directory.CreateDirectory(destDir);
                    }
                    File.Copy(file, destPath, true);
                }

                return (true, packName);
            }
            finally
            {
                // Clean up temp directory
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }
        catch (Exception ex)
        {
            return (false, $"Error importing sound pack: {ex.Message}");
        }
    }

    /// <summary>
    /// Export example sound pack template
    /// </summary>
    public bool ExportExamplePack(string savePath)
    {
        try
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                // Create example XML
                var exampleXml = CreateExampleXml();
                File.WriteAllText(Path.Combine(tempDir, "soundpack.xml"), exampleXml);

                // Create instructions file with table format
                var soundKeys = GetRequiredSoundKeys();
                var xmlExample = CreateExampleXml();
                var keyToFileMap = new Dictionary<string, string>();
                
                // Parse the example XML to get key-to-filename mappings
                using (var reader = new StringReader(xmlExample))
                {
                    var doc = System.Xml.Linq.XDocument.Load(reader);
                    foreach (var sound in doc.Descendants("Sound"))
                    {
                        var key = sound.Attribute("Key")?.Value;
                        var file = sound.Attribute("File")?.Value;
                        if (key != null && file != null)
                        {
                            // Remove extension
                            var fileWithoutExt = Path.GetFileNameWithoutExtension(file);
                            keyToFileMap[key] = fileWithoutExt;
                        }
                    }
                }
                
                var instructions = @"SOUND PACK INSTRUCTIONS
======================

1. Edit soundpack.xml and change <PackName> to your custom pack name
   - Do NOT use 'Default' as it is reserved
   
2. Add your sound files to this folder
   - Supported formats: MP3, WAV
   - File names must match those in soundpack.xml (excluding extensions)
   
3. Verify all required sounds are included:
   - Check that every Key in the XML has a corresponding sound file
   - File names are case-sensitive
   
4. Zip this folder (include soundpack.xml and all sound files)

5. Import the zip file using the Import button in Settings > Sounds

REQUIRED SOUND MAPPINGS
=======================
The table below shows the XML Key and the corresponding File Name (without extension).
You can use .mp3 or .wav extensions for any file.

XML Key                          | File Name
---------------------------------|----------------------------------";
                
                // Add each key-filename pair as a table row
                foreach (var key in soundKeys)
                {
                    if (keyToFileMap.TryGetValue(key, out var fileName))
                    {
                        instructions += Environment.NewLine + $"{key,-33}| {fileName}";
                    }
                    else
                    {
                        instructions += Environment.NewLine + $"{key,-33}| (define in XML)";
                    }
                }

                File.WriteAllText(Path.Combine(tempDir, "INSTRUCTIONS.txt"), instructions);

                // Create zip file
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }

                System.IO.Compression.ZipFile.CreateFromDirectory(tempDir, savePath);

                return true;
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }
        catch (Exception ex)
        {
            if (Program.DebugMode)
            {
                Console.WriteLine($"[SoundPack] Error exporting example: {ex.Message}");
            }
            return false;
        }
    }

    /// <summary>
    /// Remove a sound pack
    /// </summary>
    public bool RemoveSoundPack(string packName)
    {
        if (string.Equals(packName, "Default", StringComparison.OrdinalIgnoreCase))
        {
            return false; // Cannot remove default
        }

        try
        {
            var packDir = Path.Combine(_soundsDirectory, packName);
            if (Directory.Exists(packDir))
            {
                Directory.Delete(packDir, true);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get sounds grouped by category
    /// </summary>
    public Dictionary<string, List<(string Key, string File)>> GetSoundsByCategory()
    {
        var result = new Dictionary<string, List<(string Key, string File)>>();

        try
        {
            var packDir = Path.Combine(_soundsDirectory, _currentPackName);
            var xmlPath = Path.Combine(packDir, "soundpack.xml");

            if (!File.Exists(xmlPath))
            {
                return result;
            }

            var doc = XDocument.Load(xmlPath);
            var sounds = doc.Root?.Element("Sounds")?.Elements("Sound");

            if (sounds != null)
            {
                foreach (var sound in sounds)
                {
                    var key = sound.Attribute("Key")?.Value;
                    var file = sound.Attribute("File")?.Value;
                    var category = sound.Attribute("Category")?.Value ?? "Other";

                    if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(file))
                    {
                        if (!result.ContainsKey(category))
                        {
                            result[category] = new List<(string, string)>();
                        }
                        result[category].Add((key, file));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            if (Program.DebugMode)
            {
                Console.WriteLine($"[SoundPack] Error getting sounds by category: {ex.Message}");
            }
        }

        return result;
    }

    private Dictionary<string, string>? LoadSoundPackFromXml(string xmlPath, string packDir)
    {
        try
        {
            var doc = XDocument.Load(xmlPath);
            var sounds = new Dictionary<string, string>();

            var soundElements = doc.Root?.Element("Sounds")?.Elements("Sound");
            if (soundElements == null)
            {
                return null;
            }

            foreach (var sound in soundElements)
            {
                var key = sound.Attribute("Key")?.Value;
                var file = sound.Attribute("File")?.Value;

                if (!string.IsNullOrWhiteSpace(key))
                {
                    // Allow empty file values - they represent optional/unused sounds
                    if (!string.IsNullOrWhiteSpace(file))
                    {
                        var filePath = Path.Combine(packDir, file);
                        sounds[key] = filePath;
                    }
                    else
                    {
                        sounds[key] = string.Empty; // Mark as present but no file
                    }
                }
            }

            // No validation - all keys are optional. Missing keys will be treated as empty.
            if (Program.DebugMode)
            {
                Console.WriteLine($"[SoundPack] Loaded {sounds.Count} sound entries");
            }

            return sounds;
        }
        catch (Exception ex)
        {
            if (Program.DebugMode)
            {
                Console.WriteLine($"[SoundPack] Error parsing XML: {ex.Message}");
            }
            return null;
        }
    }

    private List<string> GetRequiredSoundKeys()
    {
        return new List<string>
        {
            // General/Broadcast (allow empty values for optional sounds)
            "OpeningTheme", "CommercialIn", "CommercialOut", "ClosingTheme", 
            "HostEntrance", "CloseUnderscore", "ExplainRules", 
            "QuitSmall", "QuitLarge", "WalkAwaySmall", "WalkAwayLarge", 
            "GameOver", "ToHotSeat", "ToHotSeatLightsDown", "RiskModeActive",
            "RandomContestantPicking", "SetSafetyNet",
            
            // Fastest Finger First
            "FFFMeet2", "FFFMeet3", "FFFMeet4", "FFFMeet5", "FFFMeet6", "FFFMeet7", "FFFMeet8",
            "FFFReadQuestion", "FFFThreeNotes", "FFFThinking", "FFFReadCorrectOrder",
            "FFFOrder1", "FFFOrder2", "FFFOrder3", "FFFOrder4",
            "FFFWhoWasCorrect", "FFFWinner",
            
            // Lifelines
            "Lifeline1Ping", "Lifeline2Ping", "Lifeline3Ping", "Lifeline4Ping",
            "5050", "PAFStart", "PAFCountdown", "PAFEndEarly",
            "ATAStart", "ATAVote", "ATAEnd",
            "SwitchActivate", "SwitchShowCorrect", "SwitchClear",
            "Double1stAnswer", "Double1stFinal", "Double2ndAnswer", "Double2ndFinal",
            
            // Question Lights Down (using leading zeros for proper sorting)
            "Q01to05LightsDown", "Q06LightsDown", "Q07LightsDown", "Q08LightsDown", "Q09LightsDown", "Q10LightsDown",
            "Q11LightsDown", "Q12LightsDown", "Q13LightsDown", "Q14LightsDown", "Q15LightsDown",
            
            // Question Bed Music
            "Q01to05Bed", "Q06Bed", "Q07Bed", "Q08Bed", "Q09Bed", "Q10Bed",
            "Q11Bed", "Q12Bed", "Q13Bed", "Q14Bed", "Q15Bed",
            
            // Final Answer
            "Q01Final", "Q02Final", "Q03Final", "Q04Final", "Q05Final",
            "Q06Final", "Q07Final", "Q08Final", "Q09Final", "Q10Final",
            "Q11Final", "Q12Final", "Q13Final", "Q14Final", "Q15Final",
            
            // Correct Answer
            "Q01to04Correct", "Q05Correct", "Q05Correct2",
            "Q06Correct", "Q07Correct", "Q08Correct", "Q09Correct", "Q10Correct", "Q10Correct2",
            "Q11Correct", "Q12Correct", "Q13Correct", "Q14Correct", "Q15Correct",
            
            // Wrong Answer
            "Q01to05Wrong", "Q06Wrong", "Q07Wrong", "Q08Wrong", "Q09Wrong", "Q10Wrong",
            "Q11Wrong", "Q12Wrong", "Q13Wrong", "Q14Wrong", "Q15Wrong"
        };
    }

    private string CreateExampleXml()
    {
        return @"<?xml version=""1.0"" encoding=""utf-8""?>
<SoundPack>
  <PackName>MyCustomPack</PackName>
  <Sounds>
    <!-- General/Broadcast Sounds -->
    <Sound Key=""OpeningTheme"" File=""opening_theme.mp3"" />
    <Sound Key=""CommercialIn"" File=""commercial_in.mp3"" />
    <Sound Key=""CommercialOut"" File=""commercial_out.mp3"" />
    <Sound Key=""ClosingTheme"" File=""closing_theme.mp3"" />
    <Sound Key=""HostEntrance"" File=""host_entrance.mp3"" />
    <Sound Key=""CloseUnderscore"" File=""close_underscore.mp3"" />
    <Sound Key=""ExplainRules"" File=""explain_rules.mp3"" />
    <Sound Key=""QuitSmall"" File=""quit_small.mp3"" />
    <Sound Key=""QuitLarge"" File=""quit_large.mp3"" />
    <Sound Key=""WalkAwaySmall"" File=""walk_away_small.mp3"" />
    <Sound Key=""WalkAwayLarge"" File=""walk_away_large.mp3"" />
    <Sound Key=""GameOver"" File=""game_over.mp3"" />
    <Sound Key=""ToHotSeat"" File=""to_hotseat.mp3"" />
    <Sound Key=""ToHotSeatLightsDown"" File=""to_hotseat_lights.mp3"" />
    <Sound Key=""RiskModeActive"" File=""risk_mode.mp3"" />
    <Sound Key=""RandomContestantPicking"" File=""random_pick.mp3"" />
    <Sound Key=""SetSafetyNet"" File=""safety_net.mp3"" />
    
    <!-- Fastest Finger First Sounds -->
    <Sound Key=""FFFMeet2"" File=""fff_meet_2.mp3"" />
    <Sound Key=""FFFMeet3"" File=""fff_meet_3.mp3"" />
    <Sound Key=""FFFMeet4"" File=""fff_meet_4.mp3"" />
    <Sound Key=""FFFMeet5"" File=""fff_meet_5.mp3"" />
    <Sound Key=""FFFMeet6"" File=""fff_meet_6.mp3"" />
    <Sound Key=""FFFMeet7"" File=""fff_meet_7.mp3"" />
    <Sound Key=""FFFMeet8"" File=""fff_meet_8.mp3"" />
    <Sound Key=""FFFReadQuestion"" File=""fff_question.mp3"" />
    <Sound Key=""FFFThreeNotes"" File=""fff_three_notes.mp3"" />
    <Sound Key=""FFFThinking"" File=""fff_think.mp3"" />
    <Sound Key=""FFFReadCorrectOrder"" File=""fff_correct_order.mp3"" />
    <Sound Key=""FFFOrder1"" File=""fff_order_1.mp3"" />
    <Sound Key=""FFFOrder2"" File=""fff_order_2.mp3"" />
    <Sound Key=""FFFOrder3"" File=""fff_order_3.mp3"" />
    <Sound Key=""FFFOrder4"" File=""fff_order_4.mp3"" />
    <Sound Key=""FFFWhoWasCorrect"" File=""fff_reveal.mp3"" />
    <Sound Key=""FFFWinner"" File=""fff_winner.mp3"" />
    
    <!-- Lifeline Sounds -->
    <Sound Key=""Lifeline1Ping"" File=""lifeline1_ping.mp3"" />
    <Sound Key=""Lifeline2Ping"" File=""lifeline2_ping.mp3"" />
    <Sound Key=""Lifeline3Ping"" File=""lifeline3_ping.mp3"" />
    <Sound Key=""Lifeline4Ping"" File=""lifeline4_ping.mp3"" />
    <Sound Key=""5050"" File=""fifty_fifty.mp3"" />
    <Sound Key=""PAFStart"" File=""paf_start.mp3"" />
    <Sound Key=""PAFCountdown"" File=""paf_countdown.mp3"" />
    <Sound Key=""PAFEndEarly"" File=""paf_end_early.mp3"" />
    <Sound Key=""ATAStart"" File=""ata_start.mp3"" />
    <Sound Key=""ATAVote"" File=""ata_vote.mp3"" />
    <Sound Key=""ATAEnd"" File=""ata_end.mp3"" />
    <Sound Key=""SwitchActivate"" File=""switch_start.mp3"" />
    <Sound Key=""SwitchShowCorrect"" File=""switch_reveal.mp3"" />
    <Sound Key=""SwitchClear"" File=""switch_clear.mp3"" />
    <Sound Key=""Double1stAnswer"" File=""double_1st_answer.mp3"" />
    <Sound Key=""Double1stFinal"" File=""double_1st_final.mp3"" />
    <Sound Key=""Double2ndAnswer"" File=""double_2nd_answer.mp3"" />
    <Sound Key=""Double2ndFinal"" File=""double_2nd_final.mp3"" />
    
    <!-- Question Lights Down (Questions 1-15) -->
    <Sound Key=""Q01to05LightsDown"" File=""q01-05_lights_down.mp3"" />
    <Sound Key=""Q06LightsDown"" File=""q06_lights_down.mp3"" />
    <Sound Key=""Q07LightsDown"" File=""q07_lights_down.mp3"" />
    <Sound Key=""Q08LightsDown"" File=""q08_lights_down.mp3"" />
    <Sound Key=""Q09LightsDown"" File=""q09_lights_down.mp3"" />
    <Sound Key=""Q10LightsDown"" File=""q10_lights_down.mp3"" />
    <Sound Key=""Q11LightsDown"" File=""q11_lights_down.mp3"" />
    <Sound Key=""Q12LightsDown"" File=""q12_lights_down.mp3"" />
    <Sound Key=""Q13LightsDown"" File=""q13_lights_down.mp3"" />
    <Sound Key=""Q14LightsDown"" File=""q14_lights_down.mp3"" />
    <Sound Key=""Q15LightsDown"" File=""q15_lights_down.mp3"" />
    
    <!-- Question Bed Music (Questions 1-15) -->
    <Sound Key=""Q01to05Bed"" File=""q01-05_bed.mp3"" />
    <Sound Key=""Q06Bed"" File=""q06_bed.mp3"" />
    <Sound Key=""Q07Bed"" File=""q07_bed.mp3"" />
    <Sound Key=""Q08Bed"" File=""q08_bed.mp3"" />
    <Sound Key=""Q09Bed"" File=""q09_bed.mp3"" />
    <Sound Key=""Q10Bed"" File=""q10_bed.mp3"" />
    <Sound Key=""Q11Bed"" File=""q11_bed.mp3"" />
    <Sound Key=""Q12Bed"" File=""q12_bed.mp3"" />
    <Sound Key=""Q13Bed"" File=""q13_bed.mp3"" />
    <Sound Key=""Q14Bed"" File=""q14_bed.mp3"" />
    <Sound Key=""Q15Bed"" File=""q15_bed.mp3"" />
    
    <!-- Final Answer Sounds (Questions 1-15) -->
    <Sound Key=""Q01Final"" File=""q01_final.mp3"" />
    <Sound Key=""Q02Final"" File=""q02_final.mp3"" />
    <Sound Key=""Q03Final"" File=""q03_final.mp3"" />
    <Sound Key=""Q04Final"" File=""q04_final.mp3"" />
    <Sound Key=""Q05Final"" File=""q05_final.mp3"" />
    <Sound Key=""Q06Final"" File=""q06_final.mp3"" />
    <Sound Key=""Q07Final"" File=""q07_final.mp3"" />
    <Sound Key=""Q08Final"" File=""q08_final.mp3"" />
    <Sound Key=""Q09Final"" File=""q09_final.mp3"" />
    <Sound Key=""Q10Final"" File=""q10_final.mp3"" />
    <Sound Key=""Q11Final"" File=""q11_final.mp3"" />
    <Sound Key=""Q12Final"" File=""q12_final.mp3"" />
    <Sound Key=""Q13Final"" File=""q13_final.mp3"" />
    <Sound Key=""Q14Final"" File=""q14_final.mp3"" />
    <Sound Key=""Q15Final"" File=""q15_final.mp3"" />
    
    <!-- Correct Answer Sounds -->
    <Sound Key=""Q01to04Correct"" File=""q01-04_correct.mp3"" />
    <Sound Key=""Q05Correct"" File=""q05_correct.mp3"" />
    <Sound Key=""Q05Correct2"" File=""q05_correct_alt.mp3"" />
    <Sound Key=""Q06Correct"" File=""q06_correct.mp3"" />
    <Sound Key=""Q07Correct"" File=""q07_correct.mp3"" />
    <Sound Key=""Q08Correct"" File=""q08_correct.mp3"" />
    <Sound Key=""Q09Correct"" File=""q09_correct.mp3"" />
    <Sound Key=""Q10Correct"" File=""q10_correct.mp3"" />
    <Sound Key=""Q10Correct2"" File=""q10_correct_alt.mp3"" />
    <Sound Key=""Q11Correct"" File=""q11_correct.mp3"" />
    <Sound Key=""Q12Correct"" File=""q12_correct.mp3"" />
    <Sound Key=""Q13Correct"" File=""q13_correct.mp3"" />
    <Sound Key=""Q14Correct"" File=""q14_correct.mp3"" />
    <Sound Key=""Q15Correct"" File=""q15_correct.mp3"" />
    
    <!-- Wrong Answer Sounds -->
    <Sound Key=""Q01to05Wrong"" File=""q01-05_wrong.mp3"" />
    <Sound Key=""Q06Wrong"" File=""q06_wrong.mp3"" />
    <Sound Key=""Q07Wrong"" File=""q07_wrong.mp3"" />
    <Sound Key=""Q08Wrong"" File=""q08_wrong.mp3"" />
    <Sound Key=""Q09Wrong"" File=""q09_wrong.mp3"" />
    <Sound Key=""Q10Wrong"" File=""q10_wrong.mp3"" />
    <Sound Key=""Q11Wrong"" File=""q11_wrong.mp3"" />
    <Sound Key=""Q12Wrong"" File=""q12_wrong.mp3"" />
    <Sound Key=""Q13Wrong"" File=""q13_wrong.mp3"" />
    <Sound Key=""Q14Wrong"" File=""q14_wrong.mp3"" />
    <Sound Key=""Q15Wrong"" File=""q15_wrong.mp3"" />
  </Sounds>
</SoundPack>";
    }
}
