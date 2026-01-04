# User Guide

Complete documentation for using The Millionaire Game. This guide covers all features, settings, and advanced functionality.

---

## Table of Contents

1. [Interface Overview](#interface-overview)
2. [Game Setup](#game-setup)
3. [Question Management](#question-management)
4. [Running a Game](#running-a-game)
5. [Lifelines](#lifelines)
6. [Audio System](#audio-system)
7. [Display Configuration](#display-configuration)
8. [Web Audience Participation](#web-audience-participation)
9. [Telemetry and Statistics](#telemetry-and-statistics)
10. [Advanced Features](#advanced-features)

---

## Interface Overview

The application consists of multiple windows, each serving a specific purpose.

### Control Panel (Operator Interface)

The Control Panel is your command center for running the game.

**Main Sections:**

1. **Game Controls** (Top)
   - New Game, Start FFF, Start Main Game
   - Question navigation
   - Final Answer, Walk Away, End Game

2. **Question Display** (Center-Left)
   - Current question text
   - Four answer options (A, B, C, D)
   - Answer selection buttons

3. **Money Tree** (Center-Right)
   - 15 prize levels
   - Current level highlighted
   - Safety nets marked
   - Dual currency display (if enabled)

4. **Lifelines** (Bottom-Left)
   - 50:50, Phone a Friend, Ask the Audience, Switch Question
   - Status indicators (available/used)
   - Quick-access hotkeys

5. **Game Console** (Bottom)
   - Live event log
   - Debug information
   - Error messages
   - Game state updates

### TV Screen (Player/Audience Display)

The TV Screen is the main visual display for contestants and audience.

**Display Modes:**

- **Title Screen**: Shows logo and "Ready to Play"
- **FFF Mode**: Fastest Finger First question and results
- **Question Mode**: Current question with 4 answers
- **Lifeline Results**: 50:50 elimination, audience poll chart
- **Win/Lose Screens**: Final results with animations


**Visual Elements:**

- Pre-rendered graphics (auto-scales from 1920x1080 base)
- Animated transitions
- Confetti effects (on wins)
- 6 selectable backgrounds
- Chroma key support for streaming

### Host Screen (Optional)

Separate window for host/moderator with:
- Current question and correct answer highlighted
- Money tree display with current level
- Ask the Audience results (when active)
- Lifeline status indicators

Access: Screens menu → Host Screen

---

## Game Setup

### Configuring Game Settings

Customize your game configuration (money tree, lifelines, currency) through the Settings dialog.

#### Accessing Settings

1. **Open Settings Dialog**
   - Control Panel → **Game** menu → **Settings**
   - Opens Settings window with multiple tabs

2. **Money Tree Configuration** (Money Tree tab)

   **Currency Options:**
   - **Currency 1**: Select primary currency ($, €, £, ¥, or custom text)
   - **Currency 2**: Optional second currency for dual-currency games
   - Enable "Currency 2" checkbox to use dual currency mode
   - Set currency position (prefix or suffix)
   - Assign which currency applies to each question level

3. **Prize Values**

   **Default Money Tree (US Version):**
   ```
   Level 15: $1,000,000
   Level 14: $500,000
   Level 13: $250,000
   Level 12: $125,000
   Level 11: $64,000
   Level 10: $32,000  ← Safety Net
   Level 9:  $16,000
   Level 8:  $8,000
   Level 7:  $4,000
   Level 6:  $2,000
   Level 5:  $1,000   ← Safety Net
   Level 4:  $500
   Level 3:  $300
   Level 2:  $200
   Level 1:  $100
   ```

   **Custom Money Tree:**
   - Edit any prize amount
   - Use any currency format
   - Can create themed ladders (points, tokens, etc.)

4. **Safety Nets**
   - Check boxes at Level 5 and Level 10 to set safety nets
   - **First Safety Net**: Typically Level 5 (e.g., $1,000)
   - **Second Safety Net**: Typically Level 10 (e.g., $32,000)
   - Contestants cannot fall below these amounts once reached

5. **Lifeline Configuration** (Lifelines tab)
   
   **Total Lifelines**: Set how many lifelines are available (1-4)
   
   **Configure Each Lifeline:**
   - **Lifeline Type**: Choose from 50:50, Phone a Friend (Plus One), Ask the Audience, Ask the Host, Switch Question
   - **Availability**: Set when lifeline becomes active:
     - Always Available
     - After Question 5
     - After Question 10
     - In Risk Mode Only

6. **Save Settings**
   - Click **"OK"** to save all changes
   - Settings apply immediately to the game

**Example Configurations:**
- **Educational**: Use "Points" as currency, lower prize values (100-10,000)
- **International Event**: Enable Currency 2, assign different currencies to different question levels
- **High Stakes**: Standard US money tree ($100 - $1M), all 4 lifelines enabled
- **Challenge Mode**: Disable safety nets, limit lifelines to 2-3

---

## Question Management

### Understanding Question Levels

Questions are organized into **5 difficulty levels**:

| Level Range | Difficulty | Description |
|-------------|------------|-------------|
| 1-5 | Easy | General knowledge, simple facts |
| 6-10 | Medium | Requires more specific knowledge |
| 11-14 | Hard | Specialized knowledge, challenging |
| 15 | Final | Extremely difficult, often obscure |

The game randomly selects questions from the appropriate level range.

### Creating Questions

#### Using the Question Editor

1. **Open Question Editor**
   - Control Panel → **Game** menu → **Editor**
   - Opens standalone Question Editor window
   - Two tabs: **Regular Questions** and **FFF Questions**

2. **Add New Question**
   - Click **"Add"** button in toolbar

2. **Enter Question Details**
   ```
   Question Text: "What is the capital of France?"
   
   Answer A: London
   Answer B: Paris         ← Mark as Correct
   Answer C: Berlin
   Answer D: Rome
   
   Difficulty Level: 1 (Easy)
   Category: Geography (optional)
   ```

4. **Set Difficulty**
   - Level 1-5: Easy
   - Level 6-10: Medium
   - Level 11-14: Hard
   - Level 15: Final question

5. **Save Question**
   - Click **"Save"**
   - Question added to database

### Importing Questions

#### CSV Import Format

Create a CSV file with this structure:

```csv
Question,AnswerA,AnswerB,AnswerC,AnswerD,CorrectAnswer,Level,Category
"What is 2+2?","3","4","5","6","B",1,"Math"
"Who painted the Mona Lisa?","Michelangelo","Da Vinci","Raphael","Donatello","B",3,"Art"
```

**Import Steps:**
1. Open Question Editor (Game → Editor)
2. Click **"Import"** button in toolbar
3. Select your CSV file
4. Choose question type (Regular or FFF)
5. Click **"Import"**
6. Click **"Refresh"** to see imported questions

### Organizing Questions

**Best Practices:**
- Maintain balanced distribution across difficulty levels
- Aim for 20-30 questions per level minimum
- Use categories for themed game nights
- Regularly review and update questions
- Test questions before live events

**Question Database:**
- Stored in SQL Server Express database: `dbMillionaire`
- Tables: `Questions` (regular) and `FFFQuestions` (Fastest Finger First)

**Backup Questions:**
1. Open Question Editor (Game → Editor)
2. Click **"Export"** button in toolbar
3. Choose question type (Regular or FFF)
4. Save as CSV file

---

## Running a Game

### Fastest Finger First (FFF)

The qualifying round where multiple contestants compete.

#### Setup FFF Round

1. **Start FFF**
   - Click **"Start FFF"** button
   - Or press `Ctrl+F` hotkey

2. **Enter Contestants**
   - Add 2-8 contestant names
   - Assign seat numbers (1-8)
   - Click **"Begin FFF Round"**

3. **FFF Question Appears**
   - TV Screen shows ordering question
   - Example: "Order these planets by distance from Sun:"
     - A: Mars
     - B: Earth
     - C: Venus
     - D: Mercury
   - Correct order: D, C, B, A (Mercury → Venus → Earth → Mars)

4. **Record Contestant Responses**
   - Each contestant gives their answer order
   - Enter times (e.g., 5.3 seconds)
   - Enter their answer sequence (e.g., "DCBA")

5. **Show Results**
   - Click **"Show Results"**
   - TV Screen displays:
     - All contestant times
     - Correct answer highlighted
     - Winner announced
   - Fastest correct answer wins

6. **Proceed to Main Game**
   - Winner becomes main game contestant
   - Click **"Start Main Game"** with winner

### Main Game Flow

#### Starting Main Game

**Option 1: After FFF**
- Winner automatically transferred
- Click **"Start Main Game"**

**Option 2: Direct Start**
- Click **"New Game"** → **"Main Game"**
- Enter contestant name
- Click **"Begin"**

#### Playing Questions

**Question Sequence:**

1. **Question Appears**
   - TV Screen displays question and 4 answers
   - Thinking music starts automatically
   - Control Panel shows question to operator

2. **Host Reads Question**
   - Optional: Click **"Read Question"** (`Space`)
   - Plays audio narration (if available)
   - Or host reads question aloud

3. **Contestant Considers**
   - Contestant thinks aloud
   - Can use lifelines (see [Lifelines](#lifelines))
   - Can walk away (keeps current winnings)

4. **Contestant Gives Answer**
   - Contestant verbally states answer (A, B, C, or D)
   - Operator clicks corresponding button in Control Panel

5. **Lock in Answer**
   - Host confirms: "Is that your final answer?"
   - Operator clicks **"Final Answer"** (`Enter`)
   - Dramatic pause...

6. **Answer Revealed**
   - **Correct**: 
     - TV Screen shows correct answer highlighted
     - Prize won sound plays
     - Confetti animation (higher levels)
     - Proceed to next level
   - **Wrong**:
     - Wrong answer sound
     - Correct answer revealed
     - Drop to safety net (if applicable)
     - Game ends

#### Walking Away

Contestant can walk away at any time:

1. Contestant says "I'd like to walk away"
2. Operator clicks **"Walk Away"** button (`W` key)
3. Walk away music plays
4. Contestant keeps current prize
5. Correct answer revealed
6. Game ends

**Strategy Note:** Walking away is often wise on difficult questions when substantial money is at risk.

#### Winning the Game

Reach Level 15 and answer correctly:
- Top prize won!
- Confetti animation
- Victory music
- Final screen with total winnings
- Game statistics saved

---

## Lifelines

Each lifeline can be used once per game (unless profile specifies otherwise).

### 50:50 Lifeline

**How It Works:**
1. Contestant requests "50:50"
2. Operator clicks **"50:50"** button (`F1`)
3. Two incorrect answers are eliminated
4. TV Screen updates showing only 2 answers
5. Contestant now chooses between 2 options

**Example:**
```
Before 50:50:
A: London
B: Paris
C: Berlin
D: Rome

After 50:50:
A: ———
B: Paris
C: Berlin
D: ———
```

**Limitations:**
- Cannot be undone
- May not always eliminate the scariest options
- Used before final answer

### Phone a Friend

**How It Works:**
1. Contestant requests "Phone a Friend"
2. Operator clicks **"Phone a Friend"** button (`F2`)
3. 30-second countdown starts
4. Contestant calls predetermined helper
5. Reads question and answers
6. Helper provides their opinion
7. Timer expires or operator stops timer
8. Contestant decides whether to use advice

**Best Practices:**
- Pre-arrange friends with specific expertise areas
- Have phone number ready
- Keep connection active
- Brief but clear question reading

**Control Panel:**
- Timer shows countdown
- Pause with `Space` bar
- Stop early with `Stop Timer` button

### Ask the Audience

**How It Works:**
1. Contestant requests "Ask the Audience"
2. Operator clicks **"Ask the Audience"** button (`F3`)
3. Audience votes (multiple methods available)

#### Manual Vote Entry

1. Audience votes by show of hands or devices
2. Operator counts/tallies votes
3. Enter percentages in Control Panel:
   ```
   A: 10%
   B: 75%   ← Majority
   C: 8%
   D: 7%
   ```
4. Click **"Show Results"**
5. TV Screen displays bar chart with percentages

#### Web Voting (Recommended)

Enable web server for real-time audience voting:

1. **Enable Web Server**
   - Settings tab → Audience
   - Click **"Start Web Server"**
   - Note IP address and port (e.g., `192.168.1.100:5278`)

2. **Audience Joins**
   - Audience members visit URL on phones
   - Enter name/nickname
   - Wait for voting to open

3. **Activate Lifeline**
   - Click **"Ask the Audience"** (`F3`)
   - Voting automatically opens on all connected devices

4. **Audience Votes**
   - Each person selects A, B, C, or D
   - Votes tallied in real-time
   - Operator sees live results

5. **Close Voting**
   - Click **"Close Voting"** (or auto-close after 30 seconds)
   - Results displayed on TV Screen
   - Contestant sees percentage breakdown

**Benefits:**
- Accurate vote tallying
- Prevents multiple votes
- Real-time engagement
- Professional presentation

### Switch Question

**How It Works:**
1. Contestant requests "Switch Question"
2. Operator clicks **"Switch Question"** button (`F4`)
3. Current question disappears
4. New question of same difficulty level appears
5. Original question cannot be returned to
6. Contestant must answer new question

**Use Cases:**
- Completely unfamiliar topic
- Confusing question wording
- Contestant has no knowledge base for question

**Limitations:**
- One-time use only
- Replacement is same difficulty (could be harder)
- Cannot switch back

---

## Audio System

The Millionaire Game features a comprehensive sound system for immersive gameplay.

### Sound Categories

#### Music
- **Intro Music**: Game opening
- **Thinking Music**: Plays during question consideration
  - Levels 1-5: Calm, steady
  - Levels 6-10: Rising tension
  - Levels 11-15: High drama, intense
- **Walk Away**: Contestant leaves with money
- **Win Music**: Top prize victory

#### Sound Effects
- **Button Clicks**: UI interactions
- **Final Answer**: Dramatic pause cue
- **Correct Answer**: Success sound
- **Wrong Answer**: Failure sound
- **Lifeline Sounds**: Each lifeline has unique sound

#### Voice (Optional)
- Question reading audio (if imported)
- Host commentary (custom recordings)

### Gain Controls

**Audio Processing Settings:**
- Settings → Sounds tab → Audio Settings tab
- Access Audio Processing section

**Gain Controls:**
- **Master Gain**: Overall audio level (-20dB to +20dB, default: 0dB)
- **Effects Gain**: Sound effects level (-20dB to +20dB, default: 0dB)
- **Music Gain**: Background music level (-20dB to +20dB, default: 0dB)

**Additional Audio Processing:**
- **Enable Limiter**: Prevents audio clipping
- **Silence Detection**: Auto-detects when sounds end
- **Crossfade**: Smooth transitions between sounds

### Sound Packs

Switch between different sound sets for variety or branding.

#### Using Built-in Sounds

Default sound set included:
```
lib/sounds/Default/
```

#### Creating Custom Sound Sets

1. **Create Folder**
   ```
   lib/sounds/MyCustomSet/
   ```

2. **Add Sound Files**
   Required files (see [Installation Guide - Sound Files](Installation#sound-file-requirements) for complete list):
   - `intro.mp3`
   - `thinking_music_1.mp3` (Levels 1-5)
   - `thinking_music_2.mp3` (Levels 6-10)
   - `thinking_music_3.mp3` (Levels 11-15)
   - `final_answer.mp3`
   - `correct_answer.mp3`
   - `wrong_answer.mp3`
   - `walk_away.mp3`
   - `lifeline_5050.mp3`
   - `lifeline_phone.mp3`
   - `lifeline_audience.mp3`
   - `win_game.mp3`

3. **Select Sound Pack**
   - Settings → Sounds tab → Soundpack tab
   - Sound Pack dropdown
   - Select "MyCustomSet"
   - Click **"Apply"**

4. **Test Sounds**
   - Use test buttons to preview
   - Adjust volumes as needed

**Audio Format Support:**
- MP3 (recommended)
- WAV
- OGG

---

## Display Configuration

### Monitor Setup

#### Single Monitor

**Layout Options:**

**Option A: Overlapping Windows**
- TV Screen maximized
- Control Panel floats on top (scaled down)
- Operator moves Control Panel as needed

**Option B: Split Screen**
- TV Screen: Upper half
- Control Panel: Lower half
- Use Windows Snap (Win + Arrow keys)

**Best For:**
- Testing
- Small venues
- Casual play

#### Dual Monitor (Recommended)

**Optimal Configuration:**

1. **Primary Monitor** (TV/Projector)
   - TV Screen full screen (`F11`)
   - 1920x1080 or higher
   - Visible to audience and contestant

2. **Secondary Monitor** (Operator Screen)
   - Control Panel maximized
   - 1366x768 or higher
   - Visible only to operator/host

**Setup Steps:**

1. **Connect Monitors**
   - Connect second display to computer
   - Windows should detect automatically

2. **Configure Windows Display**
   - Right-click desktop → Display Settings
   - Select "Extend these displays"
   - Arrange monitor positions
   - Set primary display

3. **Launch Application**
   - Application opens on primary display
   - Drag Control Panel to secondary display

4. **Maximize TV Screen**
   - Maximize TV Screen window on primary monitor
   - Enters borderless fullscreen mode
   - TV Screen fills entire primary monitor

5. **Save Layout**
   - Settings → Display
   - Check "Remember window positions"
   - Layout restored on next launch

#### Triple Monitor (Advanced)

**Configuration:**

1. **Monitor 1** (Primary): TV Screen for audience
2. **Monitor 2**: Control Panel for operator
3. **Monitor 3**: Host Screen for moderator

Useful for large events with dedicated host and operator roles.

### Display Scaling

The application supports Windows display scaling:

- **100%**: Native resolution, best performance
- **125-150%**: High-DPI displays, slight scaling
- **200%**: 4K displays, full scaling

Graphics are vector-based and scale cleanly at any resolution.

### Resolution Support

**Minimum:** 1280x720 (HD)  
**Recommended:** 1920x1080 (Full HD)  
**Supported:** Up to 4K (3840x2160)

---

## Web Audience Participation

Enable live audience interaction for the "Ask the Audience" lifeline.

### Enabling Web Server

1. **Open Settings**
   - Control Panel → Settings tab
   - Audience tab

2. **Configure Server**
   - **Port**: Default 5278
   - **Bind Address**: 
     - `127.0.0.1 - Localhost Only`: Local testing only
     - `0.0.0.0 - All Interfaces (Open to All)`: Accept connections from any network
     - Local IP with /prefix (e.g., `192.168.1.100/24 - Local Network`): Restrict to local network

3. **Start Server**
   - Click **"Start Web Server"**
   - Server status shows "Running"
   - Connection URL displayed (e.g., `http://192.168.1.100:5278`)

### Audience Connection

**Local Network:**
1. Audience members connect to same WiFi
2. Visit URL shown in Control Panel
3. Enter name/nickname
4. Join lobby

**Internet (Advanced):**
- Requires port forwarding on router
- Or use reverse proxy (ngrok, etc.)
- Configure firewall rules
- Use HTTPS for security

### Voting Process

1. **Lifeline Activated**
   - Operator clicks "Ask the Audience" (`F3`)
   - Question appears on all connected devices

2. **Audience Votes**
   - Each person selects A, B, C, or D
   - Vote submitted instantly
   - Can change vote until closed

3. **Results Display**
   - Operator closes voting (auto-closes after timer)
   - Percentages calculated
   - Results shown on TV Screen as bar chart
   - Contestant sees audience's choice

### Security Considerations

**Network Security:**
- Use secure WiFi (WPA2/WPA3)
- Change default port if desired
- Enable HTTPS for public networks
- Monitor connected users

**Fair Play:**
- One vote per connection
- Prevent early voting (questions hidden until lifeline used)
- Optional: Require codes to join

---

## Telemetry and Statistics

The application tracks comprehensive game statistics.

### Viewing Telemetry

**Note:** Telemetry viewing interface is planned for a future release. Currently, telemetry data is automatically saved to the database but there is no built-in viewer.

**Current Telemetry:**
   - List of all games played
   - Date, contestant name, final prize
   - Win/loss status
   - Questions answered

3. **Detailed View**
   - Click any game to see details:
     - Complete question list
     - Answers given (correct/wrong)
     - Lifelines used
     - Time per question
     - Dual currency breakdown

### Exporting Data

**Excel Export (XLSX):**

1. Telemetry tab → **"Export to Excel"**
2. Choose date range or select specific games
3. Select export format:
   - **Summary**: Game overview, win rates
   - **Detailed**: Question-by-question breakdown
   - **Dual Currency**: Currency comparison
4. Save file

**Excel File Contents:**
```
Sheet 1: Game Summary
- Date, Contestant, Final Prize, Outcome, Duration

Sheet 2: Question Analysis
- Question text, Difficulty, Correct answer rate

Sheet 3: Lifeline Usage
- Which lifelines used most, success rate after use

Sheet 4: Dual Currency (if enabled)
- Prize comparison between currencies
```

### Statistics Dashboard

**Key Metrics:**
- Total games played
- Win rate (reached top prize)
- Average prize won
- Most common exit point
- Lifeline effectiveness
- Question difficulty accuracy

---

## Advanced Features

### Background Selection

The TV screen supports background customization:

**6 Pre-rendered Backgrounds**: Built-in backgrounds optimized for 1920x1080

**Chroma Key Mode**: Solid color backgrounds for streaming/OBS integration
- Default: Blue (#0000FF) - safe choice
- Avoid: Colors used in game UI (green, red, yellow, orange)

**Change Background:**
- Settings tab → Broadcast tab
- Select between Theme Background (prerendered) or Chroma Key (solid color) mode
- Choose from dropdown or configure chroma key color
- Changes apply to TV screen only

> **Note**: Full theme system (including UI color schemes) planned for v1.2

### Hotkey Customization

**Viewing Hotkeys:**
- Settings → Hotkeys tab
- See all assigned shortcuts

**Customizing Hotkeys:**
1. Find action to customize
2. Click "Edit"
3. Press new key combination
4. Click "Save"

**Default Hotkeys:** (See [Quick Start Guide - Hotkey Reference](Quick-Start-Guide#hotkey-reference))

### Crash Recovery

The application includes automatic crash monitoring.

**Watchdog Service:**
- Monitors for crashes and freezes
- Automatically restarts application
- Generates diagnostic reports
- Preserves game state when possible

**Crash Reports:**
Located in `Logs/CrashReports/`
- Stack traces
- Memory dumps
- Event logs
- System information

**Reporting Crashes:**
Submit crash reports to [GitHub Issues](https://github.com/jdelgado-dtlabs/TheMillionaireGame/issues) with:
- Crash report file
- Steps to reproduce
- Expected vs actual behavior

---

## Tips for Live Events

### Pre-Event Checklist

**24 Hours Before:**
- [ ] Test full game run-through
- [ ] Verify all sounds play correctly
- [ ] Check display setup (projector/TV)
- [ ] Prepare question set (review for errors)
- [ ] Test web audience voting (if used)
- [ ] Backup database

**1 Hour Before:**
- [ ] Launch application
- [ ] Set up displays (maximize TV Screen window)
- [ ] Test audio levels with venue speakers
- [ ] Start web server (if using audience voting)
- [ ] Have audience join and test
- [ ] Run sample FFF round

**Just Before:**
- [ ] Create game profile for event
- [ ] Have contestant names ready
- [ ] Verify Control Panel accessible but hidden from audience
- [ ] Double-check sound levels
- [ ] Ready to start!

### During Event

**Best Practices:**
- Keep Control Panel on separate display (audience shouldn't see)
- Let sound effects and animations play fully
- Use hotkeys for speed (practice beforehand)
- Have backup questions ready (in case of technical issues)
- Monitor Game Console for errors

**Common Issues:**
- **Sound not playing**: Check Windows sound mixer
- **TV Screen frozen**: Press `ESC`, restart if needed
- **Web voting not working**: Verify network connection
- **Question won't load**: Skip to next question, report later

### Post-Event

- Export telemetry for records
- Backup database
- Review any issues encountered
- Update question set if needed
- Archive game data

---

## Next Steps

**Explore More:**
- [Troubleshooting](Troubleshooting) - Solutions to common problems
- [Configuration Files](Configuration-Files) - Advanced settings
- [Stream Deck Integration](Stream-Deck-Integration) - Host controls with 6 button module

**Get Help:**
- [GitHub Issues](https://github.com/jdelgado-dtlabs/TheMillionaireGame/issues) - Report bugs
- [Discussions](https://github.com/jdelgado-dtlabs/TheMillionaireGame/discussions) - Ask questions

---

**Questions not covered here?** Check [Troubleshooting](Troubleshooting) or open a [discussion](https://github.com/jdelgado-dtlabs/TheMillionaireGame/discussions)!
