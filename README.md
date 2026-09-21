<p align="center">
  <img src="HyperMenu.jpeg" alt="HyperMenu" width="720">
</p>

# HyperMenu 4.2.3

> **Fixed and maintained by boranseason** 🛠️

An unofficial community-maintained HyperMenu build for **Among Us 2026.8.18**. This release focuses on current-game compatibility, a cleaner workflow, and practical quality-of-life improvements.

## Highlights

- 🎚️ **Visible lobby level up to 700**
- ⚖️ Updated compatibility work for the newer Judge and meeting APIs
- 🧭 Flat tab navigation, search, favorites, and a Quick page
- ⌨️ Improved hotkey capture with duplicate-key warnings
- 💾 Named profiles, saved window layout, and configurable startup behavior
- 🛡️ Private Lobby Safety is enabled by default for network-sensitive actions

## Level 700 Update

The visible lobby-level control now supports values from **1 to 700**.

- Manual **Send Level Update** supports level 700
- Optional automatic sending after joining a lobby supports level 700
- ✅ **Tested in-game: level 700 displays correctly**

> This changes the level advertised during the current lobby/session. It does **not** grant permanent XP or modify account progression.

## Compatibility

| Component | Supported version |
| --- | --- |
| Among Us | `2026.8.18` |
| HyperMenu | `4.2.3` |
| Runtime | BepInEx IL2CPP |

## Installation

### Steam / Itch

1. Install the required **BepInEx IL2CPP** setup for Among Us.
2. Close Among Us completely.
3. Extract the Steam / Itch release ZIP.
4. Copy `HyperMenu.dll` to `Among Us/BepInEx/plugins/`.
5. Start the game and press `Delete` to open HyperMenu.

### Epic Games

1. Install the required **BepInEx IL2CPP** setup for your Epic Games installation.
2. Close Among Us completely.
3. Extract the Epic release ZIP.
4. Copy `HyperMenu.dll` to `Among Us/BepInEx/plugins/`.
5. Start the game and press `Delete` to open HyperMenu.

> Do not keep multiple HyperMenu DLL versions in the `plugins` folder. Replace the existing `HyperMenu.dll` rather than installing a second copy beside it.

## Notable Improvements

- **Navigation:** tab search, favorites, Quick access, a classic flat tab list, and scrollable long pages.
- **Hotkeys:** searchable actions, direct key capture, safe cancel/remove controls, duplicate-binding warnings, and hotkeys disabled while typing.
- **Profiles:** save and load named profiles from `BepInEx/config/HyperMenuProfiles`, with legacy profile compatibility.
- **Meeting tools:** an informational Meeting Inspector for reading the current meeting state without changing results.

## Verification

- ✅ Release build completed successfully
- ✅ BepInEx loaded HyperMenu 4.2.3 successfully
- ✅ Menu/UI startup verified
- ✅ Level 700 tested and displayed correctly in-game

## Build From Source

```bash
dotnet restore MalumMenu.sln
dotnet build MalumMenu.sln -c Release --no-restore
```

## Credits

HyperMenu and MalumMenu are the work of their respective original authors and contributors.

**Compatibility fixes, updates, and quality-of-life work by boranseason.**

## Disclaimer

HyperMenu is an unofficial community modification and is not affiliated with Innersloth. Among Us and related trademarks belong to their respective owners. Use modifications responsibly and only in sessions where their use is understood by everyone involved.
