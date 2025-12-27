# ControlPanelForm.cs - Deprecated Code Notes

## Deprecated: Manual Reset Button (btnResetGame_Click)

**Status**: Removed from UI  
**Deprecated**: v0.8.0  
**Reason**: Automated sequences now handle all game resets  

### Original Implementation

The manual reset button was removed because automated sequences (like `StartNewQuestionSequence`) now handle state cleanup and resets automatically. This ensures proper sequencing and prevents race conditions.

### Code Archive

```csharp
private async void btnResetGame_Click(object? sender, EventArgs e)
{
    // If automated sequence is running, cancel it
    if (_isAutomatedSequenceRunning)
    {
        _isAutomatedSequenceRunning = false;
        _automatedSequenceCts?.Cancel();
        _automatedSequenceCts?.Dispose();
        _automatedSequenceCts = null;
        
        // Stop all sounds before showing dialog
        await _soundService.StopAllSoundsAsync();
        
        // Clean up all timers
        _lifelineManager?.Reset();
        
        _closingTimer?.Stop();
        _closingTimer?.Dispose();
        _closingTimer = null;
        
        if (Program.DebugMode)
        {
            GameConsole.Info("[Reset] Cancelled automated sequence and cleaned up timers");
        }
    }
    
    // Stop all sounds before resetting
    await _soundService.StopAllSoundsAsync();
    
    // Reset lifeline state
    _lifelineManager?.Reset();
    
    // Reset closing timer
    _closingTimer?.Stop();
    _closingTimer?.Dispose();
    _closingTimer = null;
    
    _gameService.ResetGame();
    _firstRoundCompleted = true; // Mark that at least one round has been completed
    ResetAllControls();
}
```

### Migration Path

If manual reset is needed in the future:
1. Add button back to UI Designer
2. Uncomment and update method
3. Ensure proper interaction with automated sequences
4. Test with all game modes (Standard, Risk Mode, Free Safety Net)

### Related Methods

- `ResetAllControls()` - Resets UI state
- `_gameService.ResetGame()` - Resets game state
- `_lifelineManager.Reset()` - Resets lifeline state
- Automated sequences handle cleanup: `StartNewQuestionSequence()`, `HandleWrongAnswer()`, etc.
