# HydraMenu Integration Summary

This document outlines the integration of HydraMenu features into MalumMenu.

## Overview
All HydraMenu features have been successfully integrated into MalumMenu's existing UI framework. Instead of running HydraMenu as a separate UI instance, all HydraMenu features are now accessible through a new "Hydra" tab in the main MalumMenu interface.

## Integration Approach
- **UI Integration**: Created a new comprehensive `HydraFeaturesTab` that consolidates all HydraMenu features
- **Component Integration**: Added HydraMenu's `RoutineManager` and `NotificationManager` components to MalumMenu
- **Harmony Patches**: All HydraMenu feature patches are automatically applied through the MalumMenu Harmony instance
- **Namespace Preservation**: Kept all HydraMenu code in its original namespace to avoid conflicts

## Features Integrated

### 1. **Troll Features** (HydraFeaturesTab > Troll)
- Auto Report Bodies
- Auto Trigger Spores
- Block Sabotages
- Disable Vents
- Fuck Start Timer
- Trigger All Spores (Fungle only)
- Copy Random Player
- Door Troller (Automated door locking/unlocking)

### 2. **Visual Features** (HydraFeaturesTab > Visuals)
- Skip Shhh Animation
- Accurate Disconnection Reasons
- Fullbright
- Show Guardian Angel Protections
- Always Visible Chat
- Show Ghosts
- Show Messages By Ghosts

### 3. **Self Features** (HydraFeaturesTab > Self)
- Update Stats in Freeplay
- Always Show Task Animations
- No Ladder Cooldown
- Unlimited Meetings
- Call Meeting
- Speed Modifier (0x - 5x)

### 4. **Host Features** (HydraFeaturesTab > Host)
- Ban Mid-Game
- Use Flipped Skeld Map
- Disable Meetings
- Disable Sabotages
- Disable Close Doors
- Disable Security Cameras
- Disable Game End
- Block Low-Level Players (with customizable level threshold)
- Spam Report Bodies
- Force Start Game
- Kill Everyone
- Force Crewmate Victory
- Force Imposter Victory

### 5. **Movement Features** (HydraFeaturesTab > Movement)
- Position Display (Current map and coordinates)
- Noclip Toggle
- Speed Modifier (0x - 5x)
- Teleportation with SnapTo RPC option
- Pre-configured teleport locations for all maps:
  - **Skeld**: Cafeteria, Weapons, Medbay, Admin, Oxygen, Navigation, Shields, Communications, Storage, Electrical, Upper/Lower Engine, Security, Reactor
  - **Mira**: Launchpad, Medbay, Communications, Locker Room, Decontamination, Laboratory, Reactor, Office, Admin, Greenhouse, Cafeteria, Storage, Weapons
  - **Polus**: Dropship, Storage, Electrical, Security, Oxygen, Boiler Room, Communications, Weapons, Office, Admin, Laboratory, Specimen

### 6. **Role Features** (HydraFeaturesTab > Roles)
- Vent As Crewmate
- Move In Vents
- Sabotage As Crewmate
- Allow Sabotaging In Vents As Imposter
- No Kill Cooldown
- Disable Shapeshift Animation

### 7. **Protection Features** (HydraFeaturesTab > Protect)
- Force Enable DTLS (Network encryption)
- Block Server Teleports
- Hardened Packed Int Deserializer
- Protect Against Invalid Vent Overload
- Protect Against Invalid Ladder Overload
- Version Spoofing
- Modded Protocol Toggle

### 8. **Sabotage Features** (HydraFeaturesTab > Sabotage)
- Update Sabotage Systems Directly toggle
- Sabotage All
- Close All Doors
- Fix All Sabotages
- Unlock All Doors
- Individual Sabotage buttons (per map):
  - **Skeld**: Reactor, Oxygen, Lights, Communications
  - **Mira**: Reactor, Oxygen, Lights, Communications
  - **Polus**: Reactor, Lights, Communications
  - **Airship**: Reactor, Lights, Communications
  - **Fungle**: Reactor, Communications, Mushroom Mixup
- Individual Door Control buttons (per map)

## Files Modified

1. **src/MalumMenu.cs**
   - Added HydraMenu imports
   - Added RoutineManager and NotificationManager references
   - Added Roles MonoBehaviour component initialization
   - Applied Harmony patches for all HydraMenu features

2. **src/UI/Windows/MenuUI.cs**
   - Added HydraFeaturesTab to the tabs list

3. **src/UI/Windows/Tabs/HydraFeaturesTab.cs** (NEW)
   - Comprehensive tab with 8 sub-sections
   - Full integration of all HydraMenu features
   - Organized UI following MalumMenu's design patterns

## Component References

The following HydraMenu components are now integrated:
- `HydraMenu.routines.RoutineManager` - Manages automated routines (AutoTriggerSpores, DoorTroller, etc.)
- `HydraMenu.ui.NotificationManager` - Displays in-game notifications
- `HydraMenu.features.Roles` - Handles role modification logic

## Notification System

HydraMenu's notification system is fully functional and displays notifications in the top-right corner of the screen. Notifications are used for:
- Feature confirmations
- Error messages
- Important alerts

All notifications are sent through `MalumMenu.notificationManager.Send(title, message, duration)`

## Harmony Patches

All HydraMenu harmony patches are automatically applied when MalumMenu loads. This includes patches for:
- Troll features (Auto report, block vents, etc.)
- Visual features (Fullbright, ghost visibility, etc.)
- Self features (Speed modifier, ladder cooldown, etc.)
- Host features (Meeting/sabotage disabling, etc.)
- Role features (Vent as crewmate, kill cooldown, etc.)
- Protection features (DTLS, server teleport blocking, etc.)
- Spoofer features (Version and platform spoofing)

## UI Integration Details

The HydraFeaturesTab is organized as follows:
- **Tab Selection Grid**: 8 buttons for quick navigation (Troll, Visuals, Self, Host, Movement, Roles, Protect, Sabotage)
- **Dynamic Content**: Each sub-section displays relevant toggles and buttons
- **Consistent Styling**: Uses MalumMenu's GUIStylePreset for visual consistency
- **Responsive Layout**: Properly handles variable-width elements

## Testing Notes

When you build and test:
1. A new "Hydra" tab will appear in the main MalumMenu interface
2. All HydraMenu features will be accessible from this tab
3. The NotificationManager will display messages when features are activated
4. All Harmony patches will be automatically applied on plugin load

## Cleanup

The HydraMenu folder (`src/HydraMenu/`) can now be safely deleted as all functionality has been integrated into the MalumMenu codebase.

## Future Considerations

If you want to customize or extend these features:
- Feature toggles are controlled through HydraMenu.features classes
- Routines can be managed through MalumMenu.routineManager
- The Harmony patches follow HarmonyLib conventions and can be easily modified
- New features can be added to HydraFeaturesTab by creating new DrawFeature() methods
