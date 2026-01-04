# Custom StreamDeckSharp DLLs with Module 6 Support

This folder contains custom-built DLLs from a fork of StreamDeckSharp that includes support for the Stream Deck Module 6 (USB PID 0x00B8).

## Why Custom DLLs?

The official StreamDeckSharp NuGet package (v6.1.0) does not support the Stream Deck Module 6, which was released in 2024. A pull request has been submitted to add this support:

**Fork:** https://github.com/jdelgado-dtlabs/StreamDeckSharp  
**Branch:** `add-streamdeck-module6-support`  
**PR:** (pending review)

## DLLs Included

- `StreamDeckSharp.dll` - Custom build with Module 6 support
- `OpenMacroBoard.SDK.dll` - Dependency
- `HidSharp.dll` - HID communication library

## Module 6 Implementation

Based on official Elgato HID documentation:
https://docs.elgato.com/streamdeck/hid/module-6

Key features:
- 80×80 pixel BMP images with 90° clockwise rotation
- Report ID 0x02, Command 0x01 for image uploads
- 1024-byte output reports
- Row-major button indexing with keyId+1 offset

## When to Remove

Once the upstream PR is merged and a new version of StreamDeckSharp is released on NuGet:

1. Remove this folder
2. Remove the `<Reference>` entries in `MillionaireGame.csproj`
3. Add back: `<PackageReference Include="StreamDeckSharp" Version="X.X.X" />`
4. Update the comment noting Module 6 support is now in the official package

## Building Custom DLLs

If you need to rebuild these DLLs:

```bash
cd path/to/StreamDeckSharp/src/StreamDeckSharp
dotnet build -c Release /p:CopyLocalLockFileAssemblies=true
```

DLLs will be in: `bin/Release/net8.0/`

Copy the three DLLs listed above to this folder.
