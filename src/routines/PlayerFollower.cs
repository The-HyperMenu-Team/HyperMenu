using System;
using UnityEngine;

namespace MalumMenu.routines
{
    public class PlayerFollowerRoutine : IRoutine
    {
        private const int HandlingId = 60107;

        public PlayerFollowerRoutine() : base("PlayerFollower") { }

        public PlayerControl target;
        public bool moveable = true;

        public override void Run()
        {
            try
            {
                if(target == null || PlayerControl.LocalPlayer == null || ShipStatus.Instance == null)
                {
                    MalumMenu.notifications.Send("Player Follower", "You are no longer following a player.", 10);
                    Enabled = false;
                    return;
                }

                if(PlayerControl.LocalPlayer.inVent) return;

                PlayerControl.LocalPlayer.NetTransform.SnapTo(target.transform.position);
            }
            catch (Exception ex)
            {
                ErrorReporter.Report(ex, HandlingId, "PlayerFollowerRoutine.Run: snapping to followed player");
            }
        }

        protected override void OnEnable()
        {
            try
            {
                if(PlayerControl.LocalPlayer == null)
                {
                    MalumMenu.notifications.Send("Player Follower", "Player Follower can only be used once the game has started.", 10);
                    Enabled = false;
                    return;
                }
            }
            catch (Exception ex)
            {
                ErrorReporter.Report(ex, HandlingId, "PlayerFollowerRoutine.OnEnable: validating game state");
            }
        }

        public override void OnDisconnect()
        {
            try
            {
                MalumMenu.notifications.Send("Player Follower", "Player Follower was disabled as you left the game.", 10);
                Enabled = false;
            }
            catch (Exception ex)
            {
                ErrorReporter.Report(ex, HandlingId, "PlayerFollowerRoutine.OnDisconnect: disabling routine");
            }
        }
    }
}
