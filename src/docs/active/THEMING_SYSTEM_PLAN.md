# Theming System Implementation Plan - v1.0.7

**Status:** Planning  
**Target Release:** v1.0.7  
**Created:** January 12, 2026  
**Owner:** Development Team

## Overview

Implement a comprehensive theming system that allows users to customize the visual appearance of broadcast elements including backgrounds, question/answer straps, money tree displays, and other UI components. The system will provide both preset themes and user-customizable profiles for maximum flexibility.

## Current State

### Existing Assets
- **TV Screen Backgrounds:** Currently selectable per-game backgrounds
- **Question/Answer Straps:** Static overlays (need confirmation of current implementation)
- **Money Tree Backgrounds:** Static assets
- **Background Settings:** Basic background and chroma key configuration

### Current Limitations
- Limited customization options
- No unified theming system
- Static visual elements with minimal user control
- No ability to create cohesive visual themes across all elements

## Goals

### Primary Goals
1. **Unified Theme Management:** Central location for all visual customization
2. **Component-Based Theming:** Individual control over backgrounds, straps, money tree, etc.
3. **User Profiles:** Allow users to create and save 2 custom theme profiles
4. **Preset Themes:** Provide professional preset theme packages
5. **SVG-Based Straps:** Dynamic, customizable strap rendering with effects
6. **Database Storage:** All theme configurations stored in SQL Server database

### Secondary Goals
- Export/import individual theme configurations (database only)
- **Export/import theme packs** (ZIP files with XML config + assets: backgrounds, icons, fonts)
- Theme preview system
- Real-time theme switching during game
- Community theme sharing via theme packs (similar to soundpack distribution)

## Architecture

### Theme Structure

```
Theme
├── Background
│   ├── Image Selection
│   ├── Chroma Key Settings
│   ├── Scaling/Position
│   └── Transparency
├── Straps
│   ├── Shape (SVG-based)
│   ├── Colors (Gradient support)
│   ├── Effects (Sheen, glow, etc.)
│   ├── Typography
│   └── Animation settings
├── Money Tree
│   ├── Background Image
│   ├── Color Scheme
│   ├── Highlight Effects
│   └── Typography
├── Additional Elements
│   ├── Lifeline Overlays
│   ├── Timer Display
│   ├── Audience Graph
│   └── Host/Player Name Cards
└── Metadata
    ├── Theme Name
    ├── Author
    ├── Description
    └── Version
```

## Database Schema

### Tables

#### `Themes`
```sql
CREATE TABLE Themes (
    ThemeId INT PRIMARY KEY IDENTITY(1,1),
    ThemeName NVARCHAR(100) NOT NULL,
    ThemeType NVARCHAR(20) NOT NULL, -- 'Preset', 'UserProfile1', 'UserProfile2', 'Custom'
    ThemePackId INT NULL, -- Reference to pack if theme is part of imported pack
    IsActive BIT DEFAULT 0,
    Description NVARCHAR(500),
    Author NVARCHAR(100),
    Version NVARCHAR(20),
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    ModifiedDate DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (ThemePackId) REFERENCES ThemePacks(ThemePackId) ON DELETE SET NULL,
    CONSTRAINT CK_ThemeType CHECK (ThemeType IN ('Preset', 'UserProfile1', 'UserProfile2', 'Custom'))
);

-- Index for active theme lookup
CREATE INDEX IX_Themes_Active ON Themes(IsActive) WHERE IsActive = 1;
```

#### `ThemeBackgrounds`
```sql
CREATE TABLE ThemeBackgrounds (
    ThemeBackgroundId INT PRIMARY KEY IDENTITY(1,1),
    ThemeId INT NOT NULL,
    ComponentType NVARCHAR(50) NOT NULL, -- 'TVScreen', 'MoneyTree', 'General'
    ImagePath NVARCHAR(500),
    ChromaKeyEnabled BIT DEFAULT 0,
    ChromaKeyColor NVARCHAR(20), -- RGB/HEX value
    ChromaKeyTolerance INT,
    ScaleMode NVARCHAR(20), -- 'Stretch', 'Fill', 'Fit', 'Center'
    PositionX INT DEFAULT 0,
    PositionY INT DEFAULT 0,
    Transparency INT DEFAULT 100, -- 0-100
    FOREIGN KEY (ThemeId) REFERENCES Themes(ThemeId) ON DELETE CASCADE
);
```

#### `ThemeStraps`
```sql
CREATE TABLE ThemeStraps (
    ThemeStrapId INT PRIMARY KEY IDENTITY(1,1),
    ThemeId INT NOT NULL,
    StrapType NVARCHAR(50) NOT NULL, -- 'Question', 'Answer', 'MoneyAmount', etc.
    SvgShape NVARCHAR(50) NOT NULL, -- 'Classic', 'Modern', 'Rounded', 'Sharp', etc.
    
    -- Color Configuration
    PrimaryColor NVARCHAR(20) NOT NULL, -- RGB/HEX
    SecondaryColor NVARCHAR(20), -- For gradients
    GradientEnabled BIT DEFAULT 0,
    GradientAngle INT DEFAULT 90, -- 0-360 degrees
    
    -- Effects
    EffectType NVARCHAR(50), -- 'Sheen', 'Glow', 'Shadow', 'Metallic', 'Silk', etc.
    EffectIntensity INT DEFAULT 50, -- 0-100
    EffectColor NVARCHAR(20),
    
    -- Border
    BorderEnabled BIT DEFAULT 1,
    BorderColor NVARCHAR(20),
    BorderWidth INT DEFAULT 2,
    
    -- Typography
    FontFamily NVARCHAR(100),
    FontSize INT,
    FontColor NVARCHAR(20),
    FontBold BIT DEFAULT 0,
    FontItalic BIT DEFAULT 0,
    
    -- Animation
    AnimationEnabled BIT DEFAULT 0,
    AnimationType NVARCHAR(50), -- 'FadeIn', 'SlideIn', 'Pulse', etc.
    AnimationDuration INT DEFAULT 500, -- milliseconds
    
    FOREIGN KEY (ThemeId) REFERENCES Themes(ThemeId) ON DELETE CASCADE
);
```

#### `ThemeMoneyTree`
```sql
CREATE TABLE ThemeMoneyTree (
    ThemeMoneyTreeId INT PRIMARY KEY IDENTITY(1,1),
    ThemeId INT NOT NULL,
    BackgroundImagePath NVARCHAR(500),
    
    -- Color Scheme
    InactiveAmountColor NVARCHAR(20),
    ActiveAmountColor NVARCHAR(20),
    CompletedAmountColor NVARCHAR(20),
    SafeHavenColor NVARCHAR(20),
    
    -- Highlight Effects
    HighlightEnabled BIT DEFAULT 1,
    HighlightType NVARCHAR(50), -- 'Glow', 'Pulse', 'Border', etc.
    HighlightColor NVARCHAR(20),
    HighlightIntensity INT DEFAULT 50,
    
    -- Typography
    FontFamily NVARCHAR(100),
    FontSize INT,
    FontBold BIT DEFAULT 1,
    
    FOREIGN KEY (ThemeId) REFERENCES Themes(ThemeId) ON DELETE CASCADE
);
```

#### `ThemePresets`
```sql
CREATE TABLE ThemePresets (
    PresetId INT PRIMARY KEY IDENTITY(1,1),
    PresetName NVARCHAR(100) NOT NULL,
    DisplayName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    PreviewImagePath NVARCHAR(500),
    Category NVARCHAR(50), -- 'Classic', 'Modern', 'Elegant', 'Bold', etc.
    IsBuiltIn BIT DEFAULT 1,
    SortOrder INT DEFAULT 0
);
```

#### `ThemePacks`
```sql
CREATE TABLE ThemePacks (
    ThemePackId INT PRIMARY KEY IDENTITY(1,1),
    PackName NVARCHAR(100) NOT NULL,
    PackPath NVARCHAR(500) NOT NULL, -- Path to extracted pack folder
    Description NVARCHAR(500),
    Author NVARCHAR(100),
    Version NVARCHAR(20),
    ImportedDate DATETIME2 DEFAULT GETDATE(),
    IsBuiltIn BIT DEFAULT 0,
    CONSTRAINT UQ_ThemePack_Name UNIQUE (PackName)
);

-- Note: Theme packs contain asset files and XML config
-- Themes reference pack assets via file paths
-- No junction table needed - themes belong to packs via PackId reference
```

## UI/UX Design

### Theme Tab Location
**Path:** Control Panel → Broadcast Tab → Theme (new tab)

### Layout Structure

```
┌─────────────────────────────────────────────────────────────┐
│ Theme Management                                             │
├──────────────────┬──────────────────────────────────────────┤
│ Theme Selector   │                                          │
│ ┌──────────────┐ │  Selected Theme: "Classic Gold"          │
│ │▼ Preset      │ │                                          │
│ ├──────────────┤ │  [Apply] [Save As...] [Export Theme]    │
│ │ Classic Gold │ │                                          │
│ │ Modern Blue  │ │  ┌────────────────────────────────────┐ │
│ │ Elegant Red  │ │  │ Theme Preview                      │ │
│ │ Bold Green   │ │  │                                    │ │
│ └──────────────┘ │  │  [Preview of current selection]   │ │
│ ┌──────────────┐ │  │                                    │ │
│ │▼ User Profile│ │  └────────────────────────────────────┘ │
│ ├──────────────┤ │                                          │
│ │ Profile 1    │ │                                          │
│ │ Profile 2    │ │                                          │
│ └──────────────┘ │                                          │
│                  │  ┌────────────────────────────────────┐ │
│                  │  │ Theme Pack Management              │ │
│                  │  ├────────────────────────────────────┤ │
│                  │  │ [Import Pack] [Export Pack]        │ │
│                  │  │ [Manage Packs...]                  │ │
│                  │  └────────────────────────────────────┘ │
└──────────────────┴──────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ Component Customization                                      │
├──────────────────┬──────────────────────────────────────────┤
│ Components       │ Settings                                 │
│ ┌──────────────┐ │                                          │
│ │► Background  │ │  [Component-specific settings shown     │
│ │► Straps      │ │   based on left panel selection]        │
│ │► Money Tree  │ │                                          │
│ │  Lifelines   │ │  [Live Preview]                         │
│ │  Timer       │ │                                          │
│ │  Audience    │ │                                          │
│ │  Name Cards  │ │                                          │
│ └──────────────┘ │                                          │
└──────────────────┴──────────────────────────────────────────┘
```

### Component Panels

#### 1. Background Settings Panel
```
┌─────────────────────────────────────────────────────────┐
│ Background Settings                                     │
├─────────────────────────────────────────────────────────┤
│ Component Type: [▼ TV Screen     ]                     │
│                                                         │
│ Image Selection                                         │
│ ┌─────────────────────────────────────────┐           │
│ │ [Current Background Preview]            │           │
│ └─────────────────────────────────────────┘           │
│ [Browse...] [Clear]                                    │
│                                                         │
│ Positioning                                            │
│ Scale Mode:  [▼ Fill          ]                       │
│ Position X:  [____0___] px                            │
│ Position Y:  [____0___] px                            │
│ Transparency: [████████░░] 80%                         │
│                                                         │
│ Chroma Key Settings                                    │
│ ☑ Enable Chroma Key                                   │
│ Key Color:   [🎨] [#00FF00]                           │
│ Tolerance:   [█████░░░░░] 50                          │
│ Smoothing:   [███████░░░] 70                          │
└─────────────────────────────────────────────────────────┘
```

#### 2. Straps Settings Panel (PRIMARY FOCUS)
```
┌─────────────────────────────────────────────────────────┐
│ Strap Settings                                          │
├─────────────────────────────────────────────────────────┤
│ Strap Type: [▼ Question      ]                         │
│                                                         │
│ Shape & Style                                          │
│ SVG Shape:   [▼ Classic Ribbon]                       │
│ ┌───┐ ┌───┐ ┌───┐ ┌───┐                              │
│ │ 🎗 │ │ ▬ │ │ ⬭ │ │ ◈ │  [More shapes...]          │
│ └───┘ └───┘ └───┘ └───┘                              │
│                                                         │
│ Colors                                                 │
│ Primary:     [🎨] [#8B4513] ████████                  │
│ Secondary:   [🎨] [#D4AF37] ████████                  │
│ ☑ Enable Gradient  Angle: [__90__]°                   │
│                                                         │
│ Effects                                                │
│ Effect Type:  [▼ Silk Sheen    ]                      │
│ Intensity:    [██████░░░░] 60%                         │
│ Effect Color: [🎨] [#FFFFFF] (Optional)               │
│                                                         │
│ ┌─────────────────────────────────────┐               │
│ │ Effect Preview:                     │               │
│ │ ╔════════════════════════════════╗  │               │
│ │ ║  Sample Question Text Here... ║  │               │
│ │ ╚════════════════════════════════╝  │               │
│ └─────────────────────────────────────┘               │
│                                                         │
│ Border                                                 │
│ ☑ Enable Border                                       │
│ Color:       [🎨] [#000000] ████████                  │
│ Width:       [___2___] px                             │
│                                                         │
│ Typography                                             │
│ Font:        [▼ Copperplate Gothic]                   │
│ Size:        [__24__] pt                              │
│ Color:       [🎨] [#FFFFFF] ████████                  │
│ ☑ Bold  ☐ Italic                                      │
│                                                         │
│ Animation                                              │
│ ☑ Enable Animation                                    │
│ Type:        [▼ Fade In       ]                       │
│ Duration:    [_500_] ms                               │
└─────────────────────────────────────────────────────────┘
```

#### 3. Money Tree Settings Panel
```
┌─────────────────────────────────────────────────────────┐
│ Money Tree Settings                                     │
├─────────────────────────────────────────────────────────┤
│ Background                                              │
│ ┌─────────────────────────────────────────┐           │
│ │ [Money Tree Background Preview]         │           │
│ └─────────────────────────────────────────┘           │
│ [Browse...] [Clear]                                    │
│                                                         │
│ Amount Colors                                          │
│ Inactive:     [🎨] [#808080] ████████                 │
│ Active:       [🎨] [#FFD700] ████████                 │
│ Completed:    [🎨] [#00FF00] ████████                 │
│ Safe Haven:   [🎨] [#0080FF] ████████                 │
│                                                         │
│ Highlight Effect                                       │
│ ☑ Enable Highlight                                    │
│ Type:         [▼ Pulsing Glow ]                       │
│ Color:        [🎨] [#FFFF00] ████████                 │
│ Intensity:    [████████░░] 80%                         │
│                                                         │
│ Typography                                             │
│ Font:         [▼ Arial Bold   ]                       │
│ Size:         [__18__] pt                             │
│ ☑ Bold                                                │
└─────────────────────────────────────────────────────────┘
```

### Preset Theme Groups

#### Built-In Presets

1. **Classic Gold**
   - Gold and brown color scheme
   - Traditional ribbon straps with silk sheen
   - Elegant serif typography
   - Subtle glow effects

2. **Modern Blue**
   - Blue and silver gradient
   - Sleek angular straps with metallic finish
   - Sans-serif typography
   - Sharp, clean lines

3. **Elegant Red**
   - Deep red and gold
   - Ornate straps with velvet texture effect
   - Decorative serif font
   - Luxurious feel

4. **Bold Green**
   - Vibrant green and white
   - High contrast straps with neon glow
   - Modern bold font
   - Dynamic animations

5. **Professional Purple**
   - Purple with silver accents
   - Flowing silk sheet effect on straps
   - Professional typography
   - Smooth transitions

6. **Midnight Black**
   - Black and gold premium theme
   - Glossy straps with reflection effect
   - Premium feel
   - Dramatic lighting

### User Profile Management

**Profile Slots:** 2 user-customizable profiles

**Workflow:**
1. Select a preset or start from scratch
2. Customize all components as desired
3. Click "Save to Profile" → Choose Profile 1 or Profile 2
4. Profile is saved with all settings
5. Can be recalled, edited, or exported later

### Theme Pack Management

**Purpose:** Bundle themes with all required assets (backgrounds, icons, fonts) for easy distribution

**Pack Structure (Similar to Soundpacks):**
```
MyThemePack.zip
├── pack.xml              # Pack metadata and theme configurations
├── backgrounds/          # Background images
│   ├── main_bg.png
│   ├── money_tree_bg.png
│   └── ...
├── icons/                # Custom lifeline icons, etc.
│   ├── 50_50.png
│   ├── phone_friend.png
│   ├── ask_audience.png
│   └── ...
├── fonts/                # Optional custom fonts
│   └── CustomFont.ttf
└── preview.png           # Pack preview image
```

**Export Pack Workflow:**
1. Click "Export Pack" button
2. Select theme(s) to include
3. Select assets to bundle:
   - Backgrounds used by theme(s)
   - Custom icons
   - Custom fonts
4. Enter pack metadata (name, description, author, version)
5. Choose export location
6. System creates ZIP with XML config + assets

**Import Pack Workflow:**
1. Click "Import Pack" button
2. Browse to `.zip` pack file
3. System extracts to `lib/themepacks/[PackName]/`
4. Parse `pack.xml` to load theme definitions
5. Preview pack contents (themes, assets)
6. Confirm import
7. Themes added to database with references to pack assets
8. Pack tracked in `ThemePacks` table

**Pack Management Dialog:**
- View all installed packs
- See themes and assets in each pack
- Delete entire pack (removes themes + assets)
- Reload/refresh pack
- View pack metadata

## Implementation Plan

### Phase 1: Database & Repository Layer (Week 1)

#### Tasks
- [ ] Create database migration script with all theme tables
- [ ] Implement `ThemeRepository` class
  ```csharp
  public interface IThemeRepository
  {
      Task<Theme> GetActiveThemeAsync();
      Task<IEnumerable<Theme>> GetPresetThemesAsync();
      Task<Theme> GetUserProfileAsync(int profileNumber);
      Task<int> SaveThemeAsync(Theme theme);
      Task<bool> SetActiveThemeAsync(int themeId);
      Task<bool> DeleteThemeAsync(int themeId);
      Task<Theme> ExportThemeAsync(int themeId);
      Task<int> ImportThemeAsync(Theme theme);
  }
  ```
- [ ] Implement `ThemeBackgroundRepository`
- [ ] Implement `ThemeStrapRepository`
- [ ] Implement `ThemeMoneyTreeRepository`
- [ ] Implement `ThemePresetRepository`
- [ ] Implement `ThemePackRepository`
  ```csharp
  public interface IThemePackRepository
  {
      Task<IEnumerable<ThemePack>> GetAllPacksAsync();
      Task<ThemePack> GetPackByIdAsync(int packId);
      Task<ThemePack> GetPackByNameAsync(string packName);
      Task<IEnumerable<Theme>> GetPackThemesAsync(int packId);
      Task<int> RegisterPackAsync(ThemePack pack); // Register imported pack
      Task<bool> DeletePackAsync(int packId);
      Task<bool> UpdatePackMetadataAsync(ThemePack pack);
      Task<string> GetPackPathAsync(int packId);
  }
  ```
- [ ] Create seed data for built-in preset themes
- [ ] Unit tests for all repositories

### Phase 2: Core Services & Models (Week 2)

#### Tasks
- [ ] Create `Theme` model class hierarchy
  ```csharp
  public class Theme
  {
      public int ThemeId { get; set; }
      public string ThemeName { get; set; }
      public ThemeType ThemeType { get; set; }
      public bool IsActive { get; set; }
      public ThemeBackground Background { get; set; }
      public List<ThemeStrap> Straps { get; set; }
      public ThemeMoneyTree MoneyTree { get; set; }
      // ... additional properties
  }
  ```
- [ ] Implement `ThemeService` for business logic
  ```csharp
  public interface IThemeService
  {
      Task<Theme> GetCurrentThemeAsync();
      Task ApplyThemeAsync(int themeId);
      Task<Theme> CreateCustomThemeAsync(string name);
      Task SaveToUserProfileAsync(Theme theme, int profileNumber);
      Task<byte[]> ExportThemeAsJsonAsync(int themeId);
      Task<int> ImportThemeFromJsonAsync(byte[] data);
      Task<IEnumerable<ThemePreset>> GetPresetsAsync();
      
      // Theme Pack methods (ZIP files with XML + assets)
      Task<string> ExportThemePackAsync(List<int> themeIds, ThemePackMetadata metadata, string outputPath);
      Task<ThemePackImportResult> ImportThemePackAsync(string zipFilePath);
      Task<IEnumerable<ThemePack>> GetThemePacksAsync();
      Task<bool> DeleteThemePackAsync(int packId); // Removes pack folder and database entry
      Task<bool> ReloadThemePackAsync(int packId);
  }
  ```
- [ ] Implement SVG strap renderer
  ```csharp
  public interface IStrapRenderer
  {
      string GenerateSvg(ThemeStrap strap, string text);
      Image RenderToImage(ThemeStrap strap, string text);
  }
  ```
- [ ] Create effect processors (sheen, glow, metallic, silk, etc.)
- [ ] Implement theme validation logic
- [ ] Implement `ThemePackParser` for XML parsing
  ```csharp
  public interface IThemePackParser
  {
      ThemePackManifest ParsePackXml(string xmlPath);
      string GeneratePackXml(ThemePack pack, List<Theme> themes);
      bool ValidatePackStructure(string packPath);
  }
  ```
- [ ] Implement `ThemePackHandler` for ZIP operations
  ```csharp
  public interface IThemePackHandler
  {
      Task<string> ExtractPackAsync(string zipPath, string destPath);
      Task<string> CreatePackAsync(string sourcePath, string outputPath);
      Task<bool> DeletePackAsync(string packPath);
  }
  ```
- [ ] Unit tests for services

### Phase 3: SVG Strap System (Week 3)

#### Tasks
- [ ] Design base SVG shapes for straps
  - Classic Ribbon
  - Modern Angular
  - Rounded Rectangle
  - Sharp Diamond
  - Ornate Frame
  - Minimalist Bar
- [ ] Implement SVG effect filters
  ```xml
  <!-- Silk Sheen Effect -->
  <defs>
    <linearGradient id="silkSheen">
      <stop offset="0%" stop-color="#ffffff" stop-opacity="0.3"/>
      <stop offset="50%" stop-color="#ffffff" stop-opacity="0.1"/>
      <stop offset="100%" stop-color="#ffffff" stop-opacity="0.3"/>
    </linearGradient>
  </defs>
  ```
- [ ] Create effect library:
  - Silk Sheen (flowing diagonal gradient)
  - Metallic (reflective gradient)
  - Glow (outer shadow/blur)
  - Shadow (drop shadow)
  - Emboss (3D effect)
  - Glass (translucent with highlights)
- [ ] Implement gradient system (linear, radial, custom angles)
- [ ] Text rendering within SVG paths
- [ ] SVG-to-Image converter for display
- [ ] Performance optimization for real-time updates
- [ ] Create strap preview component

### Phase 4: UI Components (Week 4-5)

#### Tasks
- [ ] Create `ThemeTabControl` (new tab under Broadcast)
- [ ] Implement theme selector ComboBox with preview
- [ ] Create two-panel layout (component list + settings)
- [ ] Implement `BackgroundSettingsPanel`
  - Reuse existing background/chroma key controls
  - Add component type selector
- [ ] Implement `StrapSettingsPanel` (PRIMARY COMPONENT)
  - SVG shape selector with visual thumbnails
  - Color pickers with gradient support
  - Effect type dropdown with intensity slider
  - Border configuration controls
  - Typography controls (font, size, color, style)
  - Animation settings
  - Live preview area
- [ ] Implement `MoneyTreeSettingsPanel`
  - Background selection
  - Color scheme configuration
  - Highlight effect settings
  - Typography settings
- [ ] Create theme preview component
- [ ] Implement preset selection grid with thumbnails
- [ ] User profile management UI
  - Save to Profile 1/2 buttons
  - Profile naming
  - Profile management (rename, delete)
- [ ] Export/Import UI
  - Export single theme as JSON file
  - Import single theme from file
  - Validation and error handling
- [ ] Theme Pack UI
  - Export Pack dialog
    - Multi-select theme list
    - Pack metadata form (name, description, author, tags)
    - Preview generation
  - Import Pack dialog
    - File browser for `.mtpack` files
    - Preview pack contents
    - Selective theme import (checkboxes)
    - Progress indication
  - Manage Packs dialog
    - List all imported packs
    - View pack details
    - Delete pack functionality
    - Re-export functionality
- [ ] Apply/Save/Cancel action buttons
- [ ] Real-time preview updates (debounced)

### Phase 5: Integration (Week 6)

#### Tasks
- [ ] Integrate theme system with game engine
- [ ] Apply theme backgrounds to TV screen
- [ ] Apply theme straps to questions/answers
- [ ] Apply theme to money tree display
- [ ] Integrate with broadcast overlay system
- [ ] Theme switching during game (if safe)
- [ ] Handle theme changes in control panel
- [ ] Update existing background settings to use theme system
- [ ] Migration path for existing background configurations
- [ ] Performance testing under load
- [ ] Memory leak checks (SVG rendering)

### Phase 6: Preset Themes & Assets (Week 7)

#### Tasks
- [ ] Design 6 built-in preset themes
- [ ] Create preset background assets
- [ ] Configure each preset with appropriate settings
- [ ] Generate preview images for each preset
- [ ] Insert preset data into database
- [ ] Create user documentation for each preset
- [ ] Test preset application
- [ ] Ensure consistency across all components

### Phase 7: Testing & Polish (Week 8)

#### Tasks
- [ ] Unit tests for all theme components
- [ ] Integration tests for theme application
- [ ] UI/UX testing
  - Theme switching responsiveness
  - Preview accuracy
  - Save/Load reliability
  - Pack import/export functionality
- [ ] Performance testing
  - SVG rendering speed
  - Memory usage with multiple themes
  - Theme switching latency
  - Large pack import performance
- [ ] Edge case testing
  - Invalid colors
  - Missing assets
  - Corrupt theme files
  - Corrupt pack files
  - Large SVG complexity
  - Pack version mismatches
  - Duplicate theme names in packs
  - Pack size limits
- [ ] Cross-component consistency testing
- [ ] User acceptance testing
- [ ] Bug fixes and refinements
- [ ] Documentation updates

## Technical Considerations

### SVG Rendering Performance

**Challenge:** Real-time SVG rendering with effects can be CPU-intensive.

**Solutions:**
- Cache rendered SVGs with hashed keys (settings hash)
- Use async rendering with progress indication
- Limit preview updates (debounce at 300ms)
- Pre-render preset themes at application startup
- Use GPU acceleration where available (WPF: RenderOptions.ProcessRenderMode)

### Effect Implementation

#### Silk Sheen Effect
```csharp
public class SilkSheenEffect : IStrapEffect
{
    public void Apply(XElement svgElement, EffectSettings settings)
    {
        var gradient = new XElement("linearGradient",
            new XAttribute("id", "silkSheen"),
            new XAttribute("x1", "0%"), new XAttribute("y1", "0%"),
            new XAttribute("x2", "100%"), new XAttribute("y2", "100%"),
            new XElement("stop",
                new XAttribute("offset", "0%"),
                new XAttribute("stop-color", "#ffffff"),
                new XAttribute("stop-opacity", settings.Intensity * 0.005)),
            new XElement("stop",
                new XAttribute("offset", "45%"),
                new XAttribute("stop-color", settings.EffectColor ?? "#ffffff"),
                new XAttribute("stop-opacity", settings.Intensity * 0.008)),
            new XElement("stop",
                new XAttribute("offset", "55%"),
                new XAttribute("stop-color", settings.EffectColor ?? "#ffffff"),
                new XAttribute("stop-opacity", settings.Intensity * 0.002)),
            new XElement("stop",
                new XAttribute("offset", "100%"),
                new XAttribute("stop-color", "#ffffff"),
                new XAttribute("stop-opacity", settings.Intensity * 0.005))
        );
        
        var defs = svgElement.Element("defs") ?? new XElement("defs");
        defs.Add(gradient);
        
        // Apply overlay
        var overlay = new XElement("rect",
            new XAttribute("width", "100%"),
            new XAttribute("height", "100%"),
            new XAttribute("fill", "url(#silkSheen)")
        );
        svgElement.Add(overlay);
    }
}
```

### Color Management

**System:** Use ARGB color model with HEX string representation in database

**Conversion:**
```csharp
public static class ColorConverter
{
    public static string ToHex(Color color) 
        => $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    
    public static Color FromHex(string hex)
        => ColorTranslator.FromHtml(hex);
}
```

### Theme Export/Import Format

#### Single Theme Export
**Format:** Database export only (no file format needed)
- Themes exported as database records
- Can be saved to user profiles
- No asset files included

#### Theme Pack Format
**Format:** ZIP file with XML configuration + assets (`.zip` file)

**pack.xml Structure (Similar to Soundpack XML):**
```xml
<?xml version="1.0" encoding="utf-8"?>
<ThemePack>
  <Metadata>
    <Name>Elegant Professional Pack</Name>
    <Description>A collection of elegant themes with custom icons</Description>
    <Author>Theme Designer</Author>
    <Version>1.0.0</Version>
    <CreatedDate>2026-01-12</CreatedDate>
  </Metadata>
  
  <Assets>
    <Backgrounds>
      <Background id="main_bg" path="backgrounds/elegant_main.png" />
      <Background id="money_tree_bg" path="backgrounds/money_tree.png" />
    </Backgrounds>
    <Icons>
      <Icon id="lifeline_5050" path="icons/50_50_custom.png" />
      <Icon id="lifeline_phone" path="icons/phone_friend_custom.png" />
      <Icon id="lifeline_audience" path="icons/ask_audience_custom.png" />
    </Icons>
    <Fonts>
      <Font id="custom_font" path="fonts/ElegantFont.ttf" name="Elegant Display" />
    </Fonts>
  </Assets>
  
  <Themes>
    <Theme name="Elegant Gold">
      <Description>Gold and brown elegant theme</Description>
      <Background>
        <TVScreen backgroundId="main_bg" chromaKey="false" />
        <MoneyTree backgroundId="money_tree_bg" />
      </Background>
      <Straps>
        <Strap type="Question" shape="Classic" primaryColor="#8B4513" secondaryColor="#D4AF37" 
               gradient="true" gradientAngle="90" effect="SilkSheen" effectIntensity="60" />
        <Strap type="Answer" shape="Classic" primaryColor="#8B4513" secondaryColor="#D4AF37" 
               gradient="true" gradientAngle="90" effect="SilkSheen" effectIntensity="60" />
      </Straps>
      <MoneyTree>
        <Colors inactive="#808080" active="#FFD700" completed="#00FF00" safeHaven="#0080FF" />
        <Highlight enabled="true" type="PulsingGlow" color="#FFFF00" intensity="80" />
      </MoneyTree>
      <Lifelines>
        <Icon type="FiftyFifty" assetId="lifeline_5050" />
        <Icon type="PhoneFriend" assetId="lifeline_phone" />
        <Icon type="AskAudience" assetId="lifeline_audience" />
      </Lifelines>
    </Theme>
  </Themes>
</ThemePack>
```

**Legacy Single Theme Format (For Backward Compatibility):**
**Format:** JSON with metadata (`.mtheme` file) - NO ASSETS

```json
{
  "themeVersion": "1.0",
  "themeName": "My Custom Theme",
  "author": "User",
  "exportDate": "2026-01-12T10:30:00Z",
  "background": { /* settings - references existing assets only */ },
  "straps": [ /* strap configurations */ ],
  "moneyTree": { /* money tree settings */ }
}
```
**Note:** Single themes reference existing assets, do not include files

---

#### Theme Pack ZIP Structure

**Installed Location:**
```
lib/themepacks/
├── ElegantPack/
│   ├── pack.xml
│   ├── backgrounds/
│   ├── icons/
│   ├── fonts/
│   └── preview.png
├── ModernPack/
│   ├── pack.xml
│   ├── backgrounds/
│   └── ...
```

**File Extensions:**
- `.zip` - Theme pack file (exported/distributed)
- No extension needed for single themes (database only)

**Size Considerations:**
- Pack files include actual image/font files
- Recommended background images: ≤2MB each
- Icon images: ≤100KB each
- Preview image: ≤500KB
- Maximum pack size: 50MB (includes all assets)
- ZIP compression reduces distribution size

**Asset Guidelines:**
- Backgrounds: PNG/JPG, 1920x1080 recommended
- Icons: PNG with transparency, 256x256 or 512x512
- Fonts: TTF/OTF format
- Preview: PNG/JPG, 800x600 recommended

## Migration Strategy

### Existing Background Settings

**Plan:** Migrate existing background configurations to new theme system

```csharp
public async Task MigrateExistingBackgroundsAsync()
{
    var existingBackground = await GetCurrentBackgroundSettingsAsync();
    
    var defaultTheme = new Theme
    {
        ThemeName = "Migrated Settings",
        ThemeType = ThemeType.Custom,
        Background = new ThemeBackground
        {
            ImagePath = existingBackground.ImagePath,
            ChromaKeyEnabled = existingBackground.ChromaKeyEnabled,
            ChromaKeyColor = existingBackground.ChromaKeyColor,
            // ... map all existing settings
        }
    };
    
    await _themeRepository.SaveThemeAsync(defaultTheme);
    await _themeRepository.SetActiveThemeAsync(defaultTheme.ThemeId);
}
```

## User Documentation

### Required Documentation
- [ ] Theme system user guide
- [ ] Preset theme descriptions with screenshots
- [ ] Custom theme creation tutorial
- [ ] SVG strap customization guide
- [ ] Effect reference (what each effect does)
- [ ] Export/Import instructions
  - [ ] Theme pack creation guide (ZIP structure)
  - [ ] pack.xml specification
  - [ ] Pack import guide
  - [ ] Asset preparation guidelines
  - [ ] Pack distribution best practices
- [ ] Best practices for theme design
- [ ] Theme pack creation guidelines
  - [ ] Asset format requirements
  - [ ] File size recommendations
  - [ ] XML structure documentation
  - [ ] Example pack walkthrough
- [ ] Troubleshooting common issues

## Success Criteria

### Functional Requirements
- [ ] All 6 built-in presets working correctly
- [ ] 2 user profiles can be saved and loaded
- [ ] All component settings apply correctly
- [ ] SVG straps render with all effects
- [ ] Themes persist across application restarts
- [ ] Export/Import works reliably

### Performance Requirements
- [ ] Theme switching < 500ms
- [ ] SVG rendering < 100ms per strap
- [ ] Preview updates < 300ms after settings change
- [ ] No memory leaks during extended use
- [ ] Smooth UI responsiveness

### Quality Requirements
- [ ] Zero data loss on theme save
- [ ] Graceful handling of missing assets
- [ ] Validation prevents invalid configurations
- [ ] Database transactions for theme operations
- [ ] Comprehensive error logging

## Timeline

| Phase | Duration | Deliverable |
|-------|----------|-------------|
| Phase 1: Database & Repository | 1 week | Theme data layer complete |
| Phase 2: Core Services & Models | 1 week | Business logic functional |
| Phase 3: SVG Strap System | 1 week | Strap rendering working |
| Phase 4: UI Components | 2 weeks | Complete Theme tab UI |
| Phase 5: Integration | 1 week | Theme system integrated |
| Phase 6: Preset Themes & Assets | 1 week | 6 presets ready |
| Phase 7: Testing & Polish | 1 week | Release-ready |
| **TOTAL** | **8 weeks** | v1.0.7 released |

## Future Enhancements (Post-v1.0.7)

- [ ] Additional preset themes (community contributions)
- [ ] Online theme pack repository/marketplace
- [ ] Theme pack ratings and reviews
- [ ] Automated pack updates
- [ ] Pack dependencies (required base packs)
- [ ] Animated strap transitions
- [ ] Advanced SVG shape editor
- [ ] Video background support for straps
- [ ] Theme templates (partially configured themes)
- [ ] Color palette suggestions (AI-driven)
- [ ] Theme preview in full-screen mode
- [ ] Per-question theme switching
- [ ] Season/holiday themed packs
- [ ] Theme pack statistics (download counts, popularity)
- [ ] Pack creation wizard

## Dependencies

### Required Libraries
- System.Drawing (Windows Forms graphics)
- System.Xml.Linq (SVG manipulation + pack.xml parsing)
- System.IO.Compression (ZIP pack extraction/creation)
- System.IO.Compression.ZipFile (ZIP file operations)
- Microsoft.Data.SqlClient (database)
- System.Text.Json (optional: for legacy .mtheme format)

### External Assets
- Default background images (must be created)
- Default lifeline icons (existing in application)
- Preview thumbnails for built-in presets
- SVG shape templates
- Sample theme pack for testing

**Theme Pack Assets (User-Provided):**
- Custom background images
- Custom lifeline icons
- Custom fonts (optional)
- Preview images for pack

## Risk Assessment

| Risk | Impact | Likelihood | Mitigation |
|------|--------|------------|------------|
| SVG rendering performance issues | High | Medium | Implement caching, async rendering |
| Complex UI overwhelming users | Medium | Low | Provide presets, hide advanced options by default |
| Theme corruption/data loss | High | Low | Database transactions, validation, backups |
| Asset file management complexity | Medium | Medium | Database storage of settings, file paths validated |
| Memory leaks from SVG generation | High | Medium | Proper disposal, memory profiling, limits |
| User profile confusion | Low | Low | Clear UI labels, tooltips, documentation |

## Notes

- **Theme Storage:** Theme configurations stored in SQL Server database
- **Pack Format:** Theme PACKS use XML (like soundpacks) + ZIP for asset bundling
- **Consistency:** Theme pack structure mirrors soundpack structure for user familiarity
- **Database-First:** Use repository pattern for all data access
- **NO MessageBox:** Use GameConsole for logging, non-blocking UI notifications for user feedback
- **Async Operations:** All theme loading/saving must be async
- **Preview Performance:** Critical for user experience - must be smooth
- **Effect Library:** Start with 6 core effects, expand in future versions
- **User Profiles:** Limited to 2 to keep UI simple and focused
- **Preset Quality:** Built-in presets must be professional and polished

## Open Questions

1. **Q:** Should theme changes be allowed during an active game?  
   **A:** TBD - May require game state checks for safety

2. **Q:** Maximum SVG complexity limits?  
   **A:** TBD - Performance testing will determine

3. **Q:** Should fonts be embedded or system-dependent?  
   **A:** TBD - Consider license implications

4. **Q:** Real-time preview in full broadcast resolution?  
   **A:** TBD - May need scaled preview for performance

5. **Q:** Theme versioning for future compatibility?  
   **A:** Yes - Include version in theme metadata

## References

- SVG Specification: https://www.w3.org/TR/SVG2/
- Windows Forms Graphics: https://docs.microsoft.com/en-us/dotnet/desktop/winforms/advanced/graphics
- Color Theory for UI: (Reference design resources)
- Current background implementation: [ControlPanelForm.cs, Background section]
