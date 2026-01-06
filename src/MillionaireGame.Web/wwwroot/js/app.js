/**
 * Who Wants to be a Millionaire
 * Audience Participation System
 * Version: 0.6.3-2512 (Device Telemetry & Privacy)
 */

// ============================================================================
// Configuration & Constants
// ============================================================================

const STORAGE_KEYS = {
    PARTICIPANT_ID: 'waps_participant_id',
    SESSION_ID: 'waps_session_id',
    DISPLAY_NAME: 'waps_display_name',
    AUTO_SESSION_ID: 'waps_auto_session_id',
    SESSION_TIMESTAMP: 'waps_session_timestamp'
};

// Session Management Configuration
const SESSION_CONFIG = {
    maxSessionDuration: 4 * 60 * 60 * 1000, // 4 hours (typical show duration)
    warningBeforeExpiry: 15 * 60 * 1000,    // 15 minutes warning
    checkInterval: 60 * 1000                 // Check every minute
};

// ============================================================================
// Device Telemetry Functions
// ============================================================================

/**
 * Detect device type (Mobile, Tablet, Desktop)
 */
function getDeviceType() {
    const ua = navigator.userAgent;
    if (/(tablet|ipad|playbook|silk)|(android(?!.*mobi))/i.test(ua)) {
        return "Tablet";
    }
    if (/Mobile|Android|iP(hone|od)|IEMobile|BlackBerry|Kindle|Silk-Accelerated|(hpw|web)OS|Opera M(obi|ini)/.test(ua)) {
        return "Mobile";
    }
    return "Desktop";
}

/**
 * Detect OS type and version
 */
function getOSInfo() {
    const ua = navigator.userAgent;
    let osType = "Unknown";
    let osVersion = "Unknown";
    
    if (/Windows NT 10/i.test(ua)) { osType = "Windows"; osVersion = "10/11"; }
    else if (/Windows NT 6.3/i.test(ua)) { osType = "Windows"; osVersion = "8.1"; }
    else if (/Windows NT 6.2/i.test(ua)) { osType = "Windows"; osVersion = "8"; }
    else if (/Windows NT 6.1/i.test(ua)) { osType = "Windows"; osVersion = "7"; }
    else if (/Mac OS X ([\d_]+)/i.test(ua)) {
        osType = "macOS";
        const match = ua.match(/Mac OS X ([\d_]+)/i);
        osVersion = match ? match[1].replace(/_/g, '.') : "Unknown";
    }
    else if (/Android ([\d.]+)/i.test(ua)) {
        osType = "Android";
        const match = ua.match(/Android ([\d.]+)/i);
        osVersion = match ? match[1] : "Unknown";
    }
    else if (/iPhone OS ([\d_]+)/i.test(ua)) {
        osType = "iOS";
        const match = ua.match(/iPhone OS ([\d_]+)/i);
        osVersion = match ? match[1].replace(/_/g, '.') : "Unknown";
    }
    else if (/iPad.*OS ([\d_]+)/i.test(ua)) {
        osType = "iOS";
        const match = ua.match(/OS ([\d_]+)/i);
        osVersion = match ? match[1].replace(/_/g, '.') : "Unknown";
    }
    else if (/Linux/i.test(ua)) { osType = "Linux"; }
    
    return { osType, osVersion };
}

/**
 * Detect browser type and version
 */
function getBrowserInfo() {
    const ua = navigator.userAgent;
    let browserType = "Unknown";
    let browserVersion = "Unknown";
    
    if (/Edg\/([\d.]+)/.test(ua)) {
        browserType = "Edge";
        const match = ua.match(/Edg\/([\d.]+)/);
        browserVersion = match ? match[1] : "Unknown";
    }
    else if (/Chrome\/([\d.]+)/.test(ua) && !/Edg/.test(ua)) {
        browserType = "Chrome";
        const match = ua.match(/Chrome\/([\d.]+)/);
        browserVersion = match ? match[1] : "Unknown";
    }
    else if (/Safari\/([\d.]+)/.test(ua) && !/Chrome/.test(ua)) {
        browserType = "Safari";
        const match = ua.match(/Version\/([\d.]+)/);
        browserVersion = match ? match[1] : "Unknown";
    }
    else if (/Firefox\/([\d.]+)/.test(ua)) {
        browserType = "Firefox";
        const match = ua.match(/Firefox\/([\d.]+)/);
        browserVersion = match ? match[1] : "Unknown";
    }
    else if (/MSIE|Trident/.test(ua)) {
        browserType = "IE";
    }
    
    return { browserType, browserVersion };
}

/**
 * Collect all device telemetry
 */
function collectDeviceTelemetry() {
    const { osType, osVersion } = getOSInfo();
    const { browserType, browserVersion } = getBrowserInfo();
    
    return {
        deviceType: getDeviceType(),
        osType,
        osVersion,
        browserType,
        browserVersion,
        hasAgreedToPrivacy: true // Set by clicking Join button
    };
}

// ============================================================================
// State Management
// ============================================================================

let connection = null;
let currentSessionId = null;
let currentParticipantId = null;
let currentDisplayName = null;
let sessionExpiryTimer = null;
let sessionWarningShown = false;
let isReconnecting = false; // Track if we're in automatic reconnection

// ATA Voting State
let ataTimerInterval = null;
let ataTimeRemaining = 0;
let ataHasVoted = false;

// ============================================================================
// Session Management Functions
// ============================================================================

/**
 * Generate or retrieve auto session ID for troubleshooting
 */
function getAutoSessionId() {
    let autoSessionId = localStorage.getItem(STORAGE_KEYS.AUTO_SESSION_ID);
    if (!autoSessionId) {
        autoSessionId = 'AUTO_' + Math.random().toString(36).substring(2, 15);
        localStorage.setItem(STORAGE_KEYS.AUTO_SESSION_ID, autoSessionId);
    }
    return autoSessionId;
}

/**
 * Get session code - always use fixed session ID for live game
 */
function getSessionCode() {
    return 'LIVE'; // Always connect to the live game session
}

/**
 * Get stored participant ID from localStorage
 */
function getStoredParticipantId() {
    return localStorage.getItem(STORAGE_KEYS.PARTICIPANT_ID);
}

/**
 * Get stored display name from localStorage
 */
function getStoredDisplayName() {
    return localStorage.getItem(STORAGE_KEYS.DISPLAY_NAME);
}

/**
 * Save session info to localStorage
 */
function saveSessionInfo(sessionId, participantId, displayName) {
    localStorage.setItem(STORAGE_KEYS.SESSION_ID, sessionId);
    localStorage.setItem(STORAGE_KEYS.PARTICIPANT_ID, participantId);
    localStorage.setItem(STORAGE_KEYS.DISPLAY_NAME, displayName);
}

/**
 * Clear session info from localStorage
 */
function clearSessionInfo() {
    localStorage.removeItem(STORAGE_KEYS.SESSION_ID);
    localStorage.removeItem(STORAGE_KEYS.PARTICIPANT_ID);
    localStorage.removeItem(STORAGE_KEYS.DISPLAY_NAME);
}

// ============================================================================
// SignalR Connection & Event Handlers
// ============================================================================

/**
 * Join session and establish SignalR connection
 */
async function joinSession(sessionId, displayName, participantId = null) {
    try {
        hideError();

        // Collect device telemetry (anonymous, for statistics only)
        const telemetry = collectDeviceTelemetry();
        console.log("Device telemetry collected:", telemetry);

        // Create SignalR connection with custom retry policy
        connection = new signalR.HubConnectionBuilder()
            .withUrl(`/hubs/game`)
            .withAutomaticReconnect({
                nextRetryDelayInMilliseconds: retryContext => {
                    // Exponential backoff: 0s, 2s, 5s, 10s, 20s, 30s, then 30s intervals
                    if (retryContext.previousRetryCount === 0) return 0;
                    if (retryContext.previousRetryCount === 1) return 2000;
                    if (retryContext.previousRetryCount === 2) return 5000;
                    if (retryContext.previousRetryCount === 3) return 10000;
                    if (retryContext.previousRetryCount === 4) return 20000;
                    return 30000; // Max 30 seconds between retries
                }
            })
            .build();

        // Handle reconnecting state
        connection.onreconnecting(error => {
            console.warn('SignalR reconnecting...', error);
            isReconnecting = true; // Set flag
            const attempt = error?.previousRetryCount || 0;
            showConnectionOverlay('Lost Connection', `Reconnecting... (attempt ${attempt + 1})`);
        });

        // Handle successful reconnection
        connection.onreconnected(async connectionId => {
            console.log('SignalR reconnected:', connectionId);
            showConnectionOverlay('Reconnected', 'Rejoining session...');
            
            // Try to rejoin session with saved credentials
            try {
                const result = await connection.invoke("JoinSession", currentSessionId, currentDisplayName, currentParticipantId, telemetry);
                if (result.success) {
                    console.log('&#x2713; Silently rejoined session after reconnection');
                    
                    // Update with potentially new IDs (if server restarted and reassigned)
                    currentSessionId = result.sessionId;
                    currentParticipantId = result.participantId;
                    currentDisplayName = result.displayName;
                    
                    // Save updated info
                    saveSessionInfo(currentSessionId, currentParticipantId, currentDisplayName);
                    
                    // Update UI with new values
                    document.getElementById('displaySessionId').textContent = currentSessionId;
                    document.getElementById('lblDisplayName').textContent = currentDisplayName;
                    document.getElementById('displayParticipantId').textContent = currentParticipantId;
                    document.getElementById('displayConnectionId').textContent = connectionId;
                    
                    // Check participant state and redirect to appropriate screen
                    // If server restarted, participant will be in Lobby state
                    if (result.state && result.state !== 'SelectedForFFF') {
                        console.log('Server may have restarted - participant state is:', result.state);
                        console.log('Redirecting to lobby (connectedScreen)');
                        showScreen('connectedScreen');
                    }
                    
                    hideConnectionOverlay();
                    isReconnecting = false; // Clear flag
                } else {
                    // Server restarted - session no longer exists, clear and restart
                    console.warn('Session no longer exists - server may have restarted');
                    clearAllData();
                    hideConnectionOverlay();
                    isReconnecting = false; // Clear flag
                    showError('Server restarted. Please join the session again.');
                    await connection.stop();
                    connection = null;
                    showScreen('joinScreen');
                }
            } catch (err) {
                console.error('Failed to rejoin session:', err);
                clearAllData();
                hideConnectionOverlay();
                isReconnecting = false; // Clear flag
                showError('Server restarted. Please join the session again.');
                await connection.stop();
                connection = null;
                showScreen('joinScreen');
            }
        });

        // Handle connection closed (reconnection attempts exhausted or manual close)
        connection.onclose(async error => {
            if (error) {
                console.error('SignalR connection closed with error:', error);
                // Don't give up - keep trying to reconnect
                showConnectionOverlay('Connection Lost', 'Reconnecting to server...');
                
                // Wait a bit before attempting manual reconnection
                await new Promise(resolve => setTimeout(resolve, 5000));
                
                // Attempt to reconnect
                try {
                    console.log('Attempting manual reconnection...');
                    await connection.start();
                    console.log('Manual reconnection successful');
                    
                    // Try to rejoin session
                    if (currentSessionId && currentParticipantId && currentDisplayName) {
                        const telemetry = collectTelemetry();
                        const result = await connection.invoke("JoinSession", currentSessionId, currentDisplayName, currentParticipantId, telemetry);
                        if (result.success) {
                            console.log('Successfully rejoined session after manual reconnection');
                            hideConnectionOverlay();
                        } else {
                            throw new Error('Failed to rejoin session: ' + (result.error || 'Unknown error'));
                        }
                    } else {
                        hideConnectionOverlay();
                    }
                } catch (reconnectError) {
                    console.error('Manual reconnection failed:', reconnectError);
                    // Keep the overlay showing and it will try again via onclose
                }
            } else {
                console.log('SignalR connection closed gracefully');
                isReconnecting = false;
                hideConnectionOverlay();
            }
        });

        // Setup FFF event handlers
        connection.on("ParticipantJoined", (data) => {
            console.log("Participant joined:", data);
        });

        connection.on("AnswerReceived", (data) => {
            console.log("Answer received:", data);
            if (!data.success) {
                alert(data.error || "Answer submission failed");
            }
        });

        // Setup FFF event handlers
        setupFFFEventHandlers();

        // Setup ATA event handlers
        setupATAEventHandlers();

        // Start connection with retry logic
        let retryCount = 0;
        const maxRetries = 10;
        while (retryCount < maxRetries) {
            try {
                await connection.start();
                console.log("SignalR connected");
                break; // Success!
            } catch (err) {
                retryCount++;
                if (retryCount >= maxRetries) {
                    throw new Error('Could not connect to server after ' + maxRetries + ' attempts. Please check if the server is running.');
                }
                console.warn(`Connection attempt ${retryCount} failed, retrying in ${retryCount * 2}s...`, err);
                showError(`Connecting to server... (attempt ${retryCount}/${maxRetries})`, false);
                await new Promise(resolve => setTimeout(resolve, retryCount * 2000));
            }
        }

        hideError();

        // Join the session with telemetry data
        const result = await connection.invoke("JoinSession", sessionId, displayName, participantId, telemetry);
        console.log("Join result:", result);

        // Check if validation failed
        if (result.success === false) {
            showError(result.error || "Failed to join session");
            await connection.stop();
            connection = null;
            return;
        }

        // Save session info
        currentSessionId = result.sessionId;
        currentParticipantId = result.participantId;
        currentDisplayName = result.displayName;
        saveSessionInfo(currentSessionId, currentParticipantId, currentDisplayName);

        // Start session expiry timer for privacy/security
        startSessionExpiryTimer();

        // Update UI
        document.getElementById('displaySessionId').textContent = currentSessionId;
        document.getElementById('lblDisplayName').textContent = currentDisplayName;
        document.getElementById('displayParticipantId').textContent = currentParticipantId;
        document.getElementById('displayConnectionId').textContent = connection.connectionId;

        showScreen('connectedScreen');

    } catch (err) {
        console.error("Join failed:", err);
        showError("Failed to join session: " + err.toString());
    }
}

/**
 * Setup ATA SignalR event handlers
 */
function setupATAEventHandlers() {
    console.log("[ATA] Setting up ATA event handlers");
    
    // Handle ATA Intro - Show question in view-only mode
    connection.on('ATAIntroStarted', (data) => {
        console.log("[ATA] ✓ ATAIntroStarted event received:", data);
        
        // Reset voting state
        ataHasVoted = false;
        hideATAMessage();
        
        // Set question and options
        document.getElementById('ataQuestionText').textContent = data.questionText;
        document.getElementById('optionAText').textContent = data.optionA;
        document.getElementById('optionBText').textContent = data.optionB;
        document.getElementById('optionCText').textContent = data.optionC;
        document.getElementById('optionDText').textContent = data.optionD;
        
        // Disable vote buttons (view-only during intro)
        document.querySelectorAll('#ataVoteButtons .vote-button').forEach(btn => {
            btn.disabled = true;
            btn.classList.remove('selected');
        });
        
        // Hide results initially
        document.getElementById('ataResults').style.display = 'none';
        
        // Start timer
        updateATATimer(data.timeLimit || 120);
        startATATimerCountdown();
        
        // Show message
        showATAMessage("Please wait for voting to begin...", false);
        
        // Show ATA screen
        showScreen('ataVotingScreen');
    });
    
    connection.on('VotingStarted', (data) => {
        console.log("[ATA] ✓ VotingStarted event received:", data);
        
        // Reset voting state
        ataHasVoted = false;
        hideATAMessage();
        
        // Set question and options
        document.getElementById('ataQuestionText').textContent = data.questionText;
        document.getElementById('optionAText').textContent = data.optionA;
        document.getElementById('optionBText').textContent = data.optionB;
        document.getElementById('optionCText').textContent = data.optionC;
        document.getElementById('optionDText').textContent = data.optionD;
        
        // Enable vote buttons
        document.querySelectorAll('#ataVoteButtons .vote-button').forEach(btn => {
            btn.disabled = false;
            btn.classList.remove('selected');
        });
        
        // Hide results initially
        document.getElementById('ataResults').style.display = 'none';
        
        // Start timer
        updateATATimer(data.timeLimit || 30);
        startATATimerCountdown();
        
        // Show ATA screen
        showScreen('ataVotingScreen');
    });

    connection.on('VotesUpdated', (data) => {
        console.log("[ATA] ✓ VotesUpdated event received:", data);
        updateATAResults(data.results, data.totalVotes);
    });

    connection.on('VotingEnded', (data) => {
        console.log("[ATA] ✓ VotingEnded event received:", data);
        
        stopATATimer();
        
        // Disable vote buttons
        document.querySelectorAll('#ataVoteButtons .vote-button').forEach(btn => {
            btn.disabled = true;
        });
        
        // Show final results (will persist until answer is selected)
        updateATAResults(data.results, data.totalVotes);
        showATAMessage("Voting has ended - waiting for answer...", false);
    });

    // Clear ATA and return to lobby when answer is selected
    connection.on('ATACleared', () => {
        console.log("[ATA]  ATACleared event received - returning to lobby");
        showScreen('connectedScreen');
    });

    // Game State Management - handle state transitions
    connection.on('GameStateChanged', (stateData) => {
        console.log("[GameState] State changed to:", stateData.State, stateData.Message);
        handleGameStateChange(stateData);
    });

    connection.on('VoteReceived', (data) => {
        console.log("Vote Received:", data);
        if (data.success) {
            showATAMessage("✓ Your vote has been recorded!", false);
        } else {
            showATAMessage(data.message || "Vote failed", true);
        }
    });
}

// ============================================================================
// Game State Management
// ============================================================================
function handleGameStateChange(stateData) {
    const { state, message, data } = stateData;
    const State = state;
    const Message = message;
    const Data = data;
    
    // State enum values (must match C# GameStateType)
    const GameState = {
        InitialLobby: 0,
        WaitingLobby: 1,
        FFFLobby: 2,
        FFFQuestion: 3,
        FFFCalculating: 4,
        FFFNoResponse: 5,
        FFFResults: 6,
        FFFWinner: 7,
        ATAReady: 8,
        ATAVoting: 9,
        ATAVoteSubmitted: 10,
        ATAResults: 11,
        GameComplete: 12
    };
    
    switch (State) {
        case GameState.InitialLobby:
            showScreen('connectedScreen');
            break;
            
        case GameState.WaitingLobby:
            showScreen('connectedScreen');
            const infoDiv = document.querySelector('#connectedScreen .info');
            if (infoDiv && Message) {
                const waitingMsg = document.createElement('div');
                waitingMsg.style.marginTop = '20px';
                waitingMsg.style.background = 'rgba(255, 215, 0, 0.1)';
                waitingMsg.style.padding = '10px';
                waitingMsg.style.borderRadius = '4px';
                waitingMsg.textContent = Message;
                const existing = infoDiv.querySelector('.waiting-message');
                if (existing) existing.remove();
                waitingMsg.classList.add('waiting-message');
                infoDiv.appendChild(waitingMsg);
            }
            break;
            
        case GameState.GameComplete:
            showScreen('connectedScreen');
            const completeDiv = document.getElementById('connectedScreen');
            if (completeDiv) {
                completeDiv.innerHTML = '<div class="status connected"> Game Complete</div><div class="info" style="text-align: center; margin-top: 40px;"><h2 style="color: #FFD700;">Thank You for Participating!</h2><p style="margin-top: 20px;">' + (Message || 'Please close your browser to clear this from your device.') + '</p><p style="margin-top: 20px; color: #888;">This session will be automatically disconnected in 10 minutes.</p></div>';
            }
            setTimeout(() => {
                if (connection) connection.stop();
                window.location.reload();
            }, 600000);
            break;
    }
}
// ============================================================================
// ATA Voting Functions
// ============================================================================

/**
 * Submit ATA vote
 */
async function submitATAVote(option) {
    if (!connection || !currentSessionId || !currentParticipantId) {
        showATAMessage("Error: Not connected", true);
        return;
    }

    if (ataHasVoted) {
        showATAMessage("You have already voted", true);
        return;
    }

    try {
        console.log(`Submitting vote: ${option}`);
        await connection.invoke('SubmitVote', currentSessionId, currentParticipantId, option);
        
        ataHasVoted = true;
        
        // Disable all vote buttons
        document.querySelectorAll('#ataVoteButtons .vote-button').forEach(btn => {
            btn.disabled = true;
            if (btn.dataset.option === option) {
                btn.classList.add('selected');
            }
        });

        showATAMessage("✓ Your vote has been recorded!", false);

    } catch (err) {
        console.error("Vote failed:", err);
        showATAMessage("Failed to submit vote: " + err.toString(), true);
    }
}

/**
 * Show ATA message
 */
function showATAMessage(message, isError) {
    const messageDiv = document.getElementById('ataMessage');
    messageDiv.textContent = message;
    messageDiv.style.display = 'block';
    messageDiv.style.background = isError ? 'rgba(255, 0, 0, 0.2)' : 'rgba(0, 255, 0, 0.2)';
    messageDiv.style.borderColor = isError ? '#ff6666' : '#00ff00';
}

/**
 * Hide ATA message
 */
function hideATAMessage() {
    document.getElementById('ataMessage').style.display = 'none';
}

/**
 * Update ATA timer display
 */
function updateATATimer(seconds) {
    ataTimeRemaining = seconds;
    const timerDiv = document.getElementById('ataTimer');
    const timerSeconds = document.getElementById('timerSeconds');
    
    timerSeconds.textContent = seconds;
    
    if (seconds <= 10) {
        timerDiv.classList.add('warning');
    } else {
        timerDiv.classList.remove('warning');
    }
}

/**
 * Start ATA timer countdown
 */
function startATATimerCountdown() {
    if (ataTimerInterval) {
        clearInterval(ataTimerInterval);
    }

    ataTimerInterval = setInterval(() => {
        ataTimeRemaining--;
        updateATATimer(ataTimeRemaining);

        if (ataTimeRemaining <= 0) {
            clearInterval(ataTimerInterval);
            ataTimerInterval = null;
        }
    }, 1000);
}

/**
 * Stop ATA timer
 */
function stopATATimer() {
    if (ataTimerInterval) {
        clearInterval(ataTimerInterval);
        ataTimerInterval = null;
    }
}

/**
 * Update ATA results display
 */
function updateATAResults(results, totalVotes) {
    document.getElementById('ataResults').style.display = 'block';

    ['A', 'B', 'C', 'D'].forEach(option => {
        const percentage = results[option] || 0;
        document.getElementById(`resultsFill${option}`).style.width = percentage + '%';
        document.getElementById(`resultsText${option}`).textContent = `${option}: ${percentage.toFixed(1)}%`;
    });

    document.getElementById('totalVotes').textContent = `Total votes: ${totalVotes || 0}`;
}

// ============================================================================
// FFF (Fastest Finger First) Functions
// ============================================================================

let fffCurrentAnswers = [];
let fffCurrentOrder = [];
let fffCurrentQuestionId = null;
let fffTimerInterval = null;
let fffTimeRemaining = 20;
let fffHasSubmitted = false;
let fffStartTime = null;

/**
 * Setup FFF event handlers on SignalR connection
 */
function setupFFFEventHandlers() {
    console.log("📡 Setting up FFF event handlers...");
    
    connection.on('QuestionStarted', (data) => {
        console.log("FFF Question Started:", data);
        startFFFQuestion(data);
    });

    connection.on('QuestionEnded', (data) => {
        console.log("FFF Question Ended:", data);
        endFFFQuestion(data);
    });

    connection.on('RankingsUpdated', (data) => {
        console.log("FFF Rankings Updated:", data);
        showFFFRankings(data);
    });
    
    // FFF Phase Messages
    connection.on('TimerExpired', (data) => {
        console.log("⏰ FFF Timer Expired:", data);
        handleTimerExpired();
    });
    
    connection.on('RevealingWinner', (data) => {
        console.log("🏆 FFF Revealing Winner:", data);
        handleRevealingWinner();
    });
    
    connection.on('WinnerConfirmed', (data) => {
        console.log("✅ FFF Winner Confirmed:", data);
        handleWinnerConfirmed(data);
    });
    
    connection.on('NoWinner', (data) => {
        console.log("❌ FFF No Winner:", data);
        handleNoWinner();
    });
    
    connection.on('ResetToLobby', (data) => {
        console.log("🔄 FFF Reset To Lobby:", data);
        handleResetToLobby();
    });
    
    connection.on('ServerShuttingDown', () => {
        console.log("🛑 Server is shutting down - clearing session data");
        handleServerShutdown();
    });
    
    console.log("✅ FFF event handlers registered successfully");
}

/**
 * Start FFF question
 */
function startFFFQuestion(data) {
    console.log("Starting FFF question with data:", data);
    
    // Reset state
    fffHasSubmitted = false;
    fffCurrentQuestionId = data.QuestionId || data.questionId;
    
    // Extract answers from Options array
    const options = data.Options || data.options || [];
    fffCurrentAnswers = [
        { letter: 'A', text: options[0] || 'Answer A' },
        { letter: 'B', text: options[1] || 'Answer B' },
        { letter: 'C', text: options[2] || 'Answer C' },
        { letter: 'D', text: options[3] || 'Answer D' }
    ];
    fffCurrentOrder = ['A', 'B', 'C', 'D']; // Default order
    fffTimeRemaining = Math.floor((data.TimeLimit || data.timeLimit || 20000) / 1000); // Convert ms to seconds
    fffStartTime = Date.now();
    
    // Update UI with question text
    const questionText = data.Question || data.question || data.QuestionText || data.questionText || 'Question';
    document.getElementById('fffQuestionText').textContent = questionText;
    document.getElementById('btnSubmitFFF').disabled = false;
    document.getElementById('fffMessage').style.display = 'none';
    
    // Render answer list
    renderFFFAnswers();
    
    // Start timer
    updateFFFTimer(fffTimeRemaining);
    startFFFTimerCountdown();
    
    // Show FFF screen
    showScreen('fffQuestionScreen');
}

/**
 * Render FFF answer list
 */
function renderFFFAnswers() {
    const answerList = document.getElementById('fffAnswerList');
    answerList.innerHTML = '';
    
    fffCurrentOrder.forEach((letter, index) => {
        const answer = fffCurrentAnswers.find(a => a.letter === letter);
        const answerItem = document.createElement('div');
        answerItem.className = 'fff-answer-item';
        answerItem.dataset.letter = letter;
        answerItem.innerHTML = `
            <div class="fff-position">${index + 1}</div>
            <div class="fff-letter">${letter}</div>
            <div class="fff-text">${answer.text}</div>
        `;
        
        // Add click handler to select and reorder
        answerItem.addEventListener('click', () => selectFFFAnswer(letter));
        
        answerList.appendChild(answerItem);
    });
}

let fffSelectedLetter = null;

/**
 * Select FFF answer for reordering
 */
function selectFFFAnswer(letter) {
    if (fffHasSubmitted) return;
    
    const answerItems = document.querySelectorAll('.fff-answer-item');
    
    if (fffSelectedLetter === null) {
        // First selection - mark as selected
        fffSelectedLetter = letter;
        answerItems.forEach(item => {
            if (item.dataset.letter === letter) {
                item.classList.add('selected');
            }
        });
    } else if (fffSelectedLetter === letter) {
        // Deselect
        fffSelectedLetter = null;
        answerItems.forEach(item => item.classList.remove('selected'));
    } else {
        // Second selection - swap positions
        const index1 = fffCurrentOrder.indexOf(fffSelectedLetter);
        const index2 = fffCurrentOrder.indexOf(letter);
        
        // Swap in array
        [fffCurrentOrder[index1], fffCurrentOrder[index2]] = [fffCurrentOrder[index2], fffCurrentOrder[index1]];
        
        // Deselect and re-render
        fffSelectedLetter = null;
        renderFFFAnswers();
    }
}

/**
 * Submit FFF answer
 */
async function submitFFFAnswer() {
    if (fffHasSubmitted) return;
    
    try {
        fffHasSubmitted = true;
        document.getElementById('btnSubmitFFF').disabled = true;
        // Don't stop timer - let it continue counting down to 0
        
        const timeElapsed = Date.now() - fffStartTime;
        const answerSequence = fffCurrentOrder.join(''); // e.g., "BDCA"
        
        console.log("Submitting FFF answer:", answerSequence, "Time:", timeElapsed, "ms", "QuestionId:", fffCurrentQuestionId);
        
        // Submit to server
        await connection.invoke("SubmitAnswer", currentSessionId, currentParticipantId, fffCurrentQuestionId, answerSequence);
        
        showFFFMessage(`&#x2713; Answer submitted in ${(timeElapsed / 1000).toFixed(1)}s!`, false);
        
    } catch (err) {
        console.error("Submit FFF answer failed:", err);
        showFFFMessage("&#x274C; Failed to submit answer: " + err.toString(), true);
        fffHasSubmitted = false;
        document.getElementById('btnSubmitFFF').disabled = false;
    }
}

/**
 * End FFF question
 */
function endFFFQuestion(data) {
    stopFFFTimer();
    document.getElementById('btnSubmitFFF').disabled = true;
    
    if (!fffHasSubmitted) {
        showFFFMessage("ΓÅ▒∩╕Å Time's up! Question ended.", true);
    }
}

/**
 * Show FFF rankings
 */
function showFFFRankings(data) {
    if (!data.rankings || data.rankings.length === 0) {
        showFFFMessage("&#x2713; No correct answers submitted.", false);
        return;
    }
    
    let message = "&#x1F3C6; <strong>Results:</strong><br><br>";
    data.rankings.forEach((rank, index) => {
        const medal = index === 0 ? "&#x1F947;" : index === 1 ? "&#x1F948;" : index === 2 ? "&#x1F949;" : `${index + 1}.`;
        message += `${medal} ${rank.displayName} - ${rank.timeElapsed.toFixed(1)}s<br>`;
    });
    
    showFFFMessage(message, false);
}

/**
 * Show FFF message
 */
function showFFFMessage(message, isError) {
    const messageDiv = document.getElementById('fffMessage');
    messageDiv.innerHTML = message;
    messageDiv.style.display = 'block';
    messageDiv.style.background = isError ? 'rgba(255, 0, 0, 0.2)' : 'rgba(0, 255, 0, 0.2)';
    messageDiv.style.borderColor = isError ? '#ff6666' : '#00ff00';
}

/**
 * Update FFF timer display
 */
function updateFFFTimer(seconds) {
    fffTimeRemaining = seconds;
    const timerDiv = document.getElementById('fffTimer');
    const timerSeconds = document.getElementById('fffTimerSeconds');
    
    timerSeconds.textContent = seconds;
    
    if (seconds <= 10) {
        timerDiv.classList.add('warning');
    } else {
        timerDiv.classList.remove('warning');
    }
}

/**
 * Start FFF timer countdown
 */
function startFFFTimerCountdown() {
    if (fffTimerInterval) {
        clearInterval(fffTimerInterval);
    }

    fffTimerInterval = setInterval(() => {
        fffTimeRemaining--;
        updateFFFTimer(fffTimeRemaining);

        if (fffTimeRemaining <= 0) {
            clearInterval(fffTimerInterval);
            fffTimerInterval = null;
            
            if (!fffHasSubmitted) {
                submitFFFAnswer(); // Auto-submit on timeout
            }
        }
    }, 1000);
}

/**
 * Stop FFF timer
 */
function stopFFFTimer() {
    if (fffTimerInterval) {
        clearInterval(fffTimerInterval);
        fffTimerInterval = null;
    }
}

/**
 * Handle FFF timer expired
 */
function handleTimerExpired() {
    document.getElementById('fffWaitingMessage').innerHTML = 
        'Results are being calculated.<br>Please Stand By.<br>The Host will announce the winners shortly.';
    showScreen('fffWaitingScreen');
}

/**
 * Handle revealing winner announcement
 */
function handleRevealingWinner() {
    document.getElementById('fffWaitingMessage').innerHTML = 'And the winner is...';
    showScreen('fffWaitingScreen');
}

/**
 * Handle winner confirmed with personalized outcome
 */
function handleWinnerConfirmed(data) {
    const myId = currentParticipantId;
    let message;
    
    if (data.winnerId === myId) {
        message = 'Congratulations! You are the next contestant!<br>Come up to the stage!';
    } else if (data.correctParticipants && data.correctParticipants.includes(myId)) {
        message = "Sorry! You weren't fast enough, but you can try again later!";
    } else {
        message = "Sorry! Your answer was incorrect, but you can try again later!";
    }
    
    document.getElementById('fffResultMessage').innerHTML = message;
    showScreen('fffResultScreen');
}

/**
 * Handle no winner scenario
 */
function handleNoWinner() {
    document.getElementById('fffResultMessage').innerHTML = 
        'No winners! You get to try again!<br>Get ready to answer the next one!';
    showScreen('fffResultScreen');
}

/**
 * Handle reset to lobby
 */
function handleResetToLobby() {
    showScreen('connectedScreen');
}

/**
 * Handle server shutdown - clear session and show message
 */
function handleServerShutdown() {
    // Clear all session data
    clearSessionData();
    
    // Show disconnection message
    showScreen('welcomeScreen');
    showError('The game server is shutting down. Your session has been cleared. Please wait for the server to restart and rejoin.', true);
    
    // Close the SignalR connection gracefully
    if (connection) {
        connection.stop().catch(err => console.error('Error closing connection:', err));
    }
}

// ============================================================================
// UI Helper Functions
// ============================================================================

/**
 * Show error message
 */
function showError(message, isError = true) {
    const errorDiv = document.getElementById('errorMessage');
    const nameInput = document.getElementById('txtDisplayName');
    if (!errorDiv) return;
    errorDiv.textContent = message;
    errorDiv.style.display = 'block';
    // Only highlight input field for actual errors, not status messages
    if (nameInput && isError) {
        nameInput.classList.add('error');
    }
}

/**
 * Hide error message
 */
function hideError() {
    const errorDiv = document.getElementById('errorMessage');
    const nameInput = document.getElementById('txtDisplayName');
    if (!nameInput || !errorDiv) return;
    errorDiv.style.display = 'none';
    nameInput.classList.remove('error');
}

/**
 * Clear all data (for server restarts)
 */
function clearAllData() {
    clearSessionData();
    
    // Clear any cached game state
    currentSessionId = null;
    currentParticipantId = null;
    currentDisplayName = null;
    
    // Disconnect if still connected
    if (connection) {
        connection.stop().catch(err => console.error('Error stopping connection:', err));
        connection = null;
    }
}

/**
 * Show connection overlay with status message
 */
function showConnectionOverlay(title, message) {
    const overlay = document.getElementById('connectionOverlay');
    const titleElem = document.getElementById('connectionOverlayTitle');
    const messageElem = document.getElementById('connectionOverlayMessage');
    
    if (overlay && titleElem && messageElem) {
        titleElem.textContent = title;
        messageElem.textContent = message;
        overlay.style.display = 'flex';
    }
}

/**
 * Hide connection overlay
 */
function hideConnectionOverlay() {
    const overlay = document.getElementById('connectionOverlay');
    if (overlay) {
        overlay.style.display = 'none';
    }
}

/**
 * Show specific screen
 */
function showScreen(screenId) {
    document.querySelectorAll('.screen').forEach(s => s.classList.remove('active'));
    const screen = document.getElementById(screenId);
    if (screen) {
        screen.classList.add('active');
    } else {
        console.error(`Screen not found: ${screenId}`);
    }
}

// ============================================================================
// Event Listeners & Initialization
// ============================================================================
// Initialization
// ============================================================================

/**
 * Setup all button and form event listeners
 */
function setupEventListeners() {
    // Join button
    document.getElementById('btnJoin').addEventListener('click', async () => {
        const displayName = document.getElementById('txtDisplayName').value.trim();

        if (!displayName) {
            showError("Please enter your name");
            return;
        }

        const sessionCode = getSessionCode(); // Always use "LIVE" session
        // Get existing participant ID to support reconnection
        const existingParticipantId = getStoredParticipantId();
        await joinSession(sessionCode, displayName, existingParticipantId);
    });

    // Allow Enter key to join session
    document.getElementById('txtDisplayName').addEventListener('keypress', (e) => {
        if (e.key === 'Enter') {
            document.getElementById('btnJoin').click();
        }
    });

    // Test reconnect button
    document.getElementById('btnTestReconnect').addEventListener('click', async () => {
        if (connection) {
            console.log("Testing reconnect - stopping connection...");
            await connection.stop();
            
            setTimeout(async () => {
                console.log("Reconnecting with stored participant ID:", currentParticipantId);
                await joinSession(currentSessionId, currentDisplayName, currentParticipantId);
            }, 1000);
        }
    });

    // Leave button
    document.getElementById('btnLeave').addEventListener('click', async () => {
        if (connection) {
            await connection.stop();
        }
        clearSessionData();
        showScreen('joinScreen');
    });

    // ATA vote button handlers
    document.querySelectorAll('#ataVoteButtons .vote-button').forEach(button => {
        button.addEventListener('click', async function() {
            const option = this.dataset.option;
            await submitATAVote(option);
        });
    });
    
    // FFF submit button handler
    document.getElementById('btnSubmitFFF').addEventListener('click', () => {
        submitFFFAnswer();
    });
}

// ============================================================================
// Privacy & Session Management
// ============================================================================

/**
 * Clear all session data and cached information for privacy
 */
function clearSessionData() {
    console.log("Clearing all session data for privacy...");
    
    // Clear state variables
    currentSessionId = null;
    currentParticipantId = null;
    currentDisplayName = null;
    sessionWarningShown = false;
    
    // Clear all localStorage
    Object.values(STORAGE_KEYS).forEach(key => {
        localStorage.removeItem(key);
    });
    
    // Clear session storage as well
    sessionStorage.clear();
    
    // Stop session expiry timer
    if (sessionExpiryTimer) {
        clearInterval(sessionExpiryTimer);
        sessionExpiryTimer = null;
    }
    
    console.log("&#x2713; All session data cleared");
}

/**
 * Start session expiry timer
 */
function startSessionExpiryTimer() {
    // Clear any existing timer
    if (sessionExpiryTimer) {
        clearInterval(sessionExpiryTimer);
    }
    
    // Store session start timestamp
    const now = Date.now();
    localStorage.setItem(STORAGE_KEYS.SESSION_TIMESTAMP, now.toString());
    sessionWarningShown = false;
    
    // Check session expiry every minute
    sessionExpiryTimer = setInterval(() => {
        const sessionStart = parseInt(localStorage.getItem(STORAGE_KEYS.SESSION_TIMESTAMP) || '0');
        const elapsed = Date.now() - sessionStart;
        const remaining = SESSION_CONFIG.maxSessionDuration - elapsed;
        
        // Show warning 15 minutes before expiry
        if (remaining <= SESSION_CONFIG.warningBeforeExpiry && !sessionWarningShown) {
            sessionWarningShown = true;
            const minutesLeft = Math.ceil(remaining / 60000);
            showError(`Session will expire in ${minutesLeft} minutes. The show will end soon.`, 'warning');
        }
        
        // Session expired - auto cleanup
        if (remaining <= 0) {
            console.log("Session expired - auto cleanup triggered");
            clearInterval(sessionExpiryTimer);
            sessionExpiryTimer = null;
            
            if (connection) {
                connection.stop().catch(err => console.error("Error stopping connection:", err));
            }
            
            showError('Your session has expired. Thank you for participating!', 'info');
            
            // Clear all data after 3 seconds
            setTimeout(() => {
                clearSessionData();
                showScreen('joinScreen');
            }, 3000);
        }
    }, SESSION_CONFIG.checkInterval);
    
    console.log(`Session expiry timer started (${SESSION_CONFIG.maxSessionDuration / 3600000} hours)`);
}

/**
 * Clear cache and session data when window is closed/reloaded
 */
function setupCleanupHandlers() {
    // Don't clear data on beforeunload - we want to persist across page refreshes
    // Session cleanup happens when user explicitly leaves or clicks leave game
    
    // Handle visibility change (user switches tabs/minimizes)
    document.addEventListener('visibilitychange', () => {
        if (document.hidden) {
            console.log("Page hidden - maintaining session but ready for cleanup");
        }
    });
}

// ============================================================================
// Initialization
// ============================================================================

/**
 * Initialize application on page load
 */
window.addEventListener('DOMContentLoaded', () => {
    // Setup cleanup handlers for privacy/security
    setupCleanupHandlers();
    
    // Pre-fill session code (hidden field) with auto-generated ID
    const sessionCode = getSessionCode();
    const sessionCodeInput = document.getElementById('sessionCode');
    if (sessionCodeInput) {
        sessionCodeInput.value = sessionCode;
    }

    // Pre-fill display name if stored
    const storedName = getStoredDisplayName();
    if (storedName) {
        const displayNameInput = document.getElementById('displayName');
        if (displayNameInput) {
            displayNameInput.value = storedName;
        }
    }

    // Auto-reconnect if we have stored session info
    const storedParticipantId = getStoredParticipantId();
    if (sessionCode && storedName && storedParticipantId) {
        console.log("Auto-reconnecting with stored participant ID:", storedParticipantId);
        
        // Initialize global variables from localStorage before joining
        currentSessionId = sessionCode;
        currentDisplayName = storedName;
        currentParticipantId = storedParticipantId;
        
        joinSession(sessionCode, storedName, storedParticipantId);
    }

    // Setup button event listeners
    setupEventListeners();
});
