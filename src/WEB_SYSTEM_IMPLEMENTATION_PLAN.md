# Modern Web-Based Audience Participation System (WAPS) - Implementation Plan

## Overview
This document outlines the phased implementation of **WAPS** - a unified web-based audience participation system that replaces the old TCP/IP FFF client and adds new capabilities for real ATA voting, mobile access, and QR code joining.

**Goal**: Create a modern, maintainable, cross-platform audience participation system using ASP.NET Core, SignalR, and Progressive Web App (PWA) technologies.

**Replaces**:
- Old standalone FFF TCP/IP client
- Placeholder ATA voting system
- Planned mobile interface features

---

## Technology Stack

### Backend
- **ASP.NET Core 8.0** - Web API framework
- **SignalR** - Real-time bidirectional communication
- **Entity Framework Core** - Database access (extends existing SQLite database)
- **QRCoder** or **QRCode.js** - QR code generation

### Frontend
- **Blazor WebAssembly** or **React/Vue.js** - Progressive Web App
- **SignalR Client** - Real-time connection to backend
- **Bootstrap 5** - Responsive UI framework
- **Service Workers** - Offline capability

### Database
- **SQLite** (existing) - Extended with new tables for sessions, participants, votes

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    Main WinForms App                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │ Control Panel│  │  Game Screen │  │  QR Display  │     │
│  │   (Host UI)  │  │   (TV View)  │  │   (Join)     │     │
│  └──────┬───────┘  └──────────────┘  └──────────────┘     │
│         │ SignalR Client                                    │
└─────────┼───────────────────────────────────────────────────┘
          │
          │ HTTPS/WSS
          │
┌─────────▼───────────────────────────────────────────────────┐
│              ASP.NET Core Web API + SignalR                 │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │   FFF Hub    │  │   ATA Hub    │  │ Session Mgmt │     │
│  └──────────────┘  └──────────────┘  └──────────────┘     │
│  ┌──────────────────────────────────────────────────────┐  │
│  │        Repository Layer (EF Core + SQLite)           │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────┬───────────────────────────────────────────────────┘
          │
          │ HTTPS/WSS
          │
┌─────────▼───────────────────────────────────────────────────┐
│                Progressive Web App (PWA)                     │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │  Join Page   │  │  FFF Screen  │  │  ATA Screen  │     │
│  │ (QR Scan)    │  │  (Answer)    │  │  (Vote)      │     │
│  └──────────────┘  └──────────────┘  └──────────────┘     │
│                   SignalR Client                            │
└─────────────────────────────────────────────────────────────┘
     Accessed via mobile browser (iOS/Android/Desktop)
```

---

## Database Schema Extensions

### New Tables

```sql
-- Game sessions
CREATE TABLE Sessions (
    Id TEXT PRIMARY KEY,
    HostName TEXT NOT NULL,
    CreatedAt DATETIME NOT NULL,
    StartedAt DATETIME,
    EndedAt DATETIME,
    Status TEXT NOT NULL, -- 'Waiting', 'Active', 'Completed'
    CurrentMode TEXT, -- 'FFF', 'ATA', 'Idle'
    CurrentQuestionId INTEGER,
    FOREIGN KEY (CurrentQuestionId) REFERENCES FFFQuestions(Id)
);

-- Participant connections
CREATE TABLE Participants (
    Id TEXT PRIMARY KEY,
    SessionId TEXT NOT NULL,
    DisplayName TEXT NOT NULL,
    JoinedAt DATETIME NOT NULL,
    ConnectionId TEXT,
    LastSeenAt DATETIME,
    IsActive INTEGER NOT NULL DEFAULT 1,
    FOREIGN KEY (SessionId) REFERENCES Sessions(Id)
);

-- FFF answers submitted
CREATE TABLE FFFAnswers (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SessionId TEXT NOT NULL,
    ParticipantId TEXT NOT NULL,
    QuestionId INTEGER NOT NULL,
    AnswerSequence TEXT NOT NULL, -- e.g., "C,A,D,B"
    SubmittedAt DATETIME NOT NULL,
    TimeElapsed REAL NOT NULL, -- Milliseconds
    IsCorrect INTEGER NOT NULL,
    Rank INTEGER, -- NULL until evaluated
    FOREIGN KEY (SessionId) REFERENCES Sessions(Id),
    FOREIGN KEY (ParticipantId) REFERENCES Participants(Id),
    FOREIGN KEY (QuestionId) REFERENCES FFFQuestions(Id)
);

-- ATA votes
CREATE TABLE ATAVotes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SessionId TEXT NOT NULL,
    ParticipantId TEXT NOT NULL,
    QuestionText TEXT NOT NULL,
    SelectedOption TEXT NOT NULL, -- 'A', 'B', 'C', 'D'
    SubmittedAt DATETIME NOT NULL,
    FOREIGN KEY (SessionId) REFERENCES Sessions(Id),
    FOREIGN KEY (ParticipantId) REFERENCES Participants(Id)
);

-- Session events log (optional, for debugging/analytics)
CREATE TABLE SessionEvents (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SessionId TEXT NOT NULL,
    EventType TEXT NOT NULL,
    EventData TEXT, -- JSON
    Timestamp DATETIME NOT NULL,
    FOREIGN KEY (SessionId) REFERENCES Sessions(Id)
);
```

---

## Phase 1: Foundation & Infrastructure

**Estimated Time**: 4-6 hours

### Objectives
- Set up ASP.NET Core Web API project
- Configure SignalR hubs
- Implement session management
- Add QR code generation
- Create basic database models and repositories

### Tasks

#### 1.1 Create Web Project
```bash
cd src/
dotnet new webapi -n MillionaireGame.Web
dotnet sln add MillionaireGame.Web/MillionaireGame.Web.csproj
```

#### 1.2 Add Dependencies
```xml
<!-- MillionaireGame.Web.csproj -->
<ItemGroup>
  <PackageReference Include="Microsoft.AspNetCore.SignalR" Version="8.0.*" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.*" />
  <PackageReference Include="QRCoder" Version="1.5.1" />
  <ProjectReference Include="..\MillionaireGame.Core\MillionaireGame.Core.csproj" />
</ItemGroup>
```

#### 1.3 Create SignalR Hubs
```csharp
// Hubs/FFFHub.cs
public class FFFHub : Hub
{
    public async Task JoinSession(string sessionId, string displayName) { }
    public async Task SubmitAnswer(string sessionId, string answerSequence) { }
    public async Task StartQuestion(string sessionId, int questionId) { }
}

// Hubs/ATAHub.cs
public class ATAHub : Hub
{
    public async Task SubmitVote(string sessionId, string option) { }
    public async Task StartVoting(string sessionId, string questionText) { }
}
```

#### 1.4 Create Database Models
```csharp
// Models/Session.cs
public class Session
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string HostName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Waiting;
    public SessionMode? CurrentMode { get; set; }
    public int? CurrentQuestionId { get; set; }
    
    public virtual ICollection<Participant> Participants { get; set; }
    public virtual ICollection<FFFAnswer> FFFAnswers { get; set; }
    public virtual ICollection<ATAVote> ATAVotes { get; set; }
}

// Models/Participant.cs, FFFAnswer.cs, ATAVote.cs
// (Follow schema above)
```

#### 1.5 Extend DbContext
```csharp
// Database/GameDbContext.cs (extend existing)
public class GameDbContext : DbContext
{
    // Existing DbSets...
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Participant> Participants { get; set; }
    public DbSet<FFFAnswer> FFFAnswers { get; set; }
    public DbSet<ATAVote> ATAVotes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Configure relationships, indexes
    }
}
```

#### 1.6 Create Session Service
```csharp
// Services/SessionService.cs
public class SessionService
{
    public Task<Session> CreateSessionAsync(string hostName);
    public Task<string> GenerateQRCodeAsync(string sessionId);
    public Task<Session> GetSessionAsync(string sessionId);
    public Task<Participant> AddParticipantAsync(string sessionId, string displayName);
    public Task<IEnumerable<Participant>> GetActiveParticipantsAsync(string sessionId);
}
```

#### 1.7 Configure Startup
```csharp
// Program.cs
builder.Services.AddSignalR();
builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<FFFQuestionRepository>();
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", builder => 
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();
app.UseCors("AllowAll");
app.MapHub<FFFHub>("/hubs/fff");
app.MapHub<ATAHub>("/hubs/ata");
app.Run();
```

### Deliverables
- ✅ MillionaireGame.Web project created
- ✅ SignalR hubs scaffolded (FFFHub, ATAHub)
- ✅ Database models and migration created
- ✅ SessionService with QR code generation
- ✅ Basic API endpoints for session CRUD

---

## Phase 2: FFF System Implementation

**Estimated Time**: 6-8 hours

### Objectives
- Implement complete FFF question flow
- Real-time answer submission and validation
- Timing logic with countdown
- Leaderboard calculation
- Winner selection

### Tasks

#### 2.1 FFF Question API
```csharp
// Controllers/FFFController.cs
[ApiController]
[Route("api/fff")]
public class FFFController : ControllerBase
{
    [HttpGet("sessions/{sessionId}/question")]
    public async Task<IActionResult> GetCurrentQuestion(string sessionId);
    
    [HttpPost("sessions/{sessionId}/start")]
    public async Task<IActionResult> StartQuestion(string sessionId, int questionId);
    
    [HttpGet("sessions/{sessionId}/leaderboard")]
    public async Task<IActionResult> GetLeaderboard(string sessionId);
}
```

#### 2.2 FFF Hub Methods
```csharp
public class FFFHub : Hub
{
    private readonly SessionService _sessionService;
    private readonly FFFQuestionRepository _fffRepo;
    
    // Host starts question
    public async Task StartQuestion(string sessionId, int questionId)
    {
        var question = await _fffRepo.GetQuestionByIdAsync(questionId);
        await Clients.Group(sessionId).SendAsync("QuestionStarted", new
        {
            QuestionId = questionId,
            Question = question.Question,
            Options = new[] { question.A, question.B, question.C, question.D },
            TimeLimit = 20000 // 20 seconds
        });
        
        // Start server-side timer
        _ = Task.Delay(20000).ContinueWith(_ => EndQuestion(sessionId, questionId));
    }
    
    // Participant submits answer
    public async Task SubmitAnswer(string sessionId, string answerSequence)
    {
        var participantId = Context.ConnectionId;
        var submittedAt = DateTime.UtcNow;
        
        // Validate and save answer
        var answer = new FFFAnswer
        {
            SessionId = sessionId,
            ParticipantId = participantId,
            AnswerSequence = answerSequence,
            SubmittedAt = submittedAt
        };
        
        await _sessionService.SaveFFFAnswerAsync(answer);
        
        // Acknowledge submission
        await Clients.Caller.SendAsync("AnswerReceived", new
        {
            Timestamp = submittedAt,
            Sequence = answerSequence
        });
    }
    
    // Host ends question manually or timer expires
    public async Task EndQuestion(string sessionId, int questionId)
    {
        // Calculate rankings
        var results = await _sessionService.CalculateFFFRankingsAsync(sessionId, questionId);
        
        // Broadcast results to all
        await Clients.Group(sessionId).SendAsync("QuestionEnded", new
        {
            Winner = results.Winner,
            Leaderboard = results.Rankings
        });
    }
    
    // Connection management
    public override async Task OnConnectedAsync()
    {
        var sessionId = Context.GetHttpContext().Request.Query["sessionId"];
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        await base.OnConnectedAsync();
    }
}
```

#### 2.3 FFF Answer Validation & Ranking
```csharp
// Services/FFFService.cs
public class FFFService
{
    public async Task<FFFResults> CalculateRankingsAsync(string sessionId, int questionId)
    {
        var correctAnswer = await _fffRepo.GetCorrectAnswerAsync(questionId);
        var submissions = await _context.FFFAnswers
            .Where(a => a.SessionId == sessionId && a.QuestionId == questionId)
            .OrderBy(a => a.SubmittedAt)
            .ToListAsync();
        
        // Validate answers
        foreach (var submission in submissions)
        {
            submission.IsCorrect = submission.AnswerSequence == correctAnswer;
        }
        
        // Rank correct answers by time
        var correctSubmissions = submissions
            .Where(s => s.IsCorrect)
            .OrderBy(s => s.TimeElapsed)
            .ToList();
        
        for (int i = 0; i < correctSubmissions.Count; i++)
        {
            correctSubmissions[i].Rank = i + 1;
        }
        
        await _context.SaveChangesAsync();
        
        return new FFFResults
        {
            Winner = correctSubmissions.FirstOrDefault(),
            Rankings = correctSubmissions.Take(10).ToList()
        };
    }
}
```

#### 2.4 Host Control Panel Integration
```csharp
// In MillionaireGame project (WinForms)
// Add SignalR client reference
public class FFFHostClient
{
    private HubConnection _hubConnection;
    
    public async Task ConnectAsync(string serverUrl, string sessionId)
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{serverUrl}/hubs/fff?sessionId={sessionId}")
            .Build();
        
        _hubConnection.On<object>("QuestionStarted", OnQuestionStarted);
        _hubConnection.On<object>("QuestionEnded", OnQuestionEnded);
        
        await _hubConnection.StartAsync();
    }
    
    public async Task StartQuestionAsync(int questionId)
    {
        await _hubConnection.InvokeAsync("StartQuestion", _sessionId, questionId);
    }
}
```

### Deliverables
- ✅ FFF question API endpoints
- ✅ FFF hub with real-time submission handling
- ✅ Answer validation and ranking logic
- ✅ Timer-based question lifecycle
- ✅ Leaderboard generation
- ✅ Host control panel SignalR client integration

---

## Phase 3: Real ATA Voting System

**Estimated Time**: 4-5 hours

### Objectives
- Implement live voting for Ask The Audience
- Real-time vote aggregation
- Percentage calculation and display
- Results broadcast to host and participants

### Tasks

#### 3.1 ATA API
```csharp
// Controllers/ATAController.cs
[ApiController]
[Route("api/ata")]
public class ATAController : ControllerBase
{
    [HttpPost("sessions/{sessionId}/start")]
    public async Task<IActionResult> StartVoting(string sessionId, [FromBody] ATAQuestion question);
    
    [HttpGet("sessions/{sessionId}/results")]
    public async Task<IActionResult> GetResults(string sessionId);
    
    [HttpPost("sessions/{sessionId}/end")]
    public async Task<IActionResult> EndVoting(string sessionId);
}
```

#### 3.2 ATA Hub Methods
```csharp
public class ATAHub : Hub
{
    public async Task StartVoting(string sessionId, string questionText, string[] options)
    {
        await Clients.Group(sessionId).SendAsync("VotingStarted", new
        {
            QuestionText = questionText,
            Options = options,
            TimeLimit = 10000 // 10 seconds
        });
        
        // Auto-end voting after timeout
        _ = Task.Delay(10000).ContinueWith(_ => EndVoting(sessionId));
    }
    
    public async Task SubmitVote(string sessionId, string option)
    {
        var vote = new ATAVote
        {
            SessionId = sessionId,
            ParticipantId = Context.ConnectionId,
            SelectedOption = option,
            SubmittedAt = DateTime.UtcNow
        };
        
        await _context.ATAVotes.AddAsync(vote);
        await _context.SaveChangesAsync();
        
        // Broadcast updated percentages in real-time
        var percentages = await CalculatePercentagesAsync(sessionId);
        await Clients.Group(sessionId).SendAsync("VotesUpdated", percentages);
    }
    
    public async Task EndVoting(string sessionId)
    {
        var finalResults = await CalculatePercentagesAsync(sessionId);
        await Clients.Group(sessionId).SendAsync("VotingEnded", finalResults);
    }
    
    private async Task<Dictionary<string, double>> CalculatePercentagesAsync(string sessionId)
    {
        var votes = await _context.ATAVotes
            .Where(v => v.SessionId == sessionId)
            .GroupBy(v => v.SelectedOption)
            .Select(g => new { Option = g.Key, Count = g.Count() })
            .ToListAsync();
        
        var total = votes.Sum(v => v.Count);
        return votes.ToDictionary(
            v => v.Option,
            v => total > 0 ? (double)v.Count / total * 100 : 0
        );
    }
}
```

#### 3.3 ATA UI Components (PWA)
```javascript
// ATA voting screen
const ATAVotingScreen = () => {
    const [question, setQuestion] = useState(null);
    const [selectedOption, setSelectedOption] = useState(null);
    const [results, setResults] = useState({});
    const [votingEnded, setVotingEnded] = useState(false);
    
    useEffect(() => {
        hubConnection.on("VotingStarted", (data) => {
            setQuestion(data);
            setVotingEnded(false);
        });
        
        hubConnection.on("VotesUpdated", (percentages) => {
            setResults(percentages);
        });
        
        hubConnection.on("VotingEnded", (finalResults) => {
            setResults(finalResults);
            setVotingEnded(true);
        });
    }, []);
    
    const submitVote = async (option) => {
        setSelectedOption(option);
        await hubConnection.invoke("SubmitVote", sessionId, option);
    };
    
    return (
        <div className="ata-voting">
            <h2>{question?.questionText}</h2>
            <div className="options">
                {question?.options.map(opt => (
                    <button
                        key={opt}
                        onClick={() => submitVote(opt)}
                        disabled={selectedOption || votingEnded}
                        className={selectedOption === opt ? 'selected' : ''}
                    >
                        {opt}
                        {votingEnded && <span>{results[opt]?.toFixed(1)}%</span>}
                    </button>
                ))}
            </div>
            <div className="results-bar">
                {Object.entries(results).map(([opt, pct]) => (
                    <div key={opt} className="bar" style={{width: `${pct}%`}}>
                        {opt}: {pct.toFixed(1)}%
                    </div>
                ))}
            </div>
        </div>
    );
};
```

#### 3.4 Host Display Integration
```csharp
// In MillionaireGame/Forms/GameScreen.cs
private void btnAskAudience_Click(object sender, EventArgs e)
{
    // Start ATA voting
    var question = currentQuestion.Text;
    var options = new[] { currentQuestion.A, currentQuestion.B, currentQuestion.C, currentQuestion.D };
    
    _ataHostClient.StartVotingAsync(question, options);
    
    // Show ATA results panel
    pnlATAResults.Visible = true;
}

public class ATAHostClient
{
    private HubConnection _hubConnection;
    
    public event EventHandler<ATAResultsEventArgs> ResultsReceived;
    
    public async Task ConnectAsync(string serverUrl, string sessionId)
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{serverUrl}/hubs/ata?sessionId={sessionId}")
            .Build();
        
        _hubConnection.On<Dictionary<string, double>>("VotesUpdated", (results) => {
            ResultsReceived?.Invoke(this, new ATAResultsEventArgs(results));
        });
        
        await _hubConnection.StartAsync();
    }
    
    public async Task StartVotingAsync(string questionText, string[] options)
    {
        await _hubConnection.InvokeAsync("StartVoting", _sessionId, questionText, options);
    }
}
```

### Deliverables
- ✅ ATA voting API endpoints
- ✅ ATA hub with real-time vote aggregation
- ✅ Live percentage calculation and broadcast
- ✅ Timer-based voting lifecycle
- ✅ PWA voting UI components
- ✅ Host display integration for live results

---

## Phase 4: PWA & Mobile UI

**Estimated Time**: 6-8 hours

### Objectives
- Create responsive Progressive Web App
- QR code join flow
- Offline capability with service workers
- Install prompts for mobile home screen
- Participant experience for FFF and ATA

### Tasks

#### 4.1 Create PWA Project Structure
```
MillionaireGame.Web/
├── wwwroot/
│   ├── index.html
│   ├── manifest.json
│   ├── service-worker.js
│   ├── css/
│   │   └── app.css
│   ├── js/
│   │   ├── app.js
│   │   ├── signalr.min.js
│   │   └── qr-scanner.min.js
│   └── icons/
│       ├── icon-192.png
│       └── icon-512.png
```

#### 4.2 Create Web Manifest
```json
// wwwroot/manifest.json
{
  "name": "Millionaire Game - Audience",
  "short_name": "Millionaire",
  "description": "Join the Millionaire Game audience participation",
  "start_url": "/",
  "display": "standalone",
  "background_color": "#000033",
  "theme_color": "#FFD700",
  "icons": [
    {
      "src": "/icons/icon-192.png",
      "sizes": "192x192",
      "type": "image/png"
    },
    {
      "src": "/icons/icon-512.png",
      "sizes": "512x512",
      "type": "image/png"
    }
  ]
}
```

#### 4.3 Implement Service Worker
```javascript
// wwwroot/service-worker.js
const CACHE_NAME = 'millionaire-v1';
const urlsToCache = [
  '/',
  '/index.html',
  '/css/app.css',
  '/js/app.js',
  '/js/signalr.min.js',
  '/icons/icon-192.png'
];

self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then(cache => cache.addAll(urlsToCache))
  );
});

self.addEventListener('fetch', event => {
  event.respondWith(
    caches.match(event.request)
      .then(response => response || fetch(event.request))
  );
});
```

#### 4.4 Create Join Flow UI
```html
<!-- wwwroot/index.html -->
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Millionaire Game</title>
    <link rel="manifest" href="/manifest.json">
    <link rel="stylesheet" href="/css/app.css">
</head>
<body>
    <div id="app">
        <!-- Join Screen -->
        <div id="joinScreen" class="screen active">
            <h1>Join Millionaire Game</h1>
            <input type="text" id="displayName" placeholder="Enter your name">
            <input type="text" id="sessionCode" placeholder="Session code (or scan QR)">
            <button id="btnJoin">Join Session</button>
            <button id="btnScanQR">Scan QR Code</button>
        </div>
        
        <!-- Waiting Screen -->
        <div id="waitingScreen" class="screen">
            <h1>Connected!</h1>
            <p>Waiting for the game to start...</p>
            <p id="participantCount">0 participants online</p>
        </div>
        
        <!-- FFF Screen -->
        <div id="fffScreen" class="screen">
            <h2 id="fffQuestion"></h2>
            <div id="fffOptions" class="options-grid"></div>
            <div id="fffTimer"></div>
        </div>
        
        <!-- ATA Screen -->
        <div id="ataScreen" class="screen">
            <h2 id="ataQuestion"></h2>
            <div id="ataOptions" class="options-grid"></div>
            <div id="ataResults" class="results-bars"></div>
        </div>
    </div>
    
    <script src="/js/signalr.min.js"></script>
    <script src="/js/app.js"></script>
</body>
</html>
```

#### 4.5 Implement Client-Side Logic
```javascript
// wwwroot/js/app.js
class MillionaireClient {
    constructor() {
        this.sessionId = null;
        this.participantId = null;
        this.hubConnection = null;
    }
    
    async joinSession(displayName, sessionCode) {
        this.sessionId = sessionCode;
        
        // Connect to SignalR hubs
        this.hubConnection = new signalR.HubConnectionBuilder()
            .withUrl(`/hubs/fff?sessionId=${sessionCode}`)
            .build();
        
        // Register event handlers
        this.hubConnection.on("QuestionStarted", this.onQuestionStarted.bind(this));
        this.hubConnection.on("VotingStarted", this.onVotingStarted.bind(this));
        this.hubConnection.on("AnswerReceived", this.onAnswerReceived.bind(this));
        
        await this.hubConnection.start();
        await this.hubConnection.invoke("JoinSession", sessionCode, displayName);
        
        this.showScreen('waitingScreen');
    }
    
    onQuestionStarted(data) {
        document.getElementById('fffQuestion').textContent = data.question;
        const optionsContainer = document.getElementById('fffOptions');
        optionsContainer.innerHTML = '';
        
        data.options.forEach((opt, idx) => {
            const btn = document.createElement('button');
            btn.textContent = ['A', 'B', 'C', 'D'][idx] + ': ' + opt;
            btn.dataset.option = ['A', 'B', 'C', 'D'][idx];
            btn.addEventListener('click', () => this.selectOption(btn));
            optionsContainer.appendChild(btn);
        });
        
        this.showScreen('fffScreen');
        this.startTimer(data.timeLimit);
    }
    
    async submitFFFAnswer() {
        const selectedOptions = Array.from(document.querySelectorAll('.option.selected'))
            .map(btn => btn.dataset.option)
            .join(',');
        
        await this.hubConnection.invoke("SubmitAnswer", this.sessionId, selectedOptions);
    }
    
    showScreen(screenId) {
        document.querySelectorAll('.screen').forEach(s => s.classList.remove('active'));
        document.getElementById(screenId).classList.add('active');
    }
}

// Initialize app
const app = new MillionaireClient();

document.getElementById('btnJoin').addEventListener('click', () => {
    const name = document.getElementById('displayName').value;
    const code = document.getElementById('sessionCode').value;
    if (name && code) {
        app.joinSession(name, code);
    }
});

// Register service worker
if ('serviceWorker' in navigator) {
    navigator.serviceWorker.register('/service-worker.js');
}
```

#### 4.6 Responsive CSS
```css
/* wwwroot/css/app.css */
:root {
    --primary: #000033;
    --secondary: #FFD700;
    --correct: #00FF00;
    --incorrect: #FF0000;
}

* {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

body {
    font-family: 'Segoe UI', Arial, sans-serif;
    background: linear-gradient(135deg, var(--primary), #001155);
    color: white;
    min-height: 100vh;
    display: flex;
    justify-content: center;
    align-items: center;
}

.screen {
    display: none;
    max-width: 600px;
    width: 100%;
    padding: 20px;
}

.screen.active {
    display: block;
}

h1, h2 {
    color: var(--secondary);
    text-align: center;
    margin-bottom: 20px;
}

input, button {
    width: 100%;
    padding: 15px;
    margin: 10px 0;
    font-size: 16px;
    border: none;
    border-radius: 8px;
}

button {
    background: var(--secondary);
    color: var(--primary);
    font-weight: bold;
    cursor: pointer;
    transition: transform 0.2s;
}

button:hover {
    transform: scale(1.05);
}

button:active {
    transform: scale(0.95);
}

.options-grid {
    display: grid;
    grid-template-columns: 1fr;
    gap: 10px;
    margin: 20px 0;
}

.option {
    background: rgba(255, 255, 255, 0.1);
    border: 2px solid transparent;
    transition: all 0.3s;
}

.option.selected {
    background: var(--secondary);
    color: var(--primary);
    border-color: white;
}

@media (min-width: 768px) {
    .options-grid {
        grid-template-columns: 1fr 1fr;
    }
}
```

### Deliverables
- ✅ Progressive Web App with install capability
- ✅ Responsive UI for mobile/tablet/desktop
- ✅ QR code join flow
- ✅ Service worker for offline capability
- ✅ FFF and ATA participant screens
- ✅ Real-time SignalR integration

---

## Phase 5: Main App Integration

**Estimated Time**: 4-5 hours

### Objectives
- Integrate web server into main WinForms app
- Display QR code on TV screen
- Control panel for host (start FFF, start ATA, view participants)
- Embed web server or run as separate process

### Tasks

#### 5.1 Add Web Server Host to Main App
```csharp
// In MillionaireGame project
// Add NuGet: Microsoft.AspNetCore.Hosting.WindowsServices

public class WebServerHost
{
    private IHost _host;
    private string _baseUrl = "http://localhost:5000";
    
    public async Task StartAsync()
    {
        var builder = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.UseUrls(_baseUrl);
            });
        
        _host = builder.Build();
        await _host.StartAsync();
    }
    
    public async Task StopAsync()
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
    }
    
    public string GetJoinUrl(string sessionId)
    {
        return $"{_baseUrl}?session={sessionId}";
    }
}
```

#### 5.2 Add QR Code Display to Game Screen
```csharp
// Forms/GameScreen.cs
private void ShowAudienceJoinQR()
{
    var sessionId = _sessionService.CurrentSessionId;
    var joinUrl = _webServerHost.GetJoinUrl(sessionId);
    
    // Generate QR code
    using (var qrGenerator = new QRCodeGenerator())
    {
        var qrCodeData = qrGenerator.CreateQrCode(joinUrl, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new QRCode(qrCodeData);
        var qrBitmap = qrCode.GetGraphic(20);
        
        picQRCode.Image = qrBitmap;
    }
    
    lblJoinUrl.Text = $"Join at: {joinUrl}";
    pnlQRCode.Visible = true;
}
```

#### 5.3 Add Audience Control Panel
```csharp
// Forms/ControlPanel.cs - New tab for audience management
public partial class AudienceControlTab : UserControl
{
    private SessionService _sessionService;
    private FFFHostClient _fffClient;
    private ATAHostClient _ataClient;
    
    public AudienceControlTab()
    {
        InitializeComponent();
        InitializeSignalRClients();
    }
    
    private async void btnStartSession_Click(object sender, EventArgs e)
    {
        var session = await _sessionService.CreateSessionAsync(Environment.MachineName);
        txtSessionId.Text = session.Id;
        
        await _fffClient.ConnectAsync("http://localhost:5000", session.Id);
        await _ataClient.ConnectAsync("http://localhost:5000", session.Id);
        
        btnStartFFF.Enabled = true;
        btnStartATA.Enabled = true;
        
        // Start refreshing participant list
        timerRefreshParticipants.Start();
    }
    
    private async void btnStartFFF_Click(object sender, EventArgs e)
    {
        var selectedQuestion = (FFFQuestion)cmbFFFQuestions.SelectedItem;
        await _fffClient.StartQuestionAsync(selectedQuestion.Id);
    }
    
    private async void btnStartATA_Click(object sender, EventArgs e)
    {
        var questionText = txtCurrentQuestion.Text;
        var options = new[] { txtOptionA.Text, txtOptionB.Text, txtOptionC.Text, txtOptionD.Text };
        await _ataClient.StartVotingAsync(questionText, options);
    }
    
    private async void timerRefreshParticipants_Tick(object sender, EventArgs e)
    {
        var participants = await _sessionService.GetActiveParticipantsAsync(txtSessionId.Text);
        lstParticipants.DataSource = participants.ToList();
        lblParticipantCount.Text = $"{participants.Count()} participants online";
    }
}
```

#### 5.4 Start Web Server on App Launch
```csharp
// Program.cs or Main Form constructor
public partial class frmMain : Form
{
    private WebServerHost _webServer;
    
    public frmMain()
    {
        InitializeComponent();
        InitializeWebServer();
    }
    
    private async void InitializeWebServer()
    {
        try
        {
            _webServer = new WebServerHost();
            await _webServer.StartAsync();
            
            // Show success indicator
            lblWebServerStatus.Text = "✓ Web server running";
            lblWebServerStatus.ForeColor = Color.Green;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to start web server: {ex.Message}", 
                "Web Server Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            lblWebServerStatus.Text = "✗ Web server offline";
            lblWebServerStatus.ForeColor = Color.Red;
        }
    }
    
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _webServer?.StopAsync().Wait();
        base.OnFormClosing(e);
    }
}
```

#### 5.5 Display Live ATA Results
```csharp
// Forms/GameScreen.cs
private void InitializeATAResultsDisplay()
{
    _ataClient.ResultsReceived += (sender, args) =>
    {
        this.InvokeIfRequired(() =>
        {
            // Update percentage bars
            barA.Value = (int)args.Results["A"];
            barB.Value = (int)args.Results["B"];
            barC.Value = (int)args.Results["C"];
            barD.Value = (int)args.Results["D"];
            
            lblA.Text = $"A: {args.Results["A"]:F1}%";
            lblB.Text = $"B: {args.Results["B"]:F1}%";
            lblC.Text = $"C: {args.Results["C"]:F1}%";
            lblD.Text = $"D: {args.Results["D"]:F1}%";
        });
    };
}
```

### Deliverables
- ✅ Web server hosted within main WinForms app
- ✅ QR code display on game screen
- ✅ Audience control panel for host
- ✅ Live participant count
- ✅ FFF and ATA control buttons
- ✅ Live ATA results display on TV screen

---

## Testing & Validation

### Unit Tests
- Session creation and management
- FFF answer validation logic
- ATA vote aggregation
- QR code generation

### Integration Tests
- SignalR hub connectivity
- Real-time message delivery
- Database operations with concurrency
- Timer-based lifecycle events

### End-to-End Tests
1. **FFF Flow**:
   - Host creates session and displays QR code
   - 5+ participants join via mobile
   - Host starts FFF question
   - Participants submit answers within time limit
   - System calculates rankings correctly
   - Winner is displayed on all screens

2. **ATA Flow**:
   - Host starts ATA voting
   - Participants vote from mobile devices
   - Live percentages update in real-time
   - Results display correctly on TV screen
   - Host can use results for gameplay

3. **Mixed Flow**:
   - Session supports both FFF and ATA
   - Participants remain connected across modes
   - No data leakage between questions
   - Clean transitions between game modes

### Performance Tests
- 50+ concurrent participants
- Answer submission under load
- Real-time update latency (<100ms)
- QR code generation speed
- Database query optimization

---

## Deployment Considerations

### Development
- Run web server on localhost:5000
- Use SQLite database in project directory
- Hot reload for PWA development

### Production
- Host web server on local network (e.g., 192.168.1.100:5000)
- Configure firewall to allow inbound port 5000
- Consider HTTPS with self-signed certificate for security
- Database backup strategy for session history

### Alternative: Cloud Deployment (Optional)
- Deploy web API to Azure App Service
- Use Azure SignalR Service for scaling
- Azure SQL or Cosmos DB for cloud database
- Allows participants to join from anywhere (not just local network)

---

## Future Enhancements

### Phase 6: Advanced Features (Post-MVP)
- **Participant Profiles**: Save player history, stats, leaderboards
- **Replay System**: Record and replay FFF/ATA sessions
- **Analytics Dashboard**: Session statistics, participation rates
- **Custom Lifelines**: 50:50 voting, double dip, etc.
- **Multi-Language Support**: Internationalization for PWA
- **Voice Commands**: Alexa/Google Assistant integration
- **Accessibility**: Screen reader support, high contrast mode
- **Gamification**: Achievements, badges, seasonal leaderboards

### Phase 7: Monetization (Optional)
- **Premium Features**: Custom branding, advanced analytics
- **Subscription Model**: Host multiple concurrent sessions
- **Ad-Free Experience**: Remove ads for premium users

---

## Resource Requirements

### Development Tools
- Visual Studio 2022 (17.8+)
- .NET 8 SDK
- Node.js (for PWA build tools, optional)
- Postman or Thunder Client (API testing)
- Browser DevTools (PWA debugging)

### NuGet Packages
```xml
<PackageReference Include="Microsoft.AspNetCore.SignalR" Version="8.0.*" />
<PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="8.0.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.*" />
<PackageReference Include="QRCoder" Version="1.5.1" />
<PackageReference Include="Microsoft.AspNetCore.Hosting.WindowsServices" Version="8.0.*" />
```

### Documentation
- ASP.NET Core SignalR: https://learn.microsoft.com/en-us/aspnet/core/signalr/
- Progressive Web Apps: https://web.dev/progressive-web-apps/
- Entity Framework Core: https://learn.microsoft.com/en-us/ef/core/

---

## Timeline Summary

| Phase | Description | Estimated Time |
|-------|-------------|----------------|
| 1 | Foundation & Infrastructure | 4-6 hours |
| 2 | FFF System Implementation | 6-8 hours |
| 3 | Real ATA Voting System | 4-5 hours |
| 4 | PWA & Mobile UI | 6-8 hours |
| 5 | Main App Integration | 4-5 hours |
| **Total** | **MVP Complete** | **24-32 hours** |

### Recommended Schedule
- **Week 1**: Phase 1 + Phase 2 (Foundation + FFF)
- **Week 2**: Phase 3 + Phase 4 (ATA + PWA)
- **Week 3**: Phase 5 + Testing (Integration + QA)

---

## Success Criteria

### MVP Launch Checklist
- [ ] Session creation and QR code generation working
- [ ] Participants can join via mobile browser
- [ ] FFF questions can be started and answered
- [ ] FFF rankings calculated correctly and displayed
- [ ] ATA voting works with real-time percentage updates
- [ ] Results display on host's TV screen
- [ ] PWA installable on iOS and Android
- [ ] Service worker provides offline capability
- [ ] Web server runs within WinForms app
- [ ] No crashes with 50+ concurrent users

### Quality Gates
- [ ] Unit test coverage >70%
- [ ] Zero critical bugs in production
- [ ] API response time <200ms (95th percentile)
- [ ] SignalR message latency <100ms
- [ ] PWA lighthouse score >80
- [ ] Mobile-friendly responsive design
- [ ] Accessibility score >85

---

## Notes

- This replaces **4 separate TODO items** (FFF, ATA, Mobile, QR) with **1 unified system**
- Estimated time savings: **15-20 hours** vs. building old system then rebuilding
- Modern tech stack ensures **long-term maintainability**
- Progressive Web App eliminates **client installation headaches**
- SignalR provides **production-ready real-time communication**
- Database schema extensions preserve **existing data and workflows**

---

## Questions & Decisions

### Decision Log
1. **Framework Choice**: ASP.NET Core chosen for .NET ecosystem consistency
2. **Real-Time Tech**: SignalR selected over WebSockets for built-in reconnection
3. **Frontend**: PWA chosen over native apps for cross-platform reach
4. **Database**: Extend existing SQLite to avoid migration complexity
5. **Hosting**: Embed web server in WinForms app for simplicity (can deploy separately later)

### Open Questions
- Should we support multiple concurrent sessions? (Answer: Yes, host can run multiple games)
- Do we need authentication beyond session codes? (Answer: No for MVP, optional later)
- Should votes/answers be anonymous? (Answer: Yes, only display names shown)
- Do we store historical data forever? (Answer: Yes, but add cleanup tools later)

---

*Document Version: 1.0*
*Last Updated: 2024*
*Author: GitHub Copilot*
