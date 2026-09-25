using System.Collections.Generic;

namespace MalumMenu;

/// <summary>
/// Registry of 5-digit handling IDs, one per mod file.
/// Each file that reports errors declares its own
/// <c>private const int HandlingId = XXXXX;</c> using the ID assigned here.
/// IDs must be unique and in the range 10000-99999.
/// <c>00000</c> is reserved for unhandled/global errors (see ErrorReporter.GlobalHandlingId).
///
/// Ranges:
///   10000-10999  core (MalumMenu.cs, Utils, UI shell, Console)
///   20000-20999  Cheats/
///   30000-30999  Patches/
///   40000-40999  features/
///   50000-50999  anticheat/
///   60000-69999  routines/, Components/, Utilities/, misc (Teleporter, Sabotage, ...)
///
/// When adding a new file, pick the next free ID in its folder's range and add a row below.
/// </summary>
public static class HandlingIds
{
    private static readonly Dictionary<int, string> _registry = new()
    {
        // core
        { 10001, "MalumMenu.cs" },
        { 10002, "Utilities/Utils.cs" },
        { 10003, "UI/Windows/ConsoleUI.cs" },
        { 10004, "UI/Windows/MenuUI.cs" },
        { 10005, "UI/NotificationManager.cs" },
        { 10006, "UI/Windows/RolesUI.cs" },
        { 10007, "UI/Windows/ProtectUI.cs" },
        { 10008, "UI/Windows/OverloadUI.cs" },
        { 10009, "UI/Windows/DoorsUI.cs" },
        { 10010, "UI/Windows/TasksUI.cs" },
        { 10011, "UI/Windows/StreamerUI.cs" },
        // Cheats/
        { 20001, "Cheats/MalumRandomizer.cs" },
        { 20002, "Cheats/MalumCheats.cs" },
        { 20006, "Cheats/ArrowHandler.cs" },
        { 20007, "Cheats/DoorsHandler.cs" },
        { 20008, "Cheats/MalumESP.cs" },
        { 20009, "Cheats/MalumPPMCheats.cs" },
        { 20010, "Cheats/MalumSabotageCheats.cs" },
        { 20011, "Cheats/MalumSpoof.cs" },
        { 20012, "Cheats/MinimapHandler.cs" },
        { 20013, "Cheats/OffensiveNameKicker.cs" },
        { 20014, "Cheats/OverloadHandler.cs" },
        { 20015, "Cheats/TracersHandler.cs" },
        // Patches/ + Network.cs
        { 30001, "Patches/AmongUsClientPatches.cs" },
        { 30002, "Patches/AntiOverloadPatches.cs" },
        { 30003, "Patches/ChatControllerPatches.cs" },
        { 30004, "Patches/EOSManagerPatches.cs" },
        { 30005, "Patches/HudManagerPatches.cs" },
        { 30006, "Patches/LogicGameFlowPatches.cs" },
        { 30007, "Patches/LogicOptionsPatches.cs" },
        { 30008, "Patches/MapBehaviourPatches.cs" },
        { 30009, "Patches/MatchInfoGuidePatches.cs" },
        { 30010, "Patches/MeetingHudPatches.cs" },
        { 30011, "Patches/NormalPlayerTaskPatches.cs" },
        { 30012, "Patches/NumberOptionPatches.cs" },
        { 30013, "Patches/OtherPatches.cs" },
        { 30014, "Patches/PlayerControlPatches.cs" },
        { 30015, "Patches/PlayerPhysicsPatches.cs" },
        { 30016, "Patches/RoleBehaviourPatches.cs" },
        { 30017, "Patches/ShapeshifterMinigamePatches.cs" },
        { 30018, "Patches/ShipStatusPatches.cs" },
        { 30019, "Patches/TextBoxTMPPatches.cs" },
        { 30020, "Patches/VentPatches.cs" },
        { 30021, "Patches/VoteBanSystemPatches.cs" },
        { 30050, "Network.cs" },
        // features/
        { 40001, "features/Chat.cs" },
        { 40002, "features/Host.cs" },
        { 40003, "features/Immortality.cs" },
        { 40004, "features/PlayerLogger.cs" },
        { 40005, "features/Protections.cs" },
        { 40006, "features/Roles.cs" },
        { 40007, "features/Self.cs" },
        { 40008, "features/Spoofer.cs" },
        { 40009, "features/Troll.cs" },
        { 40010, "features/Visuals.cs" },
        // anticheat/
        { 50001, "anticheat/Anticheat.cs" },
        { 50002, "anticheat/GameDataCheck.cs" },
        { 50003, "anticheat/PlatformSpoofer.cs" },
        // UI tabs + routines + components
        { 60001, "UI/Windows/Tabs/MovementTab.cs" },
        { 60002, "UI/Windows/Tabs/AnticheatTab.cs" },
        { 60003, "UI/Windows/Tabs/AnimationsTab.cs" },
        { 60004, "UI/Windows/Tabs/ChatTab.cs" },
        { 60005, "UI/Windows/Tabs/ConfigTab.cs" },
        { 60006, "UI/Windows/Tabs/ConsoleTab.cs" },
        { 60007, "UI/Windows/Tabs/ESPTab.cs" },
        { 60008, "UI/Windows/Tabs/HostOnlyTab.cs" },
        { 60009, "UI/Windows/Tabs/HostOnlyTab2.cs" },
        { 60010, "UI/Windows/Tabs/ModesTab.cs" },
        { 60011, "UI/Windows/Tabs/OverloadTab.cs" },
        { 60012, "UI/Windows/Tabs/PassiveTab.cs" },
        { 60013, "UI/Windows/Tabs/PlayersTab.cs" },
        { 60014, "UI/Windows/Tabs/ProtectionsTab.cs" },
        { 60015, "UI/Windows/Tabs/RolesTab.cs" },
        { 60016, "UI/Windows/Tabs/SabotageTab.cs" },
        { 60017, "UI/Windows/Tabs/SelfTab.cs" },
        { 60018, "UI/Windows/Tabs/SettingsTab.cs" },
        { 60019, "UI/Windows/Tabs/ShipTab.cs" },
        { 60020, "UI/Windows/Tabs/TrollTab.cs" },
        { 60102, "routines/RoutineManager.cs" },
        { 60103, "routines/TeleportSpammer.cs" },
        { 60104, "routines/AutoTriggerSporesRoutine.cs" },
        { 60105, "routines/JailPlayerRoutine.cs" },
        { 60106, "routines/PetPlayer.cs" },
        { 60107, "routines/PlayerFollower.cs" },
        { 60108, "routines/FungleSporeTriggerRoutine.cs" },
        { 60109, "routines/DiscoHost.cs" },
        { 60110, "routines/DoorTroller.cs" },
        { 60111, "routines/ReportBodySpam.cs" },
        { 60201, "Components/KeybindListener.cs" },
    };

    /// <summary>Returns true if the ID is registered in the table above (or is the global 00000).</summary>
    public static bool IsValid(int id)
    {
        if (id == ErrorReporter.GlobalHandlingId) return true;
        lock (_registry) { return _registry.ContainsKey(id); }
    }

    /// <summary>Returns the file name registered for an ID, or null if unregistered.</summary>
    public static string FileFor(int id)
    {
        lock (_registry)
        {
            return _registry.TryGetValue(id, out var file) ? file : null;
        }
    }
}
