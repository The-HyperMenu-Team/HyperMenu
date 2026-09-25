using System;
using HarmonyLib;
using MalumMenu.features;
using UnityEngine;

namespace MalumMenu.routines
{
    public class RoutineManager : MonoBehaviour
    {
        private const int HandlingId = 60102;

        public AutoTriggerSporesRoutine autoTriggerSpores = new AutoTriggerSporesRoutine();
        public DiscoHostRoutine discoHost = new DiscoHostRoutine();
        public DoorTrollerRoutine doorTroller = new DoorTrollerRoutine();
        public FungleSporeTriggerRoutine fungleSporeTrigger = new FungleSporeTriggerRoutine();
        public JailPlayerRoutine jailPlayer = new JailPlayerRoutine();
        public PetPlayerRoutine petPlayer = new PetPlayerRoutine();
        public PlayerFollowerRoutine playerFollower = new PlayerFollowerRoutine();
        public ReportBodySpam reportBodySpam = new ReportBodySpam();
        public TeleportSpammer teleportSpammer = new TeleportSpammer();

        public readonly IRoutine[] routineList;

        public RoutineManager()
        {
            routineList = [ autoTriggerSpores, discoHost, doorTroller, fungleSporeTrigger, jailPlayer, petPlayer, playerFollower, reportBodySpam, teleportSpammer ];
        }

        public void Update()
        {
            try
            {
                foreach(IRoutine routine in routineList)
                {
                    if(!routine.Enabled) continue;

                    routine.Run();
                }
            }
            catch (Exception ex)
            {
                ErrorReporter.Report(ex, HandlingId, "RoutineManager.Update: running enabled routines");
            }
        }

        [HarmonyPatch(typeof(GameData), nameof(GameData.OnDisconnected))]
        class DisconnectHandler
        {
            static void Prefix()
            {
                try
                {
                    MalumMenu.Log.LogInfo("Player disconnected from the lobby, disabling relevant routines");

                    foreach(IRoutine routine in MalumMenu.routines.routineList)
                    {
                        if(!routine.Enabled) continue;

                        routine.OnDisconnect();
                    }
                }
                catch (Exception ex)
                {
                    ErrorReporter.Report(ex, HandlingId, "DisconnectHandler.Prefix: disabling routines on disconnect");
                }
            }
        }
    }
}
