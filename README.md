# The Millionaire Game

A modern C# implementation of the classic "Who Wants to Be a Millionaire?" game show experience, built with .NET 8 and WinForms.

## Quick Start

See [`src/docs/START_HERE.md`](src/docs/START_HERE.md) for complete setup and build instructions.

## Project Structure

- **`src/`** - Main C# solution and projects
  - `MillionaireGame/` - Main game application (WinForms)
  - `MillionaireGame.Core/` - Core business logic and services
  - `MillionaireGame.Web/` - Web API and SignalR backend
  - `MillionaireGame.FFFClient/` - Fastest Finger First client
  - `MillionaireGame.QuestionEditor/` - Question database editor
- **`archive-vbnet/`** - Original VB.NET implementation (archived, will be removed at v1.0)

## Documentation

Comprehensive documentation is available in the [`src/docs/`](src/docs/) folder:

- Project architecture and design
- API reference
- Development guides
- Session notes and changelogs

For detailed codebase documentation and developer manual, please visit the [**GitHub Wiki**](../../wiki).

## Features

- Classic game show experience with authentic sound and visuals
- Fastest Finger First contestant selection
- Three lifelines: Phone-a-Friend, Ask the Audience, 50:50
- Multi-screen support (Host control panel, TV display, audience view)
- Web-based audience participation
- Question database editor with CSV import/export

## Technology Stack

- **.NET 8** (C#, WinForms)
- **CSCore** - Audio engine
- **SignalR** - Real-time web communication
- **SQLite** - Question database

## Development Status

🚧 **Active Development** - Migrating from VB.NET to C#

See [`src/docs/PRE_1.0_FINAL_CHECKLIST.md`](src/docs/active/PRE_1.0_FINAL_CHECKLIST.md) for current progress toward v1.0 release.

## Contributing

This is a personal project, but feedback and suggestions are welcome via Issues.

## License

*License information will be added before v1.0 release.*

---

**Note:** This README is a placeholder and will be expanded with more detailed information as the project approaches v1.0 release. For now, please refer to the documentation in `src/docs/` and the GitHub Wiki.
