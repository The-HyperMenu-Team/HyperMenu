# Integration Verification Checklist

## Pre-Build Verification

### Files Created/Modified
- [ ] `src/UI/Windows/Tabs/HydraFeaturesTab.cs` - NEW file exists
- [ ] `src/MalumMenu.cs` - Contains HydraMenu imports and initialization
- [ ] `src/UI/Windows/MenuUI.cs` - HydraFeaturesTab added to tabs list

### Code Review
- [ ] MalumMenu.cs has: `using HydraMenu.routines;`
- [ ] MalumMenu.cs has: `using HydraMenu.ui;`
- [ ] MalumMenu.cs has: `public static RoutineManager routineManager;`
- [ ] MalumMenu.cs has: `public static NotificationManager notificationManager;`
- [ ] MalumMenu.cs has: `notificationManager = AddComponent<NotificationManager>();`
- [ ] MalumMenu.cs has: `routineManager = AddComponent<RoutineManager>();`
- [ ] MalumMenu.cs has: `AddComponent<HydraMenu.features.Roles>();`
- [ ] MalumMenu.cs has: `Harmony.PatchAll(typeof(HydraMenu.features.Troll).Assembly);`
- [ ] MenuUI.cs has: `_tabs.Add(new HydraFeaturesTab());`
- [ ] HydraFeaturesTab.cs imports are correct
- [ ] HydraFeaturesTab.cs has 8 draw methods (Troll, Visuals, Self, Host, Movement, Roles, Protections, Sabotage)

## Build Verification

### Build Status
- [ ] Build completes with no errors
- [ ] Build completes with no critical warnings
- [ ] All HydraMenu source files compile
- [ ] All new code compiles
- [ ] Output binary is generated

### Common Build Issues
- [ ] Check for namespace conflicts
- [ ] Verify all using statements are correct
- [ ] Ensure HydraMenu folder is present
- [ ] Confirm .csproj includes all files

## Runtime Verification

### Plugin Load
- [ ] Plugin loads without crashing
- [ ] No exceptions during Load() method
- [ ] Harmony patches applied successfully
- [ ] Components initialized
- [ ] No null reference errors

### Menu Initialization
- [ ] Menu opens (press Delete key)
- [ ] Menu displays correctly
- [ ] All original tabs still visible
- [ ] "Hydra" tab appears in tab list
- [ ] "Hydra" tab is clickable

### Hydra Tab Functionality
- [ ] Troll sub-tab loads
- [ ] Visuals sub-tab loads
- [ ] Self sub-tab loads
- [ ] Host sub-tab loads
- [ ] Movement sub-tab loads
- [ ] Roles sub-tab loads
- [ ] Protect sub-tab loads
- [ ] Sabotage sub-tab loads

### Feature Testing

#### Troll Features
- [ ] Auto Report Bodies toggle works
- [ ] Block Sabotages toggle works
- [ ] Door Troller slider works
- [ ] "Fuck Start Timer" button clickable
- [ ] "Trigger All Spores" button clickable
- [ ] "Copy Random Player" button clickable

#### Visual Features
- [ ] All visual toggles can be toggled
- [ ] No visual glitches when toggling

#### Self Features
- [ ] Speed slider works (0-5x)
- [ ] Call Meeting button clickable
- [ ] All toggles functional

#### Host Features (Test when you're host)
- [ ] Host toggles operational
- [ ] Force Victory buttons clickable
- [ ] Kill Everyone button clickable
- [ ] Level slider works

#### Movement Features
- [ ] Noclip toggle works
- [ ] Speed slider works
- [ ] Teleport buttons all present
- [ ] Teleport destinations correct for map

#### Role Features
- [ ] All role toggles operational
- [ ] No conflicts with existing role system

#### Protection Features
- [ ] All protection toggles operational
- [ ] No false positives on detections

#### Sabotage Features
- [ ] Sabotage/Close door buttons present
- [ ] Appropriate buttons for current map
- [ ] Buttons execute sabotage actions

### Notification System
- [ ] Notifications display when features trigger
- [ ] Notifications appear in top-right corner
- [ ] Notifications have appropriate duration
- [ ] Notification text is readable
- [ ] Multiple notifications queue properly

### Harmony Patches
- [ ] Restricted features work (Vent as Crewmate, etc.)
- [ ] Speed modifier affects movement
- [ ] Fullbright actually brightens the map
- [ ] Protected features are blocked from cheaters

## In-Game Testing

### Game Load
- [ ] Join a game/lobby
- [ ] Menu remains accessible
- [ ] Features don't cause immediate crash

### Feature Testing
- [ ] Speed modifier shows effect
- [ ] Noclip allows walking through walls
- [ ] Teleport changes position
- [ ] Fullbright affects lighting
- [ ] Role features work (if applicable)
- [ ] Host features work (if you're host)
- [ ] Sabotage features trigger sabotages

### Stability
- [ ] No crashes when using features
- [ ] No lag spikes from routines
- [ ] Game remains playable
- [ ] Anti-cheat doesn't detect false positives

## Post-Testing

### Documentation
- [ ] README_INTEGRATION.md created
- [ ] HYDRAMENU_INTEGRATION_SUMMARY.md created
- [ ] INTEGRATION_COMPLETE.md created
- [ ] INTEGRATION_ARCHITECTURE.md created
- [ ] This checklist created

### Cleanup (Only after ALL tests pass)
- [ ] Confirmed all features work
- [ ] Backed up project (optional but recommended)
- [ ] Ready to delete HydraMenu folder (if desired)

## Issue Tracking

If you encounter any issues, note them here:

### Issue #1
**Symptom**: 
**Location**: 
**Solution Attempted**: 
**Resolution**: 

### Issue #2
**Symptom**: 
**Location**: 
**Solution Attempted**: 
**Resolution**: 

## Sign-Off

- [ ] All checks passed
- [ ] Integration verified working
- [ ] Ready for production use
- [ ] HydraMenu folder can be deleted

**Integration Status**: ____________________

**Date Completed**: ____________________

**Tester Name**: ____________________

## Notes

Use this space for any additional observations or notes:

---

---

---

---

---
