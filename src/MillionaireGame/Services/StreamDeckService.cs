using System;
using System.Drawing;
using System.IO;
using OpenMacroBoard.SDK;
using StreamDeckSharp;
using MillionaireGame.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MillionaireGame.Services
{
    /// <summary>
    /// Manages Stream Deck device connection, button rendering, and input events.
    /// Provides host control interface for answer lock-in and reveal during gameplay.
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
                // Open first available Stream Deck device
                _device = StreamDeck.OpenDevice();

                if (_device == null)
                {
                    GameConsole.Warn("[StreamDeck] No Stream Deck device found");
                    return false;
                }

                // Subscribe to events
                _device.ConnectionStateChanged += OnConnectionStateChanged;
                _device.KeyStateChanged += OnKeyStateChanged;

                _isConnected = true;
                GameConsole.Info($"[StreamDeck] Connected to Stream Deck device");

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
            if (!_isConnected || _device == null) return;

            try
            {
                string imagePath = Path.Combine(_imageBasePath, filename);
                
                if (!File.Exists(imagePath))
                {
                    GameConsole.Warn($"[StreamDeck] Image not found: {filename}");
                    SetButtonBlank(row, col);
                    return;
                }

                // Convert row/col to Stream Deck key index
                // Stream Deck uses single index: index = (row * columns) + col
                int keyIndex = (row * 3) + col;

                // Load image and set to key
                using (var image = SixLabors.ImageSharp.Image.Load<Rgb24>(imagePath))
                {
                    var keyBitmap = KeyBitmap.Create.FromImageSharpImage(image);
                    _device.SetKeyBitmap(keyIndex, keyBitmap);
                }
            }
            catch (Exception ex)
            {
                GameConsole.Error($"[StreamDeck] Failed to set button image: {ex.Message}");
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
                // Convert key index to row/col
                int row = e.Key / 3;
                int col = e.Key % 3;

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
