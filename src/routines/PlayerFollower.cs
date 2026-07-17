using UnityEngine;

namespace MalumMenu.routines
{
    public class PlayerFollowerRoutine : IRoutine
    {
        public PlayerFollowerRoutine() : base("PlayerFollower") { }

        public PlayerControl target;
        public bool moveable = true;

        public override void Run()
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

        protected override void OnEnable()
        {
            if(PlayerControl.LocalPlayer == null)
            {
                MalumMenu.notifications.Send("Player Follower", "Player Follower can only be used once the game has started.", 10);
                Enabled = false;
                return;
            }
        }

        public override void OnDisconnect()
        {
            MalumMenu.notifications.Send("Player Follower", "Player Follower was disabled as you left the game.", 10);
            Enabled = false;
        }
    }
}
