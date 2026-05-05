# HydraMenu Integration - Visual Overview

## Menu Structure After Integration

```
MalumMenu (Main UI)
├── Movement Tab
├── ESP Tab
├── Roles Tab (Original MalumMenu)
├── Ship Tab
├── Chat Tab
├── Animations Tab
├── Overload Tab
├── Console Tab
├── HostOnly Tab
├── Passive Tab
├── Anticheat Tab
├── Modes Tab
│
├── ★ HYDRA TAB ★ (NEW - Integrated HydraMenu Features)
│   ├── Troll Sub-Tab
│   │   ├── Auto Report Bodies
│   │   ├── Auto Trigger Spores
│   │   ├── Block Sabotages
│   │   ├── Disable Vents
│   │   ├── Fuck Start Timer (Button)
│   │   ├── Trigger All Spores (Button)
│   │   ├── Copy Random Player (Button)
│   │   └── Door Troller Settings
│   │
│   ├── Visuals Sub-Tab
│   │   ├── Skip Shhh Animation
│   │   ├── Accurate Disconnection Reasons
│   │   ├── Fullbright
│   │   ├── Show Guardian Angel Protections
│   │   ├── Always Visible Chat
│   │   ├── Show Ghosts
│   │   └── Show Messages By Ghosts
│   │
│   ├── Self Sub-Tab
│   │   ├── Update Stats in Freeplay
│   │   ├── Always Show Task Animations
│   │   ├── No Ladder Cooldown
│   │   ├── Unlimited Meetings
│   │   ├── Call Meeting (Button)
│   │   └── Speed Modifier (Slider)
│   │
│   ├── Host Sub-Tab
│   │   ├── Ban Mid-Game
│   │   ├── Use Flipped Skeld Map
│   │   ├── Disable Meetings
│   │   ├── Disable Sabotages
│   │   ├── Disable Close Doors
│   │   ├── Disable Security Cameras
│   │   ├── Disable Game End
│   │   ├── Block Low-Level Players (with slider)
│   │   ├── Spam Report Bodies
│   │   ├── Force Start Game (Button)
│   │   ├── Kill Everyone (Button)
│   │   ├── Force Crewmate Victory (Button)
│   │   └── Force Imposter Victory (Button)
│   │
│   ├── Movement Sub-Tab
│   │   ├── Current Map Display
│   │   ├── Current Position Display
│   │   ├── Noclip Toggle
│   │   ├── Speed Modifier (Slider)
│   │   ├── Use SnapTo RPC For Teleports
│   │   └── Teleport Location Buttons (Dynamic per map)
│   │
│   ├── Roles Sub-Tab
│   │   ├── Vent As Crewmate
│   │   ├── Move In Vents
│   │   ├── Sabotage As Crewmate
│   │   ├── Allow Sabotaging In Vents As Imposter
│   │   ├── No Kill Cooldown
│   │   └── Disable Shapeshift Animation
│   │
│   ├── Protect Sub-Tab
│   │   ├── Force Enable DTLS
│   │   ├── Block Server Teleports
│   │   ├── Hardened Packed Int Deserializer
│   │   ├── Protect Against Invalid Vent Overload
│   │   ├── Protect Against Invalid Ladder Overload
│   │   ├── Enable Version Spoofing
│   │   └── Use Modded Protocol
│   │
│   └── Sabotage Sub-Tab
│       ├── Update Sabotage Systems Directly
│       ├── Sabotage All (Button)
│       ├── Close All Doors (Button)
│       ├── Fix All Sabotages (Button)
│       ├── Unlock All Doors (Button)
│       ├── Individual Sabotage Buttons (Map-specific)
│       └── Individual Door Control Buttons (Map-specific)
│
├── Config Tab
└── Settings Tab
```

## Component Architecture

```
MalumMenu Plugin
├── UI Components
│   ├── MenuUI (Main Window)
│   ├── HydraFeaturesTab ← NEW
│   ├── Other UI Windows (ConsoleUI, RolesUI, etc.)
│   └── Notification Manager ← From HydraMenu
│
├── Feature Components
│   ├── HydraMenu.features.Troll ← Patched
│   ├── HydraMenu.features.Self ← Patched
│   ├── HydraMenu.features.Visuals ← Patched
│   ├── HydraMenu.features.Host ← Patched
│   ├── HydraMenu.features.Roles ← Patched + MonoBehaviour
│   ├── HydraMenu.features.Protections ← Patched
│   ├── HydraMenu.features.Chat ← Patched
│   ├── HydraMenu.features.Spoofer ← Patched
│   └── HydraMenu.features.PlayerLogger ← Patched
│
├── Routine Components
│   └── HydraMenu.routines.RoutineManager ← NEW Component
│       ├── AutoTriggerSporesRoutine
│       ├── DoorTrollerRoutine
│       ├── PlayerFollowerRoutine
│       ├── ReportBodySpamRoutine
│       └── DiscoHostRoutine
│
└── Harmony Patches
    └── Applied to entire HydraMenu.features Assembly
```

## Data Flow

```
User Input (Button/Toggle in HydraFeaturesTab)
    ↓
Feature Class (HydraMenu.features.*)
    ↓
Harmony Patch (Applied during Load)
    ↓
Among Us Game Code (Modified Behavior)
    ↓
Result (Feature Activated/Deactivated)
    ↓
Notification (Optional - via NotificationManager)
    ↓
Display to User
```

## Integration Timeline

1. **Plugin Load (MalumMenu.Load())**
   - Apply MalumMenu Harmony patches
   - Apply HydraMenu Harmony patches
   - Initialize NotificationManager component
   - Initialize RoutineManager component
   - Initialize Roles MonoBehaviour component

2. **Menu Initialization (MenuUI.Start())**
   - Add all tabs including HydraFeaturesTab
   - Create 2D GUI window

3. **Runtime (MenuUI.Update() / MenuUI.OnGUI())**
   - Handle user input from HydraFeaturesTab
   - Update feature toggles and sliders
   - Execute button callbacks
   - Send notifications as needed

4. **Routine Management (RoutineManager.Update())**
   - Run enabled routines each frame
   - Update routine states

5. **Notification Display (NotificationManager.OnGUI())**
   - Render notifications in top-right corner
   - Handle notification lifecycle

## Key Integration Points

### 1. Harmony Patch Application
```csharp
// In MalumMenu.cs Load() method
Harmony.PatchAll();  // Apply MalumMenu patches
Harmony.PatchAll(typeof(HydraMenu.features.Troll).Assembly);  // Apply HydraMenu patches
```

### 2. Component Initialization
```csharp
// In MalumMenu.cs Load() method
notificationManager = AddComponent<NotificationManager>();
routineManager = AddComponent<RoutineManager>();
AddComponent<HydraMenu.features.Roles>();
```

### 3. UI Tab Addition
```csharp
// In MenuUI.cs Start() method
_tabs.Add(new HydraFeaturesTab());
```

### 4. Feature Access
```csharp
// In HydraFeaturesTab.cs
Troll.AutoReportBodies.Enabled = GUILayout.Toggle(...);
MalumMenu.routineManager.doorTroller.Enabled = GUILayout.Toggle(...);
MalumMenu.notificationManager.Send("Title", "Message", 5);
```

## Feature Availability

✅ All HydraMenu features are available
✅ All features integrated into single UI tab
✅ All Harmony patches automatically applied
✅ Notification system functional
✅ Routine management functional
✅ Role MonoBehaviour component active

## After Cleanup

Once the HydraMenu folder is deleted:
- All source remains in MalumMenu.cs
- All features continue to work
- No dependency on HydraMenu folder
- Clean project structure
