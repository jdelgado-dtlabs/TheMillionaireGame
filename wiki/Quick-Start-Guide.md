# Quick Start Guide

Get up and running with The Millionaire Game in 5 minutes! This guide walks you through your first game.

---

## Before You Begin

✅ [System Requirements](System-Requirements) met  
✅ [Installation](Installation) completed  
✅ Application launched successfully

---

## Step 1: Launch and Initial Setup (1 minute)

### Launch the Application
1. Double-click the desktop shortcut or Start Menu entry
2. The **Control Panel** window opens - this is your operator interface
3. Other screens (TV Screen, Host Screen, Guest Screen) can be opened via the **Screens** menu

### First-Time Setup
On first launch, the application will:
- Initialize the database (creates settings and WAPS tables)
- Create default configuration files

### Database Question Loading
The application does NOT automatically load questions. You must populate the question database:

**Option A: During Installation**
- Check the "Initialize SQL Server database" option during install
- This runs the `init_database.sql` script automatically

**Option B: After Installation (SQL Script)**
1. Locate `init_database.sql` in the application directory
2. Run the script against your SQL Server Express instance
3. This populates:
   - Main game questions (80 generic trivia questions)
   - FFF questions (41 ordering questions)

**Option C: Question Editor (Easiest)**
1. In Control Panel, go to **Game → Editor**
2. Use the **Questions** tab for main game questions
3. Use the **FFF Questions** tab for Fastest Finger First questions
4. Add questions manually through the editor interface, or:
   - Click **Import** to load questions from CSV file
   - Click **Export** to save questions to CSV file

> 💡 **Tip**: If using multiple monitors, configure screen assignments via **Game → Settings → Screens** tab for automatic positioning.

---

## Step 2: Configure Display (30 seconds)

The application provides multiple screens for different purposes:
- **TV Screen** - Main audience display
- **Host Screen** - Private host view (main game only)
- **Guest Screen** - Contestant view (main game only)
- **Preview Screen** - Supervision tool for all screens

### Single Monitor Setup
1. Control Panel opens by default
2. Open other screens via **Screens** menu as needed
3. Use **Preview Screen** to supervise what's being displayed
4. Manual window positioning is supported but not persistent

### Multi-Monitor Setup (Recommended)
1. Connect monitors (supports 2, 3, or 4 monitor setups)
2. Configure Windows display settings (Extend displays)
3. Go to **Game** → **Settings** → **Screens** tab
4. Assign each screen to a specific display:
   - Each display can only be assigned to one screen
   - Assignments are persistent and screens auto-maximize on startup
5. Configured screens will automatically open and position themselves

> 💡 **Tip**: Use screen assignments for persistent configurations. Manual drag-and-drop is not saved between sessions.

---

## Step 3: Configure Game Settings (1 minute)

### Money Tree Configuration

Set up your prize ladder and currency options:

1. Go to **Game** → **Settings** → **Money Tree** tab
2. Configure prize amounts:
   - Use default money tree or customize each level
   - **Enable Second Currency** - Run games with two currencies simultaneously
   - **Custom Currencies** - Create your own currency definitions
   - **Assign Currency to Questions** - Set which currency applies to specific questions
3. Configure safety nets:
   - Check boxes to set safety nets at Question 5 and Question 10
   - **Partial Risk Mode** - Unset one safety net (contestants risk more)
   - **Full Risk Mode** - Unset all safety nets (automatic when none are selected)

### Web Audience Participation (Optional)

Enable mobile voting for Ask the Audience and FFF Online:

1. Go to **Game** → **Settings** → **Audience** tab
2. Configure Web-based Audience Participation System (WAPS):
   - **IP Restriction**: Choose access level
     - Public - Accessible from anywhere
     - Local Network - Only local network devices
     - Localhost Only - This computer only
   - **Port**: Set port number (default: 5278)
   - **Start at Application Launch** - Auto-start web server when app opens
   - **Start/Stop Web Server** - Control server manually
3. Click **Apply** to save settings

> 💡 **Tip**: For your first game, use default money tree settings and skip WAPS configuration.

---

## Step 4: Start a New Game (1 minute)

The Control Panel uses a sequential button workflow to progress through the game:

### 1. Click "Host Intro"
- Plays host entrance audio
- Enables **Pick Player** and **Reset Game** buttons
- Ready to select contestant

### 2. Click "Pick Player"
- Opens FFF (Fastest Finger First) Window
- Automatically detects mode based on WAPS server status:
  - **Online Mode**: Web-based FFF with connected participants
  - **Offline Mode**: Local player selection interface
- **Optional**: You can close the FFF window immediately to skip contestant selection
  - FFF is a "game within a game" feature - not required for main gameplay
  - Useful if you already know who the contestant is

### 3. Click "Explain Game" (Optional)
- Plays background music
- Host can:
  - Demo the money tree (visual presentation)
  - Ping lifelines (test/demonstrate lifeline buttons)
  - Explain game rules using their own script (not automated)
- **Can be skipped**: Click "Lights Down" directly to begin gameplay

### 4. Click "Lights Down"
- Begins the main game round
- Loads first question
- Gameplay starts

### After Round Completion
- Click **Reset Round** to return to "Pick Player" state
- Select new contestant and repeat

### After Game Completion (Closing Sequence)
The **Closing** button becomes available when:
- A question is answered (Q5-Q14)
- Contestant loses
- Contestant walks away
- Contestant wins the grand prize (Q15)

**Closing Sequence:**
1. Click **Closing** button
   - If clicked after Q5-Q14: Plays interrupt sound cue (signals early game closure)
   - Host can say goodbyes and wrap up the show
   - Final theme music plays
2. Telemetry file is created with complete game statistics
3. Game session is complete
4. Click **Reset Game** to start entirely new game session
   - Re-enables "Host Intro" button
   - Begins fresh game session from scratch

**Important Notes:**
- **One session at a time**: Each game session can have multiple rounds (multiple contestants)
- **Reset Game vs Reset Round**:
  - **Reset Round**: Returns to "Pick Player" for same game session
  - **Reset Game**: Ends entire session, starts completely fresh
- **Resume from specific question**: Before clicking "Lights Down", select the question level the contestant should start from (useful if previous game was interrupted)
- **Question reuse**: Each application restart resets the "used" flag on all questions in the database. This prevents premature exhaustion of your question pool. A previously answered question may appear again in a new session.

> 📝 **Future Enhancement**: The application will calculate available rounds based on unused questions per difficulty level and notify if insufficient questions remain. A manual reset option will be available in **Game → Editor** menu under **Reset Used**.

> 💡 **Quick Start Tip**: For your first test, click Host Intro → Pick Player (close the **FFF Control window** immediately, not the main Control Panel) → Lights Down to jump straight into gameplay.

---

## Step 5: Play the Game (5-10 minutes)

### Fastest Finger First (FFF) - Two Modes

The FFF window automatically detects which mode to use based on WAPS server status:

#### **Online Mode** (Web-based with WAPS)
Participants join via mobile devices and compete in real-time:

1. **FFF Intro** - Plays lights down and intro sound
2. **Load Question** - Question is loaded automatically (no dropdown selection)
3. **Reveal Answers** - Displays answers in scrambled order to participants
   - Starts 20-second countdown timer
   - Activates web interface for participants to reorder and submit answers
4. **View Submissions** - Monitor answers as they come in real-time
5. **Reveal Correct Answers** - Click multiple times to reveal the correct order one answer at a time
6. **Show Rankings** - Displays results:
   - If 2-8 players answered correctly: Shows top 2-8 fastest players with names and times
   - If only 1 player answered correctly: Skips directly to winner reveal
7. **Reveal Winner** - Announces the winning contestant
   - **Important**: Winner info remains on screen while FFF Window is open
   - Close window only after host has acknowledged winner (closing early clears the display)

#### **Offline Mode** (Local Selection)
Manual player management with animation:

1. **Select Player Count** - Choose 2-8 players
2. **Enter Player Names** - Fill in names to be displayed on TV Screen
3. **FFF Intro** - Plays lights down sound
4. **Player Intro** - Each player introduced on TV Screen (3-second intervals)
5. **Random Select** - Animated random selection with sound effects
6. **Winner Selected** - Selected player becomes the contestant

---

### Main Game Flow

After clicking **Lights Down**, gameplay follows different patterns based on question level:

#### Questions 1-5 (Lightning Round)
- **Lights Down** (clicked once at start of Q1 only)
- Question displays automatically after lights down cue ends
- Click **Question** button to reveal each answer one at a time (A → B → C → D)
- After correct answer: Click **Question** button again to load next question
- No additional Lights Down needed between Q1-Q5

#### Questions 6-15 (Dramatic Play)
- **Lights Down** (required for each question)
- Question displays automatically after lights down cue
- Click **Question** button to reveal each answer one at a time (A → B → C → D)
- More dramatic pacing with extended music and tension

---

### Gameplay Sequence

#### 1. Question and Answer Presentation
- Lights Down sound plays (Q1 or Q6+)
- Question text appears on TV Screen
- Click **Question** button repeatedly to reveal answers A → B → C → D one at a time
- Background "bed" music plays (intensity increases with question level)
- Lifelines are in standby mode (orange, not clickable) until all answers revealed

#### 2. Contestant Consideration
Once all answers are revealed:
- Lifelines become active (available lifelines turn green)
- Contestant can now use lifelines before selecting answer
- **Walk Away** button becomes available (contestant can quit and keep winnings)

#### 3. Lifelines (Configurable)

**Configuration:** **Game → Settings → Lifelines** tab
- Select number of lifelines (2-4)
- Choose which lifelines to enable

**Available Lifelines:**

**50:50** - Removes two wrong answers, leaving one correct and one incorrect

**Phone a Friend**:
1. First click: Plays intro segment
2. Second click: Starts 30-second countdown timer (call begins)
3. Optional third click: Interrupt and end Phone a Friend early
4. If timer expires: Host hangs up call externally (not controlled by game)

**Ask the Audience**:
- **Offline Mode**: Simulated audience vote
  - Correct answer wins by "majority"
  - Percentage and degree are randomized to simulate real audience
- **Online Mode (WAPS)**: Real audience voting via mobile devices
  - 60-second countdown timer
  - Real-time vote tracking
  - Auto-proceeds when all participants have voted

**Ask the Host** - Host gives their opinion on the answer
- **Note**: Enabling this lifeline disables showing the host the correct answer beforehand

**Switch Question** - Loads new question of same difficulty (one-time use per game)

**Double Dip** - Allows two answer attempts
- Contestant can give first answer
- If incorrect, gets second chance
- Lifeline is used if either attempt is correct
- If both attempts are incorrect, normal loss rules apply
- Available once per round

#### 4. Answer Selection and Lock-In
1. Contestant verbally states their answer choice (A, B, C, or D)
2. Host asks: "Is that your final answer?"
3. Press corresponding answer button (**A**, **B**, **C**, or **D**) in Control Panel (or Stream Deck if equipped)
4. Answer is now locked in - cannot be changed
5. Only the **Reveal** button can be pressed next

#### 5. Answer Reveal
**Questions 1-5**: Reveal happens quickly after lock-in

**Questions 6+**: Host can add dramatic delay before reveal
- **With Stream Deck**: Host controls reveal timing with dedicated button
- **Without Stream Deck**: Press **Reveal** button in Control Panel
- Correct/Incorrect animation and sound plays
- Money tree updates with new winnings amount

#### 6. Next Question or End Round

**If Answer is Correct:**
- Prize won and added to winnings
- Correct answer animation and sound cue plays

**Loading Next Question (Q1-Q5):**
- Click **Question** button to load next question
- Question and answers appear immediately (no Lights Down needed)
- Repeat from Question Presentation

**Loading Next Question (Q6+):**
- Click **Lights Down** button for next question
- Lights down cue plays, then question loads automatically
- Click **Question** button to reveal answers one at a time
- Repeat from Question Presentation

**Milestone Celebrations (Q5 and Q10 - Optional):**

If safety nets are enabled, reaching Q5 or Q10 triggers special fanfare:

1. **Correct Answer**: Extended fanfare cue plays (signifies milestone reached)
2. **Host Commentary**: Host can discuss the safety net achievement
3. **Money Tree Display** (Optional):
   - Click **Money Tree** button in Control Panel
   - Shows current state of play with highlighted milestone
   - Button changes to **Lock In Safety**
4. **Lock In Animation** (Optional):
   - Click **Lock In Safety** to play celebration sound and animation
   - Adds dramatic emphasis to milestone achievement
   - **Can be skipped**: Safety net still applies whether locked in or not
5. Continue with next question as normal

> 💡 **Note**: Milestone celebrations are purely for dramatic effect. Skipping the Lock In animation has no impact on gameplay - the safety net remains active.

**If Answer is Incorrect:**
- Game ends
- Winnings drop to safety net amount (Q5 or Q10) or zero if no safety net reached
- **Closing** button becomes available

**Walk Away Option:**
- Available after all answers are revealed
- Allows contestant to assess the question before committing
- Useful when lifelines are exhausted and contestant unsure of answer
- Click **Walk Away** to end round and keep current winnings
- **Closing** button becomes available

---

## Step 6: Review Results (30 seconds)

### Round End Sequences

**If Contestant Wins or Walks Away:**
1. Automatic sequence begins
2. **Lower Third Strap**: Winnings displayed first
3. **Full Screen Display**: Shows "Winner" with final prizes
   - Both currencies displayed if dual currency mode enabled
4. **Closing** button becomes available

**If Contestant Loses:**
1. Loss cue plays, then stops automatically
2. Control Panel: Click **Walk Away** button to continue
   - This pause allows host time to discuss the incorrect answer
   - Host can provide context or explain the correct answer
3. After clicking Walk Away, ending sequence continues
4. **Closing** button becomes available

### Game Console
Review the game log in Control Panel for round-by-round details:
- Questions asked
- Lifelines used
- Answer selections
- Timing information

**To Open Game Console:**
1. Go to **Game** → **Settings** → **Screens** tab
2. Click **Game Console** button at the bottom
3. Console window opens showing entire log and tails new entries in real-time

**Log Files Location:**
- Logs are saved to: `%LocalAppData%\TheMillionaireGame\Logs\`
- Filename format: `YYYY-MM-DD_HH-MM-SS_game.log`
- Example: `2026-01-04_13-45-30_game.log`
- Maximum 5 log files retained (automatic cleanup)

### Telemetry Data

**Important**: Telemetry is only available after the **entire game session** ends:

- **Game Session** = One or multiple rounds (one or more contestants)
- **Session Ends** = When **Closing** button is pressed
- **Automatic Export** = Telemetry file automatically created when Closing sequence completes
  - If application is closed before Closing sequence finishes, data is **lost**
  - No recovery possible for incomplete sessions

**Telemetry File Location:**
- Auto-exported to: `%LocalAppData%\TheMillionaireGame\Telemetry\`
- Filename format: `MillionaireGame_Telemetry_YYYYMMDD_HHmmss.xlsx`
- Example: `MillionaireGame_Telemetry_20260104_134530.xlsx`
- Excel format (XLSX) with complete game statistics:
  - Round-by-round performance
  - Questions asked and answers given
  - Lifelines used and timing
  - Final winnings and results

> ⚠️ **Warning**: Always allow the Closing sequence to complete before closing the application. Incomplete sessions cannot be recovered.

> 📝 **Future Enhancement**: Database persistence will be added to allow recovery and partial export of incomplete game sessions.

---

## Quick Tips for Your First Game

### Audio Controls
**Location**: **Game → Settings → Sounds** tab

**Audio Mixer Tab:**
- **Silence Detection**: Auto-ends sound cues when volume drops below set dB level
  - Helpful for sounds with long tails that fade out
  - Prevents waiting for complete silence
- **Crossfade Settings**: Smooth transitions between cues
  - Prevents audio gaps between sound effects
  - Configurable fade duration

**Audio Processing:**
- **Three Gain Controls**:
  - **Master**: Overall output volume
  - **Effects**: Most sounds (buttons, correct/wrong answers, reveals)
  - **Music**: Beds and Explain Game background music
- Background tension music auto-adjusts intensity by question level

### Game Flow
- **Don't Rush**: Let sound effects and animations play naturally
- **Sound Cue Queuing**: Rapid button clicks are queued
  - System waits for current sound cue to finish before playing next
  - Prevents overlapping or cutting off important audio
- **Priority Cues**: Some cues interrupt immediately to move game forward
  - **Lights Down** has immediate priority
  - Other cues wait in queue
- **No Pause Function**: Game cannot be paused mid-round

### Common Mistakes
- ❌ Clicking buttons too quickly (let sound cues finish naturally)
- ❌ Forgetting to enable full screen for TV Screen
- ❌ Not testing audio levels before live event (adjust gain controls in Audio Mixer)
- ❌ Skipping the Explain Game phase (good opportunity to test displays and lifelines)
- ✅ Run a complete practice game first!

---

## Hotkey Reference

| Action | Key | Description |
|--------|-----|-------------|
| **Answer A** | `F1` | Select answer A |
| **Answer B** | `F2` | Select answer B |
| **Answer C** | `F3` | Select answer C |
| **Answer D** | `F4` | Select answer D |
| **New Question** | `F5` | Load next question |
| **Reveal Answer** | `F6` | Reveal selected answer |
| **Lights Down** | `F7` | Play lights down cue |
| **Lifeline 1** | `F8` | Activate first configured lifeline |
| **Lifeline 2** | `F9` | Activate second configured lifeline |
| **Lifeline 3** | `F10` | Activate third configured lifeline |
| **Lifeline 4** | `F11` | Activate fourth configured lifeline |
| **Toggle Money Tree** | `Home` | Show/hide money tree display |
| **Walk Away** | `End` | Contestant walks away |
| **Reset Game** | `Delete` | Reset entire game session |
| **Toggle Question Display** | `Tab` | Show/hide question on screens |
| **Level Up** | `Page Up` | Increase question level |
| **Level Down** | `Page Down` | Decrease question level |
| **Risk Mode** | `R` | Toggle risk mode |

> 💡 **Note**: Lifeline hotkeys (F8-F11) activate lifelines in the order they are configured in **Game → Settings → Lifelines** tab.

---

## What's Next?

Now that you've completed your first game:

### Learn More Features
- **[User Guide](User-Guide)** - Complete feature documentation
- **Dual Currency Mode** - Run games with two currencies simultaneously
- **Web Audience Participation** - Let audience vote with their phones
- **Custom Sounds** - Create your own sound sets

### Customize Your Experience
1. **Question Management**:
   - Import questions from CSV
   - Create themed question sets
   - Organize by difficulty
   
2. **Game Customization**:
   - Customize money trees
   - Configure different lifeline combinations

3. **Display Settings**:
   - Adjust theme backgrounds
   - Configure monitor assignments

### Advanced Setup
- **[Web Audience Setup](User-Guide#web-audience-participation)** - Enable live voting
- **[Stream Deck Integration](Stream-Deck-Integration)** - Host controls with 6 button module
- **[Custom Sound Sets](User-Guide#audio-system)** - Create branded audio

---

## Troubleshooting Quick Fixes

### TV Screen Not Visible
- Check if behind Control Panel
- Move to correct monitor

### No Sound Playing
- Check Windows volume mixer
- Verify sound files in `lib\sounds\Default\`
- Test with volume slider in Control Panel

### Questions Not Loading
- Check Questions in Question Editor **Game → Editor**
- Restart application

### Game Frozen/Crashed
- Application includes automatic crash monitoring
- Check `Logs\` folder for error reports
- See [Troubleshooting](Troubleshooting) for details

---

## Need Help?

- **Full Documentation**: [User Guide](User-Guide)
- **Common Issues**: [Troubleshooting](Troubleshooting)
- **Report Bugs**: [GitHub Issues](https://github.com/jdelgado-dtlabs/TheMillionaireGame/issues)

---

**Congratulations!** You've completed your first game. Ready to host a real event? Check out the complete [User Guide](User-Guide) for advanced features and best practices.
