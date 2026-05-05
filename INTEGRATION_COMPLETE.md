# Post-Integration Checklist

## What Has Been Done

✅ **Integration Complete** - All HydraMenu features have been integrated into MalumMenu's codebase

### Files Modified:
1. ✅ `src/MalumMenu.cs` - Added HydraMenu component initialization and Harmony patch application
2. ✅ `src/UI/Windows/MenuUI.cs` - Added HydraFeaturesTab to the menu
3. ✅ `src/UI/Windows/Tabs/HydraFeaturesTab.cs` - Created comprehensive feature tab

### Features Integrated:
- ✅ Troll Features (Auto Report, Block Sabotages, Door Troller, etc.)
- ✅ Visual Features (Fullbright, Skip Animations, Show Ghosts, etc.)
- ✅ Self Features (Speed Modifier, Ladder Cooldown, Unlimited Meetings, etc.)
- ✅ Host Features (Disable Meetings/Sabotages, Force Victory, etc.)
- ✅ Movement Features (Noclip, Speed, Teleportation)
- ✅ Role Features (Vent as Crewmate, Kill Cooldown, etc.)
- ✅ Protection Features (DTLS, Server Teleport Blocking, Overload Protection)
- ✅ Sabotage Features (Sabotage/Close Doors, Fix Systems)

## What You Need To Do

### Step 1: Build the Solution
```powershell
# Build the solution (you mentioned you'll do this manually)
# Ensure there are no compilation errors
```

### Step 2: Test the Integration
1. **Launch the mod** in Among Us
2. **Open the menu** (Default: Delete key)
3. **Locate the "Hydra" tab** - It should appear between "ModesTab" and "ConfigTab"
4. **Test each sub-section**:
   - Troll: Test Auto Report Bodies, Door Troller
   - Visuals: Test Fullbright, Ghost visibility
   - Self: Test Speed modifier, Unlimited meetings
   - Host: Test Disable Meetings, Force Victory (if you're host)
   - Movement: Test Noclip, Teleportation
   - Roles: Test Vent as Crewmate
   - Protect: Test Network protections
   - Sabotage: Test Sabotage All, Close Doors

### Step 3: Verify Notifications Work
- The notification manager should display messages in the top-right corner
- Test by activating features that send notifications (e.g., "Trigger All Spores")

### Step 4: Check Harmony Patches
- All HydraMenu harmony patches should be applied automatically
- You can verify by checking if restricted features work (e.g., Vent as Crewmate should work in-game)

### Step 5: Cleanup (Optional)
Once everything works, you can delete the HydraMenu folder:
```
src/HydraMenu/
```

**⚠️ Only delete AFTER confirming all features work!**

## Troubleshooting

### Issue: "HydraFeaturesTab not found" compile error
**Solution**: Ensure the file `src/UI/Windows/Tabs/HydraFeaturesTab.cs` is in the project

### Issue: Notifications not showing
**Solution**: Check that `notificationManager` is properly initialized in `MalumMenu.cs`

### Issue: Features not working
**Solution**: Ensure Harmony patches were applied. Check that the line in MalumMenu.cs:
```csharp
Harmony.PatchAll(typeof(HydraMenu.features.Troll).Assembly);
```
is executed during Load()

### Issue: Compilation errors with HydraMenu types
**Solution**: Make sure all HydraMenu source files are still in the project before building

## Files That Can Be Deleted After Testing

Once you've verified everything works, these can be safely deleted:
- ✅ `src/HydraMenu/` (entire folder and all contents)

**Do NOT delete before confirming all features work!**

## Key Integration Points

1. **MalumMenu.cs**
   - `public static RoutineManager routineManager;` - Manages automated routines
   - `public static NotificationManager notificationManager;` - Displays notifications
   - `Harmony.PatchAll(typeof(HydraMenu.features.Troll).Assembly);` - Applies all patches

2. **MenuUI.cs**
   - `_tabs.Add(new HydraFeaturesTab());` - Adds the Hydra tab to the menu

3. **HydraFeaturesTab.cs**
   - 8 sub-sections with comprehensive feature controls
   - All references use `HydraMenu.routineManager`, `HydraMenu.notificationManager`, etc.

## Support for Additional Features

If you want to add more HydraMenu features later:
1. Add a new `DrawFeature()` method in HydraFeaturesTab.cs
2. Add the corresponding case in the switch statement
3. Add a new button to the SelectionGrid

Example:
```csharp
case X:
    DrawNewFeature();
    break;

private void DrawNewFeature()
{
    GUILayout.Label("New Feature", GUIStylePreset.TabSubtitle);
    // Add toggles and buttons here
}
```

## Final Notes

- All HydraMenu features are now part of the MalumMenu codebase
- The notification system provides user feedback
- All Harmony patches are automatically applied
- The UI follows MalumMenu's design patterns
- The integration is complete and ready for testing
