using System;
using UnityEngine;

namespace MalumMenu.routines
{
    public class TeleportSpammer : IRoutine
    {
        private const int HandlingId = 60103;

        public TeleportSpammer() : base("TeleportSpammer") { }

        private System.Random rnd = new System.Random();
        private float teleportDelay = 0.5f;
        private float timeElapsed = 0f;

        public override void Run()
        {
            try
            {
                if(ShipStatus.Instance == null) return;

                timeElapsed += Time.deltaTime;
                if(timeElapsed < teleportDelay) return;
                timeElapsed = 0f;

                foreach(PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if(player == PlayerControl.LocalPlayer) continue;

                    int ventId = rnd.Next(0, ShipStatus.Instance.AllVents.Count);

                    Teleporter.TeleportToVent(player, ventId);
                }
            }
            catch (Exception ex)
            {
                ErrorReporter.Report(ex, HandlingId, "TeleportSpammer.Run: teleporting players to random vents");
            }
        }

        protected override void OnEnable()
        {
            try
            {
                if(PlayerControl.LocalPlayer == null || ShipStatus.Instance == null)
                {
                    MalumMenu.notifications.Send("Teleport Spammer", "Teleport Spammer can only be used once the game has started.", 10);
                    Enabled = false;
                    return;
                }
            }
            catch (Exception ex)
            {
                ErrorReporter.Report(ex, HandlingId, "TeleportSpammer.OnEnable: validating game state");
            }
        }

        public override void OnDisconnect()
        {
            try
            {
                MalumMenu.notifications.Send("Teleport Spammer", "Teleport Spammer was disabled as you left the game.", 10);
                Enabled = false;
            }
            catch (Exception ex)
            {
                ErrorReporter.Report(ex, HandlingId, "TeleportSpammer.OnDisconnect: disabling routine");
            }
        }
    }
}
