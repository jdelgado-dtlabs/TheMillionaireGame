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

**Layout:**

1. **Menu Bar** (Top)
   - Game menu (Settings, Editor, Exit)
   - Screens menu (Host Screen, Guest Screen, TV Screen, Preview Screen)
   - Help menu (Usage, About)

2. **Answer Buttons** (Left Side)
   - A, B, C, D buttons
   - Click to select contestant's answer

3. **Question Display** (Center)
   - Question text area
   - Four answer text fields (A, B, C, D)
   - Explanation text area

4. **Broadcast Flow Buttons** (Right Side - Top)
   - Host Intro, Pick Player, Lights Down, Explain Game
   - Question, Reveal, Walk Away, Closing
   - Fade Out All Sounds, Stop All Audio

5. **Lifeline Buttons** (Right Side - Middle)
   - Up to 4 buttons that are reprogrammable via Settings → Lifelines
   - Default configuration: ATA, PAF, 50:50, Switch Question
   - Number and types can be customized (1-4 lifelines total)

6. **Money Tree and Risk Mode** (Right Side)
   - Show Money Tree (toggle display)
   - Activate Risk Mode (yellow button)

7. **Reset Buttons** (Right Side - Bottom)
   - Round (Reset Round)
   - Game (Reset Game)

8. **Game Status** (Bottom Section)
   - Question # selector and current level
   - Money displays (If Correct, If Walk, Current, If Wrong)
   - Questions left counter
   - Host Note with Send/Clear buttons
   - Display options checkboxes:
     - Show Question on TV
     - Show Current Winnings
     - Show Correct Answer to Host

**Note:** The Money Tree button (Item 6) affects visibility on the TV Screen. Game Console and Web Server Console are shown when the associated button in Settings → Screens is clicked. They show by default when in debug mode.

### TV Screen (Broadcast Display)

Main visual display window for the audience and broadcast/streaming output.

**Display:**
- Customizable background (Settings → Broadcast tab):
  - 6 pre-rendered backgrounds
  - Black background option
  - Custom background file option
  - Chroma key mode for streaming
- Question and 4 answers (lower third, when active)
- Lifeline results overlays (50:50 eliminations, ATA poll charts)
- Money tree display (full screen, when shown)
- FFF contestant list and results
- Win/Lose screens with animations

**Features:**
- Pre-rendered graphics (auto-scales from 1920x1080 base)
- Animated transitions between display modes
- Confetti effects on wins (higher question levels)
- Background displays when no game content is active
- Supports fullscreen mode for presentations
- Multiple display modes automatically switch based on game state

**Display Modes:**
- **Background Only**: Shows selected background when idle
- **FFF Mode**: Fastest Finger First contestant list and results
- **Question Mode**: Current question with 4 answers
- **Lifeline Results**: 50:50 elimination, Ask the Audience poll chart
- **Money Tree**: Prize ladder display
- **Win/Lose Screens**: Final results with animations and confetti

**Purpose:** Primary broadcast output for audience viewing, streaming/recording, and projection. Unlike Guest/Host screens, TV Screen focuses on presentation and visual impact rather than gameplay information.

Access: Screens menu → TV Screen

### Guest Screen (Contestant Display)

Separate window shown to the contestant during the main game phase.

**Display:**
- Money tree (always visible, right side, upper 2/3 height)
- Current question and 4 answers (lower third)
- Lifeline icons with status indicators
- Ask the Audience results (when active)
- Phone a Friend / Ask the Audience timers (when active)
- Black background (not customizable)

**Features:**
- Shows contestant's selected answer highlighted
- Displays correct/wrong answer reveals
- Shows 50:50 eliminated answers
- Auto-scales from 1920x1080 base resolution
- Money tree highlights current level

**Purpose:** Provides clear view of question, answers, and progress for contestant without showing correct answer or host notes.

Access: Screens menu → Guest Screen

### Host Screen

Separate window for the host/moderator to view during gameplay.

**Display:**
- Money tree (always visible, right side, upper 2/3 height)
- Current question and 4 answers (lower third)
- **Correct answer highlighted** (key difference from Guest Screen)
- Lifeline icons with status indicators
- Ask the Audience results (when active)
- Phone a Friend / Ask the Audience timers (when active)
- Host notes from Control Panel
- Black background (not customizable)

**Features:**
- Shows contestant's selected answer highlighted
- Displays correct/wrong answer reveals
- Shows 50:50 eliminated answers
- Auto-scales from 1920x1080 base resolution
- Money tree highlights current level

**Purpose:** Allows host/moderator to see the correct answer and game status without revealing it to the contestant. Use Host Screen for the moderator and Guest Screen for the contestant.

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
   - Check boxes at Level 5 and Level 10 to enable/disable safety nets
   - **First Safety Net**: Level 5 (e.g., $1,000) - fixed level
   - **Second Safety Net**: Level 10 (e.g., $32,000) - fixed level
   - Contestants cannot fall below these amounts once reached
   - Safety net levels cannot be changed; only enabled or disabled

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

The qualifying round where contestants compete. FFF automatically switches between two modes based on WAPS (Web Audience Participation Server) status.

#### FFF Modes

**Online Mode** (WAPS Running):
- Participants join via mobile devices
- Compete in real-time ordering question
- Fastest correct answer wins
- Automatic tracking and ranking

**Offline Mode** (WAPS Not Running):
- Manual player selection
- Animated random selection process
- No ordering question involved
- Visual player introduction sequence

#### Online Mode Setup (Web-Based)

1. **Start FFF**
   - Click **"Pick Player"** button in Control Panel
   - FFF Control window opens automatically in Online Mode (if WAPS is running)

2. **Intro + Explain** (Button 1)
   - Plays FFF lights down sound
   - Followed by FFF explain sound
   - Button turns gray when complete
   - **Show Question** button becomes available (green)

3. **Show Question** (Button 2)
   - Randomly selects an ordering question from database
   - Displays question text ONLY on TV Screen (answers hidden)
   - Question details shown in Control Panel for reference
   - Plays FFF read question sound
   - Button turns yellow, **Reveal Answers Start** button becomes available (green)

4. **Reveal Answers Start** (Button 3)
   - Plays three-note reveal sound
   - Transmits question and 4 answers to all participants' devices
   - Displays answers on TV Screen in original order (A, B, C, D)
   - Starts 20-second countdown timer on TV and participant devices
   - Plays FFF thinking music (20 seconds)
   - Participants reorder answers on their devices and submit
   - **Answer Submissions** list shows real-time responses as they arrive

5. **View Submissions**
   - **Participants** panel: Shows all connected participants
   - **Answer Submissions** panel: Lists participant name, answer sequence, and time
   - **Timer Status**: Displays countdown (00:20 to 00:00)
   - When timer expires:
     - FFF read correct order sound plays (background bed)
     - **Reveal Correct** button becomes available (green)

6. **Reveal Correct** (Button 4 - Multi-Click)
   - Click 4 times to reveal correct order one answer at a time
   - Each click:
     - Plays corresponding reveal sound (Order 1, 2, 3, 4)
     - Shows next answer in correct position on TV Screen
     - Button shows progress: "4. Reveal Correct (1/4)", "(2/4)", "(3/4)"
   - Example: If correct order is DCBA (Mercury → Venus → Earth → Mars):
     - Click 1: Shows D in position A
     - Click 2: Shows C in position B
     - Click 3: Shows B in position C
     - Click 4: Shows A in position D
   - After 4th click:
     - If 2+ contestants correct: **Show Winners** button becomes available (green)
     - If 0-1 contestant correct: Skips to **Confirm Winner** button (green)

7. **Show Winners** (Button 5 - Conditional)
   - Only appears if 2 or more contestants answered correctly
   - Plays "Who Was Correct" sound
   - Displays top 8 correct contestants on TV Screen with times
   - Names sorted alphabetically
   - Times shown in seconds (e.g., "5.32s")
   - **Confirm Winner** button becomes available (green)

8. **✗ Confirm Winner** (Button 6)
   - Announces the fastest correct contestant as winner
   - Plays FFF winner sound
   - Displays winner name on TV Screen with highlighted strap
   - Winner info shown in **Rankings Winner** panel
   - Winner automatically selected for main game
   - **Important**: Keep FFF window open until host acknowledges winner
   - Closing window clears TV display

9. **Proceed to Main Game**
   - Close FFF Control window after winner is announced
   - Winner becomes main game contestant automatically
   - TV Screen clears to background

#### Offline Mode Setup (Local Selection)

1. **Start FFF**
   - Click **"Pick Player"** button
   - FFF window opens automatically in Offline Mode (if WAPS is not running)

2. **Select Player Count**
   - Choose 2-8 players using dropdown
   - Default: 8 players

3. **Enter Player Names**
   - Fill in text boxes with contestant names
   - Names will be displayed on TV Screen

4. **FFF Intro**
   - Click **"FFF Intro"** button
   - Plays lights down sound

5. **Player Introduction**
   - Click **"Player Intro"** button
   - Each player introduced on TV Screen individually
   - 3-second interval between players
   - All players shown together after introductions

6. **Random Selection**
   - Click **"Random Select"** button
   - Animated selection sequence on TV Screen
   - Random picker sound plays
   - Selected player highlighted as winner

7. **Proceed to Main Game**
   - Selected player becomes main game contestant
   - Close FFF window (clears TV display)
   - Winner automatically selected for main game

**Note:** Offline mode does NOT use ordering questions. It's purely a visual selection tool for live events without web participation.

### Main Game Flow

After selecting a contestant through FFF (or starting directly), the main game proceeds through the following stages.

#### Starting Main Game

1. **After FFF Winner Selected:**
   - Close FFF Control window
   - Winner automatically transferred to main game
   - Control Panel ready for main game
   - **Explain Game** button enabled (green)
   - **Lights Down** button enabled (green)

2. **Optional: Explain Game** (Before First Question)
   - Click **"Explain Game"** button
   - Plays explanation music on loop
   - Demonstrates lifelines (yellow demo mode)
   - Shows money tree and game rules to contestant
   - When ready to start, proceed to Lights Down

3. **Begin Questions:**
   - **Questions 1-5 (Lightning Round):**
     - Click **"Lights Down"** ONCE at start of Q1
     - Question 1 loads automatically after lights down cue
     - After Q1 answered correctly, click **"Question"** to load Q2-Q5
     - No additional Lights Down needed between Q1-Q5
   
   - **Questions 6-15 (Dramatic Play):**
     - Click **"Lights Down"** before EACH question
     - Lights down cue plays, then question loads automatically
     - More dramatic pacing with individual lights down per question

#### Playing Questions

**Question Sequence:**

1. **Question Appears**
   - After Lights Down cue finishes, question text displays on TV Screen
   - Question shown in Control Panel for operator reference
   - Thinking music (bed music) starts automatically
   - Question checkbox auto-checked

2. **Reveal Answers** (Operator Action)
   - Click **"Question"** button to reveal answers one at a time
   - **First click**: Shows Answer A
   - **Second click**: Shows Answer B
   - **Third click**: Shows Answer C
   - **Fourth click**: Shows Answer D
   - All four answers now visible to contestant

3. **Host Reads Question** (Optional)
   - Host reads question aloud to contestant
   - Contestant sees question and all 4 answers on Guest Screen
   - Host sees question, answers, and correct answer highlighted on Host Screen

4. **Contestant Considers**
   - Contestant thinks aloud
   - Can use lifelines (see [Lifelines](#lifelines))
   - Can walk away (keeps current winnings)
   - Operator activates lifelines via buttons

5. **Contestant Gives Answer**
   - Contestant verbally states answer (A, B, C, or D)
   - Operator clicks corresponding button in Control Panel (A, B, C, or D)

6. **Lock in Answer**
   - Host confirms: "Is that your final answer?"
   - Operator clicks **"Reveal"** button
   - Final answer sound plays (Questions 5+)
   - Dramatic pause...

7. **Answer Revealed**
   - **Correct**: 
     - TV Screen shows correct answer highlighted
     - Prize won sound plays
     - Confetti animation (higher levels)
     - Money displays update (If Correct, Current, etc.)
     - Proceed to next level
   - **Wrong**:
     - Wrong answer sound
     - Correct answer revealed
     - Drop to safety net (if applicable)
     - Game ends, walk away sequence begins

8. **Next Question:**
   - **Questions 1-5**: Click **"Question"** button to load next question immediately
   - **Questions 6-15**: Click **"Lights Down"** button, then repeat from step 1

#### Walking Away

Contestant can walk away after the question and all 4 answers have been revealed:

**Requirements:**
- Question text must be displayed
- All 4 answers (A, B, C, D) must be revealed
- Available before selecting an answer
- Can be used instead of using a lifeline

**Walk Away Process:**

1. Contestant says "I'd like to walk away"
2. Operator clicks **"Walk Away"** button
3. Walk away music plays (small or large based on question level)
4. Contestant keeps current prize
5. Correct answer revealed on TV Screen
6. Game ends, walk away sequence completes

**Strategy Note:** Walking away is often wise on difficult questions when substantial money is at risk. Contestants should consider walking away when they're unsure and have significant winnings to protect.

#### Winning the Game

Reach Level 15 and answer correctly:
- Top prize won!
- Confetti animation
- Victory music
- Final screen with total winnings
- Game statistics saved automatically
- Telemetry export available after closing sequence completes

---

## Lifelines

Each lifeline can be used once per game (unless profile specifies otherwise).

### 50:50 Lifeline

**How It Works:**
1. Contestant requests "50:50"
2. Operator clicks **"50:50"** button (or `F8` if first lifeline slot)
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
2. Operator clicks **"Phone a Friend"** button (or `F9` if second lifeline slot)
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
- Stop early with `Stop Timer` button

### Ask the Audience

**How It Works:**
1. Contestant requests "Ask the Audience"
2. Operator clicks **"Ask the Audience"** button (or `F10` if third lifeline slot)
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
   - Click **"Ask the Audience"** button
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
2. Operator clicks **"Switch Question"** button (or `F11` if fourth lifeline slot)
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
   - Operator clicks "Ask the Audience" button
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

The TV screen supports background customization through Settings → Broadcast tab:

**Background Mode Options:**

1. **Theme Background (Prerendered)**
   - 6 built-in backgrounds optimized for 1920x1080
   - Black background option (no image)
   - Custom: Load your own background image file
   - High-quality scaling to any resolution

2. **Chroma Key (Solid Color)**
   - Solid color background for OBS/streaming green screen keying
   - Default: Blue (#0000FF) - safe choice
   - Avoid: Green, Red, Yellow, Orange (conflicts with game UI)
   - Recommended: Blue or Magenta for best keying results
   - Custom color picker available

**To Change Background:**
1. Control Panel → Game menu → Settings
2. Navigate to Broadcast tab
3. Select mode: Theme Background or Chroma Key
4. For Theme Background: Choose from dropdown (Background 1-6, Black, or Custom)
5. For Chroma Key: Click color button to select color
6. Click OK to apply

**Notes:**
- Changes apply to TV Screen only (Host and Guest screens use black backgrounds)
- Custom background images should be 1920x1080 for best quality
- Background displays when no game content is active

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
