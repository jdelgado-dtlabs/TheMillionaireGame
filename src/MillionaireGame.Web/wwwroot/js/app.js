/**
 * Who Wants to be a Millionaire
 * Audience Participation System
 * Version: 0.6.2-2512 (Privacy & Session Management)
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
// State Management
// ============================================================================

let connection = null;
let currentSessionId = null;
let currentParticipantId = null;
let currentDisplayName = null;
let sessionExpiryTimer = null;
let sessionWarningShown = false;

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
 * Get session code from URL or auto-generate
 */
function getSessionCode() {
    const urlParams = new URLSearchParams(window.location.search);
    const urlSessionCode = urlParams.get('session');
    if (urlSessionCode) {
        return urlSessionCode;
    }
    return getAutoSessionId();
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

        // Create SignalR connection
        connection = new signalR.HubConnectionBuilder()
            .withUrl(`/hubs/fff`)
            .withAutomaticReconnect()
            .build();

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

        // Setup ATA event handlers
        setupATAEventHandlers();

        // Start connection
        await connection.start();
        console.log("SignalR connected");

        // Join the session
        const result = await connection.invoke("JoinSession", sessionId, displayName, participantId);
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
        document.getElementById('displayName').textContent = currentDisplayName;
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
    connection.on('VotingStarted', (data) => {
        console.log("ATA Voting Started:", data);
        
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
        console.log("Votes Updated:", data);
        updateATAResults(data.results, data.totalVotes);
    });

    connection.on('VotingEnded', (data) => {
        console.log("Voting Ended:", data);
        
        stopATATimer();
        
        // Disable vote buttons
        document.querySelectorAll('#ataVoteButtons .vote-button').forEach(btn => {
            btn.disabled = true;
        });
        
        // Show final results
        updateATAResults(data.results, data.totalVotes);
        showATAMessage("Voting has ended", false);
        
        // Return to connected screen after 5 seconds
        setTimeout(() => {
            showScreen('connectedScreen');
        }, 5000);
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
// UI Helper Functions
// ============================================================================

/**
 * Show error message
 */
function showError(message) {
    const errorDiv = document.getElementById('errorMessage');
    const nameInput = document.getElementById('displayName');
    errorDiv.textContent = message;
    errorDiv.style.display = 'block';
    nameInput.classList.add('error');
}

/**
 * Hide error message
 */
function hideError() {
    const errorDiv = document.getElementById('errorMessage');
    const nameInput = document.getElementById('displayName');
    errorDiv.style.display = 'none';
    nameInput.classList.remove('error');
}

/**
 * Show specific screen
 */
function showScreen(screenId) {
    document.querySelectorAll('.screen').forEach(s => s.classList.remove('active'));
    document.getElementById(screenId).classList.add('active');
}

// ============================================================================
// Event Listeners & Initialization
// ============================================================================

/**
 * Initialize application on page load
 */
window.addEventListener('DOMContentLoaded', () => {
    // Pre-fill session code (hidden field) with auto-generated ID
    const sessionCode = getSessionCode();
    document.getElementById('sessionCode').value = sessionCode;

    // Pre-fill display name if stored
    const storedName = getStoredDisplayName();
    if (storedName) {
        document.getElementById('displayName').value = storedName;
    }

    // Auto-reconnect if we have stored session info
    const storedParticipantId = getStoredParticipantId();
    if (sessionCode && storedName && storedParticipantId) {
        console.log("Auto-reconnecting with stored participant ID:", storedParticipantId);
        joinSession(sessionCode, storedName, storedParticipantId);
    }

    // Setup button event listeners
    setupEventListeners();
});

/**
 * Setup all button and form event listeners
 */
function setupEventListeners() {
    // Join button
    document.getElementById('btnJoin').addEventListener('click', async () => {
        const displayName = document.getElementById('displayName').value.trim();
        const sessionCode = document.getElementById('sessionCode').value.trim();

        if (!displayName) {
            alert("Please enter your name");
            return;
        }

        const existingParticipantId = getStoredParticipantId();
        await joinSession(sessionCode, displayName, existingParticipantId);
    });

    // Allow Enter key to join session
    document.getElementById('displayName').addEventListener('keypress', (e) => {
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
    
    console.log("✓ All session data cleared");
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
    // Clear data on page unload
    window.addEventListener('beforeunload', () => {
        // If we're leaving the page (not just refreshing during active session),
        // clear everything for privacy
        if (!connection || connection.state !== signalR.HubConnectionState.Connected) {
            clearSessionData();
        }
    });
    
    // Handle visibility change (user switches tabs/minimizes)
    document.addEventListener('visibilitychange', () => {
        if (document.hidden) {
            console.log("Page hidden - maintaining session but ready for cleanup");
        }
    });
    
    // Handle page cache (bfcache) - force fresh load
    window.addEventListener('pageshow', (event) => {
        if (event.persisted) {
            console.log("Page restored from cache - forcing reload for fresh session");
            window.location.reload();
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
    document.getElementById('sessionCode').value = sessionCode;

    // Pre-fill display name if stored
    const storedName = getStoredDisplayName();
    if (storedName) {
        document.getElementById('displayName').value = storedName;
    }

    // Auto-reconnect if we have stored session info
    const storedParticipantId = getStoredParticipantId();