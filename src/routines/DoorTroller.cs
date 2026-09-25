using System;
using UnityEngine;

namespace MalumMenu.routines
{
    public class DoorTrollerRoutine : IRoutine
    {
        private const int HandlingId = 60110;

        public DoorTrollerRoutine() : base("DoorTroller") { }

        public float lockAndUnlockDelay = 0.5f;
        private float timeElapsed = 0f;
        private bool doorsLocked = false;

        public override void Run()
        {
            try
            {
                if(ShipStatus.Instance == null) return;

                timeElapsed += Time.deltaTime;
                if(timeElapsed < lockAndUnlockDelay) return;

                if(doorsLocked)
                {
                    Sabotage.UnlockAll();
                }
                else
                {
                    Sabotage.LockAll();
                }

                doorsLocked = !doorsLocked;
                timeElapsed = 0;
            }
            catch (Exception ex)
            {
                ErrorReporter.Report(ex, HandlingId, "DoorTrollerRoutine.Run: toggling all doors");
            }
        }

        protected override void OnEnable()
        {
            try
            {
                if(PlayerControl.LocalPlayer == null || ShipStatus.Instance == null)
                {
                    MalumMenu.notifications.Send("Door Troller", "Door Troller can only be used once the game has started.", 10);
                    Enabled = false;
                    return;
                }

                if(ShipStatus.Instance.AllDoors.Count == 0)
                {
                    MalumMenu.notifications.Send("Door Troller", "Door Troller can not be used as this map does not have any doors.", 10);
                    Enabled = false;
                    return;
                }

                if(!Sabotage.CanUnlockDoors())
                {
                    MalumMenu.notifications.Send("Door Troller", "Door Troller can only be used if you are the host, or if the current map supports unlocking doors.", 10);
                    Enabled = false;
                    return;
                }
            }
            catch (Exception ex)
            {
                ErrorReporter.Report(ex, HandlingId, "DoorTrollerRoutine.OnEnable: validating map and door support");
            }
        }

        public override void OnDisconnect()
        {
            try
            {
                MalumMenu.notifications.Send("Door Troller", "Door Troller was disabled as you left the game.", 10);
                Enabled = false;
            }
            catch (Exception ex)
            {
                ErrorReporter.Report(ex, HandlingId, "DoorTrollerRoutine.OnDisconnect: disabling routine");
            }
        }
    }
}
