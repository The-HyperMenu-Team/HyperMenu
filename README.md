# ⚡ HyperMenu 4.4.0 — Fixed by boranseason

> 🛠️ An unofficial community-maintained HyperMenu build with updated Among Us compatibility, Judge / voting fixes, upstream 4.4.0 improvements and additional quality-of-life features.

<p align="center">
  <a href="https://github.com/boran223/HyperMenu-Fix/releases/latest">
    <b>📦 Download Latest Release</b>
  </a>
  &nbsp; • &nbsp;
  <a href="https://github.com/boran223/HyperMenu-Fix/releases/latest/download/HyperMenu.dll">
    <b>⚡ Download DLL</b>
  </a>
</p>

---

## 📌 Current Version

**HyperMenu 4.4.0**

In-game version text:

```text
HyperMenu 4.4.0 - Fixed by boranseason
```

The version watermark is displayed as a small bottom-left label, matching the modern HyperMenu style.

---

## 🚀 What's New in 4.4.0?

HyperMenu 4.4.0 merges newer upstream HyperMenu improvements into the boranseason build while preserving the custom fixes and features added in previous versions.

### 🎨 Menu Scaling

New menu customization options include:

- ↔️ Menu width multiplier
- ↕️ Menu height multiplier
- 🔍 Overall menu scale
- 🔤 Independent text scaling

This makes the interface much more usable across different resolutions and DPI settings.

Existing resize and saved-position functionality has been kept compatible with the new scaling system.

---

## 🖱️ Click-Through

Added the newer `AllowClicksThrough` behavior.

When enabled, the game interface behind the menu can remain interactable where appropriate.

Input handling has been integrated with:

- Menu dragging
- Resizing
- Voting
- Chat
- Text fields
- Hotkey capture

---

## 🔄 Match Info Improvements

The Match Info interface now refreshes its player information when reopened.

This helps prevent outdated information from remaining visible after:

- Role changes
- Player state changes
- Name changes
- Other runtime updates

The Chat / Match Info button overlap fix from newer HyperMenu code has also been integrated.

---

## 📜 New Console Logging

Additional debugging and information options have been added:

- 📋 Task logging
- 🎮 Game-state logging

These can report useful information about task completion and game start/end state inside the console.

---

## 📜 Improved Scrolling

The menu now uses the newer centralized scroll behavior where appropriate.

This reduces duplicated per-tab scrolling logic and keeps long feature pages easier to navigate.

The classic flat HyperMenu sidebar layout is still preserved.

---

## ⚖️ Judge Improvements

Judge support has been updated further.

Changes include:

- 👨‍⚖️ Judge support in compatible Force Role workflows
- 🎩 Judge cosmetic / outfit support where available
- ⚖️ Judge overrule handling
- 🔢 `overruleNonce` support
- 🗳️ Updated voting logic
- 🔍 Meeting Inspector Judge information

The Judge / meeting compatibility fixes introduced in the previous boranseason build remain intact.

---

## 🗳️ Meeting / Voting Fix

Older HyperMenu versions used meeting APIs that became incompatible after the Judge update.

The fixed build includes newer voting APIs such as:

- `MeetingHud.MeetingStates`
- `PlayerId`
- `VotedForId`
- Updated `RpcVotingComplete`
- Judge overrule processing
- Updated eject handling

✅ Normal voting

✅ Skip voting

✅ Meeting completion

✅ Judge voting logic

---

## ⭐ Quick & Favorites

Frequently used features can be marked with ⭐ and accessed from the Quick page.

Features include:

- ⭐ Favorites
- ⚡ Quick access
- 🔎 Feature search
- 🔢 Active feature counter
- ♻️ Reset Active Features

---

## ⌨️ Hotkey System

The improved hotkey manager includes:

- 🔎 Feature search
- ⌨️ Direct key capture
- `Escape` to cancel
- `Backspace` / `Delete` to clear a binding
- ⚠️ Duplicate binding warnings
- 🌐 Network-action indicators
- ⭐ Favorite integration

Hotkeys are automatically blocked while typing in chat or text fields.

---

## 💾 Profiles

Profiles are stored in:

```text
BepInEx/config/HyperMenuProfiles
```

Supported features:

- ✏️ Named profiles
- 💾 Save
- 📥 Load
- 📃 Profile list
- ✅ Active profile
- 🗑️ Two-step deletion
- 🚀 Automatic loading on startup
- 🔄 Legacy `MalumProfile.txt` support

---

## 🔍 Meeting Inspector

The read-only Meeting Inspector displays:

- 📊 Meeting state
- 🗳️ Local vote
- 👥 Player voting state
- 🎯 Vote targets
- ⏭️ Skip count
- ⏳ Players waiting to vote
- ⚖️ Judge overrule state
- 👨‍⚖️ Winning Judge
- 🎯 Overruled player
- 🔢 `overruleNonce`

The inspector does not send voting RPCs or modify meeting results.

---

## 🛡️ Private Lobby Safety

Private Lobby Safety helps prevent accidental execution of network-sensitive actions.

Safety checks can apply to actions such as:

- Meeting actions
- Role operations
- Eject
- Kill-related actions
- Sabotages
- Door controls
- Force Start
- Other network-sensitive commands

The same checks also apply when features are triggered through hotkeys.

---

## 📈 Automatic Level Update

The configured session-visible player level can be automatically refreshed while inside a lobby.

- ✅ Automatic update
- 🔁 Updates again after changing the configured level
- 🚫 No continuous RPC spam

> ⚠️ This does not grant account XP or permanently change account progression.

---

# 📥 Downloads

## ⚡ Latest Release

### 🔌 HyperMenu.dll

[**Download HyperMenu.dll**](https://github.com/boran223/HyperMenu-Fix/releases/latest/download/HyperMenu.dll)

### 📦 Full Steam / Itch Package

[**Download HyperMenu Steam / Itch Package**](https://github.com/boran223/HyperMenu-Fix/releases/latest/download/HyperMenu-Steam-Itch.zip)

### 🧑‍💻 Source Code

[**Download Source ZIP**](https://github.com/boran223/HyperMenu-Fix/releases/latest/download/HyperMenu-Source.zip)

### 🎁 All Release Files

[**Open Latest GitHub Release**](https://github.com/boran223/HyperMenu-Fix/releases/latest)

---

## 📥 Installation

1. Close Among Us.
2. Install the required BepInEx / IL2CPP setup.
3. Copy:

```text
HyperMenu.dll
```

into:

```text
Among Us/BepInEx/plugins/
```

4. Remove older HyperMenu DLLs.
5. Start Among Us.
6. Press `Delete` to open HyperMenu.

> ⚠️ Only keep one HyperMenu DLL inside the plugins folder.

---

## 🧱 Building From Source

Restore:

```bash
dotnet restore MalumMenu.sln
```

Build:

```bash
dotnet build MalumMenu.sln -c Release --no-restore
```

---

## ✅ Verification

HyperMenu 4.4.0 was validated with:

- ✅ Release build
- ✅ 0 build errors
- ✅ BepInEx startup
- ✅ Menu startup
- ✅ Updated 4.4.0 functionality
- ✅ Judge / voting compatibility
- ✅ Quick / Favorites
- ✅ Profiles
- ✅ Hotkeys
- ✅ Meeting Inspector
- ✅ Automatic Level Update
- ✅ Menu scaling
- ✅ Updated scrolling
- ✅ Match Info refresh fixes

---

## 🧑‍💻 Fixed & Maintained By

### **boranseason** ⚡

Compatibility fixes, upstream integration, UI improvements and additional HyperMenu-specific features.

---

## ❤️ Credits

Thanks to the original **HyperMenu**, **MalumMenu** and all related contributors.

This fork builds on their work while adding compatibility fixes and additional functionality.

---

## ⚠️ Disclaimer

This is an unofficial community-maintained modification and is not affiliated with **Innersloth**.

Among Us and related trademarks belong to their respective owners.
