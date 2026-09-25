using System;
using HarmonyLib;

namespace MalumMenu;

[HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.Initialize))]
public static class NormalPlayerTask_Initialize
{
    private const int HandlingId = 30011;
    // Postfix patch of NormalPlayerTask.Initialize to create arrows for tasks that don't have them
    public static void Postfix(NormalPlayerTask __instance)
    {
        try
        {
            // Set up arrow target for Airship UploadData task separately
            if (__instance.TaskType == TaskTypes.UploadData && (MapNames)Utils.GetCurrentMapID() == MapNames.Airship)
            {
                if (__instance.taskStep == 0)
                {
                    var airshipTask = __instance.GetComponent<AirshipUploadTask>();
                    var consolePositions = airshipTask.FindValidConsolesPositions();

                    // AirshipUploadTask uses an Arrows[] array instead of the inherited Arrow field
                    for (var i = 0; i < consolePositions.Count && i < airshipTask.Arrows.Length; i++)
                    {
                        // There are two already existing arrows, we just need to set the target of one of them at step 0
                        airshipTask.Arrows[i].target = consolePositions[i];
                    }

                    airshipTask.LocationDirty = true;

                    return;
                }
            }

            ArrowHandler.EnsureArrowExists(__instance);

            if (!ArrowHandler.NeedsSpecialTarget(__instance))
            {
                __instance.UpdateArrowAndLocation();
            }
            else if (ArrowHandler.IsOwnedAndIncomplete(__instance))
            {
                ArrowHandler.SetArrowTargetForSpecialTasks(__instance);
            }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "NormalPlayerTask_Initialize.Postfix: ensure task arrow"); }
    }
}

[HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.FixedUpdate))]
public static class NormalPlayerTask_FixedUpdate
{
    private const int HandlingId = 30011;
    // Postfix patch of NormalPlayerTask.FixedUpdate to control arrow visibility
    public static void Postfix(NormalPlayerTask __instance)
    {
        try
        {
            if (__instance.Arrow == null) return;

            if (!CheatToggles.taskArrows)
            {
                // Hide arrows if taskStep == 0 (vanilla behavior)
                if (__instance.taskStep == 0)
                {
                    __instance.Arrow.gameObject.SetActive(false);
                }

                return;
            }

            if (ArrowHandler.IsOwnedAndIncomplete(__instance))
            {
                if (ArrowHandler.NeedsSpecialTarget(__instance))
                {
                    ArrowHandler.SetArrowTargetForSpecialTasks(__instance);
                }

                __instance.Arrow.gameObject.SetActive(true);
            }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "NormalPlayerTask_FixedUpdate.Postfix: control arrow visibility"); }
    }
}

[HarmonyPatch(typeof(AirshipUploadTask), nameof(AirshipUploadTask.FixedUpdate))]
public static class AirshipUploadTask_FixedUpdate_Patch
{
    private const int HandlingId = 30011;
    // Postfix patch of AirshipUploadTask.FixedUpdate to keep arrows visible when taskStep == 0 or Comms sabotage is active
    public static void Postfix(AirshipUploadTask __instance)
    {
        try
        {
            // This patch is unfortunately necessary because AirshipUploadTask overrides NormalPlayerTask.FixedUpdate
            if (__instance.Arrows == null) return;

            if (!CheatToggles.taskArrows)
            {
                // Deactivate all arrows if taskStep == 0 (vanilla behavior)
                if (__instance.taskStep != 0) return;

                foreach (var arrow in __instance.Arrows)
                {
                    arrow.gameObject.SetActive(false);
                }

                return;
            }

            var consolePositions = __instance.FindValidConsolesPositions();

            for (var i = 0; i < __instance.Arrows.Length; i++)
            {
                // Only activate arrows that correspond to valid console positions
                __instance.Arrows[i].gameObject.SetActive(i < consolePositions.Count && __instance.Owner != null && __instance.Owner.AmOwner);
            }
        }
        catch (Exception ex) { ErrorReporter.Report(ex, HandlingId, "AirshipUploadTask_FixedUpdate_Patch.Postfix: control airship arrows"); }
    }
}
