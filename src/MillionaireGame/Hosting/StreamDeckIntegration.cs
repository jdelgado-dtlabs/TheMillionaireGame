using System;
using MillionaireGame.Services;
using MillionaireGame.Core.Game;
using MillionaireGame.Core.Models;
using MillionaireGame.Utilities;

namespace MillionaireGame.Hosting
{
    /// <summary>
    /// Bridges StreamDeckService with game logic, coordinating host control actions
    /// with game state and ControlPanelForm operations.
    /// </summary>
    public class StreamDeckIntegration : IDisposable
    {
        private readonly StreamDeckService _streamDeck;
        private readonly GameService _gameService;
        private readonly ScreenUpdateService _screenService;
        private char? _lockedAnswer;
        private bool _isCorrectAnswer;
        private bool _disposed;

        public bool IsEnabled { get; private set; }

        public StreamDeckIntegration(StreamDeckService streamDeck, GameService gameService, ScreenUpdateService screenService)
        {
            _streamDeck = streamDeck ?? throw new ArgumentNullException(nameof(streamDeck));
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
            _screenService = screenService ?? throw new ArgumentNullException(nameof(screenService));

            // Subscribe to Stream Deck events
            _streamDeck.AnswerButtonPressed += OnAnswerButtonPressed;
            _streamDeck.RevealButtonPressed += OnRevealButtonPressed;
            _streamDeck.DeviceConnected += OnDeviceConnected;
            _streamDeck.DeviceDisconnected += OnDeviceDisconnected;
        }

        /// <summary>
        /// Initialize Stream Deck integration (attempt device connection).
        /// </summary>
        public bool Initialize()
        {
            try
            {
                if (_streamDeck.Initialize())
                {
                    IsEnabled = true;
                    GameConsole.Info("[StreamDeckIntegration] Initialized successfully");
                    ResetButtons(); // Start with all buttons disabled
                    return true;
                }
                else
                {
                    IsEnabled = false;
                    GameConsole.Warn("[StreamDeckIntegration] No device found, integration disabled");
                    return false;
                }
            }
            catch (Exception ex)
            {
                IsEnabled = false;
                GameConsole.Error($"[StreamDeckIntegration] Initialization failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Shut down Stream Deck integration.
        /// </summary>
        public void Shutdown()
        {
            IsEnabled = false;
            _streamDeck.Shutdown();
            GameConsole.Info("[StreamDeckIntegration] Shut down");
        }

        #region Game State Synchronization

        /// <summary>
        /// Called when a new question is displayed to the contestant.
        /// Enables answer buttons and resets state.
        /// </summary>
        public void OnQuestionDisplayed()
        {
            if (!IsEnabled || !_streamDeck.IsConnected) return;

            try
            {
                _lockedAnswer = null;
                _isCorrectAnswer = false;

                // Enable answer buttons
                _streamDeck.EnableAnswerButtons();

                // Clear dynamic button
                _streamDeck.ClearDynamicButton();

                // Disable reveal button
                _streamDeck.DisableReveal();

                GameConsole.Debug("[StreamDeckIntegration] Question displayed - buttons enabled");
            }
            catch (Exception ex)
            {
                GameConsole.Error($"[StreamDeckIntegration] OnQuestionDisplayed error: {ex.Message}");
            }
        }

        /// <summary>
        /// Called when the current question/round ends.
        /// Disables all buttons.
        /// </summary>
        public void OnQuestionEnd()
        {
            if (!IsEnabled || !_streamDeck.IsConnected) return;

            try
            {
                ResetButtons();
                GameConsole.Debug("[StreamDeckIntegration] Question ended - buttons reset");
            }
            catch (Exception ex)
            {
                GameConsole.Error($"[StreamDeckIntegration] OnQuestionEnd error: {ex.Message}");
            }
        }

        /// <summary>
        /// Called when game state changes (e.g., game over, walk away).
        /// Disables all buttons.
        /// </summary>
        public void OnGameStateChanged()
        {
            if (!IsEnabled || !_streamDeck.IsConnected) return;

            try
            {
                // If game is not in active question state, disable buttons
                if (_screenService.GetCurrentQuestion() == null)
                {
                    ResetButtons();
                }
            }
            catch (Exception ex)
            {
                GameConsole.Error($"[StreamDeckIntegration] OnGameStateChanged error: {ex.Message}");
            }
        }

        /// <summary>
        /// Reset all buttons to disabled state.
        /// </summary>
        private void ResetButtons()
        {
            _streamDeck.DisableAnswerButtons();
            _streamDeck.ClearDynamicButton();
            _streamDeck.DisableReveal();
            _lockedAnswer = null;
            _isCorrectAnswer = false;
        }

        #endregion

        #region Stream Deck Event Handlers

        /// <summary>
        /// Handles answer button press from Stream Deck (host locks in answer).
        /// </summary>
        private void OnAnswerButtonPressed(object? sender, char answer)
        {
            if (!IsEnabled || !_streamDeck.IsConnected) return;

            try
            {
                // Validate answer is valid (A, B, C, D)
                if (answer != 'A' && answer != 'B' && answer != 'C' && answer != 'D')
                {
                    GameConsole.Warn($"[StreamDeckIntegration] Invalid answer button: {answer}");
                    return;
                }

                // Check if we have an active question
                var currentQuestion = _screenService.GetCurrentQuestion();
                if (currentQuestion == null)
                {
                    GameConsole.Warn("[StreamDeckIntegration] No active question - ignoring button press");
                    return;
                }

                // Check if already locked
                if (_lockedAnswer.HasValue)
                {
                    GameConsole.Debug($"[StreamDeckIntegration] Answer already locked: {_lockedAnswer.Value}");
                    return;
                }

                // Lock the answer
                _lockedAnswer = answer;

                // Determine if answer is correct
                _isCorrectAnswer = (currentQuestion.CorrectAnswer == answer.ToString());

                // Update Stream Deck display
                _streamDeck.LockAnswer(answer); // Highlight locked answer, disable others

                if (_isCorrectAnswer)
                {
                    _streamDeck.ShowCorrectIndicator(); // Green check on dynamic button
                }
                else
                {
                    _streamDeck.ShowIncorrectIndicator(); // Red X on dynamic button
                }

                _streamDeck.EnableReveal(); // Enable reveal button

                GameConsole.Info($"[StreamDeckIntegration] Answer {answer} locked by host ({(_isCorrectAnswer ? "CORRECT" : "INCORRECT")})");

                // Raise event for ControlPanelForm to handle
                AnswerLockedByHost?.Invoke(this, new AnswerLockedEventArgs(answer, _isCorrectAnswer));
            }
            catch (Exception ex)
            {
                GameConsole.Error($"[StreamDeckIntegration] OnAnswerButtonPressed error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles reveal button press from Stream Deck (host reveals answer).
        /// </summary>
        private void OnRevealButtonPressed(object? sender, EventArgs e)
        {
            if (!IsEnabled || !_streamDeck.IsConnected) return;

            try
            {
                // Check if answer is locked
                if (!_lockedAnswer.HasValue)
                {
                    GameConsole.Warn("[StreamDeckIntegration] No answer locked - ignoring reveal");
                    return;
                }

                GameConsole.Info($"[StreamDeckIntegration] Reveal triggered by host for answer {_lockedAnswer.Value}");

                // Raise event for ControlPanelForm to handle
                RevealTriggeredByHost?.Invoke(this, new RevealEventArgs(_lockedAnswer.Value, _isCorrectAnswer));

                // Clear dynamic button after reveal
                _streamDeck.ClearDynamicButton();

                // Disable all buttons (question is over)
                _streamDeck.DisableAnswerButtons();
                _streamDeck.DisableReveal();

                // Reset state
                _lockedAnswer = null;
                _isCorrectAnswer = false;
            }
            catch (Exception ex)
            {
                GameConsole.Error($"[StreamDeckIntegration] OnRevealButtonPressed error: {ex.Message}");
            }
        }

        private void OnDeviceConnected(object? sender, EventArgs e)
        {
            IsEnabled = true;
            GameConsole.Info("[StreamDeckIntegration] Device connected");
            DeviceStatusChanged?.Invoke(this, new DeviceStatusEventArgs(true));
        }

        private void OnDeviceDisconnected(object? sender, EventArgs e)
        {
            IsEnabled = false;
            GameConsole.Warn("[StreamDeckIntegration] Device disconnected");
            DeviceStatusChanged?.Invoke(this, new DeviceStatusEventArgs(false));
        }

        #endregion

        #region Events for ControlPanelForm

        /// <summary>
        /// Fired when host locks in an answer via Stream Deck.
        /// ControlPanelForm should handle final answer logic.
        /// </summary>
        public event EventHandler<AnswerLockedEventArgs>? AnswerLockedByHost;

        /// <summary>
        /// Fired when host triggers reveal via Stream Deck.
        /// ControlPanelForm should handle reveal logic.
        /// </summary>
        public event EventHandler<RevealEventArgs>? RevealTriggeredByHost;

        /// <summary>
        /// Fired when device connection status changes.
        /// </summary>
        public event EventHandler<DeviceStatusEventArgs>? DeviceStatusChanged;

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (!_disposed)
            {
                // Unsubscribe from events
                _streamDeck.AnswerButtonPressed -= OnAnswerButtonPressed;
                _streamDeck.RevealButtonPressed -= OnRevealButtonPressed;
                _streamDeck.DeviceConnected -= OnDeviceConnected;
                _streamDeck.DeviceDisconnected -= OnDeviceDisconnected;

                Shutdown();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    #region Event Args

    /// <summary>
    /// Event arguments for answer locked by host.
    /// </summary>
    public class AnswerLockedEventArgs : EventArgs
    {
        public char Answer { get; }
        public bool IsCorrect { get; }

        public AnswerLockedEventArgs(char answer, bool isCorrect)
        {
            Answer = answer;
            IsCorrect = isCorrect;
        }
    }

    /// <summary>
    /// Event arguments for reveal triggered by host.
    /// </summary>
    public class RevealEventArgs : EventArgs
    {
        public char Answer { get; }
        public bool IsCorrect { get; }

        public RevealEventArgs(char answer, bool isCorrect)
        {
            Answer = answer;
            IsCorrect = isCorrect;
        }
    }

    /// <summary>
    /// Event arguments for device status change.
    /// </summary>
    public class DeviceStatusEventArgs : EventArgs
    {
        public bool IsConnected { get; }

        public DeviceStatusEventArgs(bool isConnected)
        {
            IsConnected = isConnected;
        }
    }

    #endregion
}
