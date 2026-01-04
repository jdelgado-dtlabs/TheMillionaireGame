# Contributing to The Millionaire Game

Thank you for your interest in contributing to The Millionaire Game! This guide will help you get started with contributing code, documentation, bug reports, and feature requests.

---

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How Can I Contribute?](#how-can-i-contribute)
- [Development Setup](#development-setup)
- [Coding Standards](#coding-standards)
- [Commit Message Guidelines](#commit-message-guidelines)
- [Pull Request Process](#pull-request-process)
- [Issue Guidelines](#issue-guidelines)
- [Community](#community)

---

## Code of Conduct

### Our Pledge

We are committed to providing a welcoming and inclusive environment for all contributors, regardless of experience level, background, or identity.

### Expected Behavior

- Be respectful and considerate in all interactions
- Provide constructive feedback
- Focus on what is best for the project and community
- Show empathy towards other community members
- Accept constructive criticism gracefully

### Unacceptable Behavior

- Harassment, trolling, or discriminatory comments
- Personal attacks or insults
- Publishing others' private information without permission
- Any conduct that would be considered inappropriate in a professional setting

### Enforcement

Project maintainers have the right to remove, edit, or reject comments, commits, code, issues, and other contributions that do not align with this Code of Conduct.

---

## How Can I Contribute?

### Reporting Bugs

Before submitting a bug report:
1. **Check existing issues** - Search [GitHub Issues](https://github.com/jdelgado-dtlabs/TheMillionaireGame/issues) to see if the bug has already been reported
2. **Update to latest version** - Verify the bug exists in the most recent release
3. **Gather information** - Collect logs, screenshots, and steps to reproduce

**Bug Report Template:**
```markdown
**Describe the bug**
A clear description of what the bug is.

**To Reproduce**
Steps to reproduce the behavior:
1. Go to '...'
2. Click on '...'
3. See error

**Expected behavior**
What you expected to happen.

**Screenshots/Logs**
If applicable, attach screenshots or log files from:
- `%LocalAppData%\TheMillionaireGame\Logs\`
- `%LocalAppData%\TheMillionaireGame\CrashReports\`

**Environment:**
- OS: [e.g., Windows 11 22H2]
- Application Version: [e.g., 1.0.0]
- .NET Version: [e.g., 8.0.1]
- SQL Server Express Version: [e.g., 2022]

**Additional context**
Any other relevant information.
```

### Suggesting Features

We welcome feature suggestions! Before submitting:
1. **Check the roadmap** - Review [Home](Home) for planned features
2. **Search existing requests** - Check if someone has already suggested it
3. **Provide detailed use cases** - Explain why the feature would be valuable

**Feature Request Template:**
```markdown
**Is your feature request related to a problem?**
A clear description of the problem.

**Describe the solution you'd like**
A clear description of what you want to happen.

**Describe alternatives you've considered**
Other solutions or features you've considered.

**Use cases**
Specific scenarios where this feature would be useful.

**Additional context**
Mockups, screenshots, or examples from other applications.
```

### Contributing Code

1. **Fork the repository** - Create your own fork on GitHub
2. **Set up development environment** - Follow [Building from Source](Building-from-Source)
3. **Create a feature branch** - Branch from `master-csharp`
4. **Make your changes** - Follow coding standards below
5. **Test thoroughly** - Ensure your changes don't break existing functionality
6. **Submit a pull request** - Follow the PR process below

### Contributing Documentation

Documentation improvements are always welcome:
- Fix typos or unclear explanations
- Add missing information
- Improve code examples
- Translate documentation (future)

Documentation files are located in:
- `wiki/` - GitHub Wiki markdown files
- `src/docs/` - Development documentation
- Code comments - Inline documentation

---

## Development Setup

### Prerequisites

See [Building from Source - Prerequisites](Building-from-Source#prerequisites) for required software.

### Quick Setup

```powershell
# Clone the repository
git clone https://github.com/jdelgado-dtlabs/TheMillionaireGame.git
cd TheMillionaireGame

# Switch to development branch
git checkout master-csharp

# Build the solution
cd src
dotnet build TheMillionaireGame.sln

# Run the application
cd MillionaireGame/bin/Debug/net8.0-windows
.\MillionaireGame.exe
```

For detailed instructions, see [Building from Source](Building-from-Source).

---

## Coding Standards

### General Guidelines

- **No Blocking UI** - NEVER use `MessageBox.Show()` or modal dialogs in game operations
- **Use GameConsole Logging** - Always use `GameConsole` for logging, never `Console.WriteLine()`
- **Follow Existing Patterns** - Review existing code before adding new features
- **EditorConfig** - Respect `.editorconfig` settings (auto-applied in VS/VS Code)

### Logging

Use appropriate log levels:

```csharp
// ✅ DO - Use GameConsole logging
GameConsole.Debug("Detailed diagnostic info");  // Only shown in debug mode
GameConsole.Info("General information");        // Normal informational messages
GameConsole.Warn("Warning messages");           // Warning messages
GameConsole.Error("Error messages");            // Error messages

// ❌ DON'T - Console.WriteLine not captured in logs
Console.WriteLine("Debug info");

// ❌ DON'T - MessageBox blocks game flow
MessageBox.Show("Error occurred");
```

### Naming Conventions

- **Classes**: `PascalCase` - `GameStateManager`
- **Methods**: `PascalCase` - `LoadQuestion()`
- **Private Fields**: `_camelCase` - `_soundService`
- **Public Properties**: `PascalCase` - `CurrentLevel`
- **Local Variables**: `camelCase` - `questionText`
- **Constants**: `PascalCase` - `MaxQuestions`

### Code Structure

```csharp
// File header comments (optional)
/// <summary>
/// Brief description of the class purpose
/// </summary>
public class MyService
{
    // Private fields
    private readonly ISoundService _soundService;
    private int _currentLevel;

    // Constructor
    public MyService(ISoundService soundService)
    {
        _soundService = soundService;
    }

    // Public properties
    public int CurrentLevel => _currentLevel;

    // Public methods
    public void DoSomething()
    {
        GameConsole.Info("Doing something");
        // Implementation
    }

    // Private methods
    private void HelperMethod()
    {
        // Implementation
    }
}
```

### Error Handling

```csharp
// ✅ DO - Log errors and handle gracefully
try
{
    var result = DoSomething();
}
catch (Exception ex)
{
    GameConsole.Error($"Operation failed: {ex.Message}");
    // Handle gracefully without blocking UI
}

// ❌ DON'T - Catch and ignore errors
try
{
    DoSomething();
}
catch { }

// ❌ DON'T - Show error dialogs
catch (Exception ex)
{
    MessageBox.Show($"Error: {ex.Message}");
}
```

### Database Access

- Use parameterized queries to prevent SQL injection
- Wrap database operations in using statements
- Handle connection failures gracefully
- Log database errors with context

```csharp
// ✅ DO - Safe parameterized query
using (var connection = new SqlConnection(connectionString))
using (var command = new SqlCommand("SELECT * FROM Questions WHERE Level = @Level", connection))
{
    command.Parameters.AddWithValue("@Level", level);
    connection.Open();
    // Execute query
}
```

### Asynchronous Programming

- Use `async`/`await` for I/O operations
- Avoid blocking calls like `.Result` or `.Wait()`
- Handle cancellation tokens appropriately

```csharp
// ✅ DO - Async operations
public async Task LoadQuestionsAsync(CancellationToken cancellationToken)
{
    var questions = await _database.GetQuestionsAsync(cancellationToken);
    // Process questions
}

// ❌ DON'T - Blocking async calls
public void LoadQuestions()
{
    var questions = _database.GetQuestionsAsync(CancellationToken.None).Result;
}
```

---

## Commit Message Guidelines

We follow [Conventional Commits](https://www.conventionalcommits.org/) for clear and structured commit history.

### Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- **feat**: New feature
- **fix**: Bug fix
- **docs**: Documentation changes
- **style**: Code style changes (formatting, whitespace)
- **refactor**: Code refactoring without changing functionality
- **perf**: Performance improvements
- **test**: Adding or updating tests
- **chore**: Maintenance tasks (dependencies, build scripts)
- **ci**: CI/CD pipeline changes

### Examples

```
feat(audio): Add crossfade support for sound transitions

Implemented crossfade settings in Audio Mixer tab allowing smooth
transitions between sound cues. Includes configurable fade duration
and silence detection threshold.

Closes #123
```

```
fix(fff): Correct timer display for FFF Online mode

Timer was showing incorrect countdown values due to timezone offset.
Fixed by using UTC timestamps for calculations.

Fixes #456
```

```
docs(wiki): Update Installation guide with SQL Server Express setup

Added detailed SQL Server Express installation steps and troubleshooting
for connection issues.
```

### Guidelines

- Use present tense ("Add feature" not "Added feature")
- Use imperative mood ("Move cursor to" not "Moves cursor to")
- Limit subject line to 72 characters
- Reference issues and pull requests in footer
- Explain *what* and *why*, not *how* (code shows how)

---

## Pull Request Process

### Before Submitting

1. **Test your changes** - Run the application and test affected features
2. **Follow coding standards** - Ensure code follows project conventions
3. **Update documentation** - Update wiki/docs if functionality changes
4. **Clean commit history** - Squash fixup commits if needed
5. **Pull latest changes** - Rebase on latest `master-csharp` branch

### PR Template

```markdown
## Description
Brief description of changes.

## Type of Change
- [ ] Bug fix (non-breaking change fixing an issue)
- [ ] New feature (non-breaking change adding functionality)
- [ ] Breaking change (fix or feature causing existing functionality to change)
- [ ] Documentation update

## Related Issues
Closes #123
Related to #456

## Testing
Describe testing performed:
- [ ] Tested on Windows 10/11
- [ ] Tested with multiple monitors
- [ ] Tested FFF Online mode
- [ ] Tested all lifelines

## Screenshots (if applicable)
Add screenshots showing changes.

## Checklist
- [ ] Code follows project coding standards
- [ ] Self-review completed
- [ ] Comments added for complex logic
- [ ] Documentation updated
- [ ] No console errors or warnings
- [ ] Tested with SQL Server Express
```

### Review Process

1. **Automated checks** - CI/CD pipeline runs build and checks
2. **Code review** - Maintainer reviews code and provides feedback
3. **Revisions** - Address feedback and push updates
4. **Approval** - Maintainer approves and merges PR

### After Merge

- Delete your feature branch
- Pull latest changes to your fork
- Update your local repository

---

## Issue Guidelines

### Issue Labels

We use labels to categorize issues:

- **bug** - Something isn't working
- **enhancement** - New feature or request
- **documentation** - Documentation improvements
- **good first issue** - Good for newcomers
- **help wanted** - Extra attention needed
- **question** - Further information requested
- **wontfix** - Will not be worked on
- **duplicate** - Issue already exists
- **invalid** - Invalid issue

### Issue Lifecycle

1. **New** - Issue created
2. **Triaged** - Maintainer reviews and labels
3. **Assigned** - Developer assigned to work on it
4. **In Progress** - Work has started
5. **Review** - Pull request submitted
6. **Closed** - Completed or won't fix

---

## Community

### Communication Channels

- **GitHub Issues** - Bug reports and feature requests
- **GitHub Discussions** - General questions and discussions
- **Pull Requests** - Code contributions and reviews

### Getting Help

- **Documentation** - Check [User Guide](User-Guide) and wiki
- **Troubleshooting** - See [Troubleshooting](Troubleshooting) guide
- **Questions** - Ask in [GitHub Discussions](https://github.com/jdelgado-dtlabs/TheMillionaireGame/discussions)

### Recognition

Contributors will be:
- Listed in project credits
- Mentioned in release notes for significant contributions
- Acknowledged in commit history

---

## License

By contributing to The Millionaire Game, you agree that your contributions will be licensed under the [MIT License](https://github.com/jdelgado-dtlabs/TheMillionaireGame/blob/master/LICENSE).

---

## Questions?

If you have questions about contributing, please:
1. Check this guide thoroughly
2. Search existing issues and discussions
3. Ask in [GitHub Discussions](https://github.com/jdelgado-dtlabs/TheMillionaireGame/discussions)

---

**Thank you for contributing to The Millionaire Game!** Your contributions help make this project better for everyone.
