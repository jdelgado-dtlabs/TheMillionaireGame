# Sound Files Directory

This directory contains all MP3 audio files for The Millionaire Game.

## Current Files and Mappings

### ✅ Broadcast Flow Audio
- `host_entrance.mp3` → SoundHostStart - Host introduction music
- `explain_rules.mp3` → SoundExplainRules - Game explanation (loops)
- `walk_away_small.mp3` → SoundWalkAway1 - Walk away with small winnings
- `walk_away_large.mp3` → SoundWalkAway2 - Walk away with large winnings
- `host_end.mp3` → SoundHostEnd - Closing theme
- `close_theme.mp3` → SoundClosing - Alternative closing

### ✅ Question Level Audio

#### Lights Down
- `lights_down_classic.mp3` - Game start / Level 1 intro (Questions 1-5)
- `lights_down_1.mp3` through `lights_down_5.mp3` - For questions within levels 2, 3, and 4:
  - Level 2 (Q6-10): lights_down_1 = Q6, lights_down_2 = Q7, lights_down_3 = Q8, lights_down_4 = Q9, lights_down_5 = Q10
  - Level 3 (Q11-14): lights_down_1 = Q11, lights_down_2 = Q12, lights_down_3 = Q13, lights_down_4 = Q14
  - Level 4 (Q15): lights_down_5 only
- `q1_to_q5_lights_down.mp3` → SoundQ1to5LightsDown (legacy/alternative)

#### Question Bed Music (loops while question displayed)
- `q1_to_q5_bed.mp3` → SoundQ1to5Bed
- `q6_bed.mp3` through `q15_bed.mp3` - Level-specific beds

### ✅ Game Event Audio
- `final_answer_1.mp3` through `final_answer_5.mp3` - Final answer sounds for questions within each level
  - Level 2 (Q6-10): final_answer_1 = Q6, final_answer_2 = Q7, final_answer_3 = Q8, final_answer_4 = Q9, final_answer_5 = Q10
  - Level 3 (Q11-14): Uses final_answer_1 through final_answer_4 for Q11-14
- `FinalAnswer1Q.mp3` through `FinalAnswer5Q.mp3` - Alternative final answer sounds
- `q1_to_q4_correct.mp3`, `q5_correct.mp3`, `q6_correct.mp3`, etc. - Correct answer celebrations by question
- `q1_to_q5_lose.mp3`, `q6_lose.mp3`, `q7_lose.mp3`, etc. - Wrong answer sounds by question
- `game_over.mp3` → SoundGameOver

### ✅ Lifeline Audio
- `fifty_fifty.mp3` → Sound5050 - 50:50 activation
- `paf_start.mp3` → Phone a Friend start
- `paf_countdown.mp3` → Phone a Friend clock
- `paf_end_call_early.mp3` → End call early
- `ata_start.mp3` → SoundATAStart - Ask the Audience start
- `ata_vote.mp3` → SoundATAVoting - Audience voting
- `ata_end.mp3` → SoundATAEnd - Ask the Audience end
- `stq_start.mp3` → SoundSwitchActivate - Switch the Question start
- `stq_new_question_flip.mp3` → New question reveal
- `stq_reveal_correct_answer.mp3` → SoundSwitchShowCorrect
- `lifeline_1_on.mp3` → SoundLifeline1Ping
- `lifeline_2_on.mp3` → SoundLifeline2Ping
- `lifeline_3_on.mp3` → SoundLifeline3Ping
- `lifeline_4_on.mp3` → SoundLifeline4Ping

### ✅ Fastest Finger First
- `fastest_finger_contestants.mp3` through `fastest_finger_contestants_8.mp3` → SoundFFFMeet2-8
- `fastest_finger_read_question.mp3` → SoundFFFReadQuestion
- `fastest_finger_3_stabs.mp3` → SoundFFFThreeNotes
- `fastest_finger_think.mp3` → SoundFFFThinking
- `fastest_finger_read_answer_order.mp3` → SoundFFFReadCorrectOrder
- `fastest_finger_answer_correct_1.mp3` through `_4.mp3` → SoundFFFOrder1-4
- `fastest_finger_reveal_times.mp3` → SoundFFFWhoWasCorrect
- `fastest_finger_winner.mp3` → SoundFFFWinner
- `fastest_finger_lights_down.mp3` - FFF lights down

### ✅ Other Sounds
- `risk_mode.mp3` → SoundRiskModeActive
- `set_safety_net.mp3` → SoundSetSafetyNet
- `pick_random_contestant.mp3` → SoundRandomContestantPicking
- `commercial_in.mp3` → SoundCommercialIn
- `commercial_out.mp3` → SoundCommercialOut
- `opening_theme.mp3` → SoundOpening
- `main_theme.mp3`, `intro.mp3`, `trailer_long.mp3`, etc. - Additional themes

### ⚠️ Deprecated/Unused
- `doubledip_*.mp3` - Double Dip lifeline (not in current version)
- Plus One references removed (Phone a Friend used instead)

## File Format
- All files are MP3 format
- Files that loop have seamless loop points
- Quality: 128-192 kbps

## Usage
These files are automatically copied to the output directory during build and referenced by the SoundService class with intelligent path resolution.
