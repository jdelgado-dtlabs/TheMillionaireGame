using System;
using System.Drawing;
using System.IO;
using System.Linq;
using OpenMacroBoard.SDK;
using StreamDeckSharp;
using MillionaireGame.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MillionaireGame.Services
{
    /// <summary>
    /// Manages Stream Deck device connection, button rendering, and input events.
    /// Provides host control interface for answer lock-in and reveal during gameplay.
    /// 
    /// IMPORTANT: Stream Deck Module 6 (USB PID 0x00B8) is NOT supported by StreamDeckSharp 6.1.0 yet.
    /// The device will not be detected. Supported devices include:
    /// - Stream Deck (15 buttons)
    /// - Stream Deck Mini (6 buttons, PID 0x0063/0x0090)
    /// - Stream Deck XL (32 buttons)
    /// - Stream Deck MK.2 (15 buttons)
    /// 
    /// For Module 6 support, either:
    /// 1. Wait for StreamDeckSharp library update
    /// 2. Use Stream Deck Mini as temporary alternative (same 6-button layout)
    /// 3. Contribute Module 6 support to github.com/OpenMacroBoard/StreamDeckSharp
    /// </summary>
    public class StreamDeckService : IDisposable
    {
        private readonly string _imageBasePath;
        private IMacroBoard? _device;
        private bool _isConnected;
        private bool _disposed;

        // Button position constants (0-indexed)
        private const int DYNAMIC_ROW = 0;
        private const int DYNAMIC_COL = 0;
        private const int ANSWER_A_ROW = 0;
        private const int ANSWER_A_COL = 1;
        private const int ANSWER_B_ROW = 0;
        private const int ANSWER_B_COL = 2;
        private const int REVEAL_ROW = 1;
        private const int REVEAL_COL = 0;
        private const int ANSWER_C_ROW = 1;
        private const int ANSWER_C_COL = 1;
        private const int ANSWER_D_ROW = 1;
        private const int ANSWER_D_COL = 2;

        // Events
        public event EventHandler<char>? AnswerButtonPressed;
        public event EventHandler? RevealButtonPressed;
        public event EventHandler? DeviceConnected;
        public event EventHandler? DeviceDisconnected;

        public bool IsConnected => _isConnected;

        public StreamDeckService()
        {
            // Image path: lib/image/streamdeck/
            _imageBasePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "lib", "image", "streamdeck"
            );
        }

        /// <summary>
        /// Initialize connection to Stream Deck device.
        /// </summary>
        /// <returns>True if device connected successfully, false otherwise.</returns>
        public bool Initialize()
        {
            try
            {
                // Enumerate all available Stream Deck devices WITHOUT opening them
                // This prevents grabbing exclusive control of wrong devices
                var devices = StreamDeck.EnumerateDevices();
                
                if (!devices.Any())
                {
                    GameConsole.Warn("[StreamDeck] No Stream Deck devices found");
                    GameConsole.Warn("[StreamDeck] NOTE: Stream Deck Module 6 (PID 0x00B8) is NOT supported by StreamDeckSharp 6.1.0");
                    GameConsole.Warn("[StreamDeck] Supported: Stream Deck, Stream Deck Mini (PID 0x0063/0x0090), Stream Deck XL, Stream Deck MK.2");
                    return false;
                }

                // Debug: Log all detected devices
                GameConsole.Debug($"[StreamDeck] Detected {devices.Count()} Stream Deck device(s):");
                foreach (var dev in devices)
                {
                    var layout = dev.Keys;
                    string layoutInfo = "unknown layout";
                    if (layout is OpenMacroBoard.SDK.GridKeyLayout grid)
                    {
                        layoutInfo = $"{grid.CountX}x{grid.CountY} grid ({grid.Area.Width}x{grid.Area.Height}px)";
                    }
                    GameConsole.Debug($"  - {dev.DeviceName}: {layout.Count} buttons, {layoutInfo}");
                }

                // Find a device with 6 buttons in 3x2 layout (Stream Deck Module 6)
                StreamDeckSharp.StreamDeckDeviceReference? targetDevice = null;
                foreach (var deviceRef in devices)
                {
                    var keyLayout = deviceRef.Keys;
                    
                    // Check button count first
                    if (keyLayout.Count != 6)
                    {
                        GameConsole.Info($"[StreamDeck] Skipping {deviceRef.DeviceName} ({keyLayout.Count} buttons) - requires 6-button device");
                        continue;
                    }

                    // Check layout dimensions
                    if (keyLayout is OpenMacroBoard.SDK.GridKeyLayout gridLayout)
                    {
                        // CountX = columns, CountY = rows (NOT Area which is pixels!)
                        if (gridLayout.CountX != 3 || gridLayout.CountY != 2)
                        {
                            GameConsole.Info($"[StreamDeck] Skipping {deviceRef.DeviceName} ({gridLayout.CountX}x{gridLayout.CountY} layout) - requires 3x2 layout");
                            continue;
                        }

                        // Found matching device!
                        targetDevice = deviceRef as StreamDeckSharp.StreamDeckDeviceReference;
                        GameConsole.Info($"[StreamDeck] Found compatible device: {deviceRef.DeviceName}");
                        break;
                    }
                    else
                    {
                        GameConsole.Warn($"[StreamDeck] {deviceRef.DeviceName} has unexpected key layout type: {keyLayout.GetType().Name}");
                    }
                }

                if (targetDevice == null)
                {
                    GameConsole.Warn("[StreamDeck] No compatible Stream Deck Module 6 (3x2 layout, 6 buttons) found. Make sure the Stream Deck app is CLOSED as it conflicts with this integration.");
                    return false;
                }

                // Now open ONLY the compatible device
                _device = targetDevice.Open();

                // Subscribe to events
                _device.ConnectionStateChanged += OnConnectionStateChanged;
                _device.KeyStateChanged += OnKeyStateChanged;

                _isConnected = true;
                GameConsole.Info($"[StreamDeck] Connected to Stream Deck Module 6 (2x3 layout)");

                // Set brightness
                _device.SetBrightness(80); // 80% brightness

                // Initialize all buttons to blank
                ClearAllButtons();

                DeviceConnected?.Invoke(this, EventArgs.Empty);
                return true;
            }
            catch (Exception ex)
            {
                GameConsole.Error($"[StreamDeck] Initialization failed: {ex.Message}");
                _isConnected = false;
                return false;
            }
        }

        /// <summary>
        /// Shut down Stream Deck connection.
        /// </summary>
        public void Shutdown()
        {
            if (_device != null)
            {
                GameConsole.Info("[StreamDeck] Shutting down");
                ClearAllButtons();
                _device.ConnectionStateChanged -= OnConnectionStateChanged;
                _device.KeyStateChanged -= OnKeyStateChanged;
                _device.Dispose();
                _device = null;
                _isConnected = false;
            }
        }

        #region Button State Management

        /// <summary>
        /// Set button image from file.
        /// </summary>
        private void SetButtonImage(int row, int col, string filename)
        {
            if (!_isConnected || _device == null)
            {
                GameConsole.Debug($"[StreamDeck] Skipping SetButtonImage (not connected): {filename}");
                return;
            }

            try
            {
                string imagePath = Path.Combine(_imageBasePath, filename);
                
                GameConsole.Info($"[StreamDeck] 🔍 Looking for: {imagePath}");
                
                if (!File.Exists(imagePath))
                {
                    GameConsole.Error($"[StreamDeck] ❌ IMAGE NOT FOUND: {imagePath}");
                    GameConsole.Info($"[StreamDeck] Base path: {_imageBasePath}");
                    SetButtonBlank(row, col);
                    return;
                }

                // Convert row/col to Stream Deck key index
                // Module 6 uses ROW-MAJOR ordering: index = (row * columns) + col
                // Physical layout: [0 1 2]
                //                  [3 4 5]
                int keyIndex = (row * 3) + col;

                GameConsole.Info($"[StreamDeck] 🔍 MAPPING: {filename} → Physical(row{row},col{col}) → KeyIndex[{keyIndex}]");

                // Load image and resize to 80x80 for Module 6 (per official specs)
                using (var image = SixLabors.ImageSharp.Image.Load<Rgb24>(imagePath))
                {
                    GameConsole.Debug($"[StreamDeck] Image loaded: {image.Width}x{image.Height}px, resizing to 80x80");
                    
                    // Resize to 80x80 for Stream Deck Module 6 (official spec)
                    image.Mutate(x => x.Resize(80, 80));
                    
                    var keyBitmap = KeyBitmap.Create.FromImageSharpImage(image);
                    _device.SetKeyBitmap(keyIndex, keyBitmap);
                    GameConsole.Debug($"[StreamDeck] Image set to key {keyIndex} successfully");
                }
            }
            catch (Exception ex)
            {
                GameConsole.Error($"[StreamDeck] Failed to set button image '{filename}': {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Set button to blank (disabled state).
        /// </summary>
        private void SetButtonBlank(int row, int col)
        {
            SetButtonImage(row, col, "blank.png");
        }

        /// <summary>
        /// Clear all buttons to blank state.
        /// </summary>
        public void ClearAllButtons()
        {
            if (!_isConnected || _device == null) return;

            try
            {
                _device.ClearKeys();
                GameConsole.Debug("[StreamDeck] All buttons cleared");
            }
            catch (Exception ex)
            {
                GameConsole.Error($"[StreamDeck] Failed to clear buttons: {ex.Message}");
            }
        }

        #endregion

        #region Answer Buttons (A, B, C, D)

        /// <summary>
        /// Enable answer buttons for question display.
        /// </summary>
        public void EnableAnswerButtons()
        {
            SetButtonImage(ANSWER_A_ROW, ANSWER_A_COL, "answer-a.png");
            SetButtonImage(ANSWER_B_ROW, ANSWER_B_COL, "answer-b.png");
            SetButtonImage(ANSWER_C_ROW, ANSWER_C_COL, "answer-c.png");
            SetButtonImage(ANSWER_D_ROW, ANSWER_D_COL, "answer-d.png");
            GameConsole.Debug("[StreamDeck] Answer buttons enabled");
        }

        /// <summary>
        /// Disable all answer buttons (show blank).
        /// </summary>
        public void DisableAnswerButtons()
        {
            SetButtonBlank(ANSWER_A_ROW, ANSWER_A_COL);
            SetButtonBlank(ANSWER_B_ROW, ANSWER_B_COL);
            SetButtonBlank(ANSWER_C_ROW, ANSWER_C_COL);
            SetButtonBlank(ANSWER_D_ROW, ANSWER_D_COL);
            GameConsole.Debug("[StreamDeck] Answer buttons disabled");
        }

        /// <summary>
        /// Highlight specific answer as locked.
        /// </summary>
        public void LockAnswer(char answer)
        {
            // Show lock icon for selected answer
            switch (answer)
            {
                case 'A':
                    SetButtonImage(ANSWER_A_ROW, ANSWER_A_COL, "answer-a-lock.png");
                    SetButtonBlank(ANSWER_B_ROW, ANSWER_B_COL);
                    SetButtonBlank(ANSWER_C_ROW, ANSWER_C_COL);
                    SetButtonBlank(ANSWER_D_ROW, ANSWER_D_COL);
                    break;
                case 'B':
                    SetButtonBlank(ANSWER_A_ROW, ANSWER_A_COL);
                    SetButtonImage(ANSWER_B_ROW, ANSWER_B_COL, "answer-b-lock.png");
                    SetButtonBlank(ANSWER_C_ROW, ANSWER_C_COL);
                    SetButtonBlank(ANSWER_D_ROW, ANSWER_D_COL);
                    break;
                case 'C':
                    SetButtonBlank(ANSWER_A_ROW, ANSWER_A_COL);
                    SetButtonBlank(ANSWER_B_ROW, ANSWER_B_COL);
                    SetButtonImage(ANSWER_C_ROW, ANSWER_C_COL, "answer-c-lock.png");
                    SetButtonBlank(ANSWER_D_ROW, ANSWER_D_COL);
                    break;
                case 'D':
                    SetButtonBlank(ANSWER_A_ROW, ANSWER_A_COL);
                    SetButtonBlank(ANSWER_B_ROW, ANSWER_B_COL);
                    SetButtonBlank(ANSWER_C_ROW, ANSWER_C_COL);
                    SetButtonImage(ANSWER_D_ROW, ANSWER_D_COL, "answer-d-lock.png");
                    break;
            }
            GameConsole.Debug($"[StreamDeck] Answer {answer} locked");
        }

        #endregion

        #region Dynamic Button (Feedback Indicator)

        /// <summary>
        /// Show correct indicator on dynamic button.
        /// </summary>
        public void ShowCorrectIndicator()
        {
            SetButtonImage(DYNAMIC_ROW, DYNAMIC_COL, "correct.png");
            GameConsole.Debug("[StreamDeck] Correct indicator shown");
        }

        /// <summary>
        /// Show incorrect indicator on dynamic button.
        /// </summary>
        public void ShowIncorrectIndicator()
        {
            SetButtonImage(DYNAMIC_ROW, DYNAMIC_COL, "wrong.png");
            GameConsole.Debug("[StreamDeck] Incorrect indicator shown");
        }

        /// <summary>
        /// Clear dynamic button to blank.
        /// </summary>
        public void ClearDynamicButton()
        {
            SetButtonBlank(DYNAMIC_ROW, DYNAMIC_COL);
        }

        #endregion

        #region Reveal Button

        /// <summary>
        /// Enable reveal button.
        /// </summary>
        public void EnableReveal()
        {
            SetButtonImage(REVEAL_ROW, REVEAL_COL, "answer-reveal.png");
            GameConsole.Debug("[StreamDeck] Reveal button enabled");
        }

        /// <summary>
        /// Disable reveal button.
        /// </summary>
        public void DisableReveal()
        {
            SetButtonBlank(REVEAL_ROW, REVEAL_COL);
            GameConsole.Debug("[StreamDeck] Reveal button disabled");
        }

        #endregion

        #region Event Handlers

        private void OnKeyStateChanged(object? sender, OpenMacroBoard.SDK.KeyEventArgs e)
        {
            // Only respond to key down events
            if (!e.IsDown) return;

            try
            {
                // Convert key index to row/col (row-major)
                int row = e.Key / 3;  // row = key_index / columns
                int col = e.Key % 3;  // col = key_index % columns

                GameConsole.Debug($"[StreamDeck] Key {e.Key} pressed → row{row},col{col}");

                // Determine which button was pressed
                if (row == ANSWER_A_ROW && col == ANSWER_A_COL)
                {
                    GameConsole.Info("[StreamDeck] Answer A pressed");
                    AnswerButtonPressed?.Invoke(this, 'A');
                }
                else if (row == ANSWER_B_ROW && col == ANSWER_B_COL)
                {
                    GameConsole.Info("[StreamDeck] Answer B pressed");
                    AnswerButtonPressed?.Invoke(this, 'B');
                }
                else if (row == ANSWER_C_ROW && col == ANSWER_C_COL)
                {
                    GameConsole.Info("[StreamDeck] Answer C pressed");
                    AnswerButtonPressed?.Invoke(this, 'C');
                }
                else if (row == ANSWER_D_ROW && col == ANSWER_D_COL)
                {
                    GameConsole.Info("[StreamDeck] Answer D pressed");
                    AnswerButtonPressed?.Invoke(this, 'D');
                }
                else if (row == REVEAL_ROW && col == REVEAL_COL)
                {
                    GameConsole.Info("[StreamDeck] Reveal pressed");
                    RevealButtonPressed?.Invoke(this, EventArgs.Empty);
                }
                // Dynamic button (row 0, col 0) has no action
            }
            catch (Exception ex)
            {
                GameConsole.Error($"[StreamDeck] Key event error: {ex.Message}");
            }
        }

        private void OnConnectionStateChanged(object? sender, ConnectionEventArgs e)
        {
            _isConnected = e.NewConnectionState;

            if (_isConnected)
            {
                GameConsole.Info("[StreamDeck] Device reconnected");
                DeviceConnected?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                GameConsole.Warn("[StreamDeck] Device disconnected");
                DeviceDisconnected?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (!_disposed)
            {
                Shutdown();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
