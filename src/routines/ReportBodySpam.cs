using UnityEngine;

namespace MalumMenu.routines
{
    public class ReportBodySpam : IRoutine
    {
        public ReportBodySpam() : base("ReportBodySpam") { }

        private float reportDelay = 0.5f;
        private float timeElapsed = 0f;

        public override void Run()
        {
            timeElapsed += Time.deltaTime;
            if(timeElapsed < reportDelay) return;
            timeElapsed = 0f;

            PlayerControl.LocalPlayer.CmdReportDeadBody(null);
        }

        protected override void OnEnable()
        {
            if(PlayerControl.LocalPlayer == null || ShipStatus.Instance == null)
            {
                MalumMenu.notifications.Send("Report Body Spam", "Report Body Spam can only be used once the game has started.", 10);
                Enabled = false;
                return;
            }
        }

        public override void OnDisconnect()
        {
            MalumMenu.notifications.Send("Report Body Spam", "Report Body Spam was disabled as you left the game.", 10);
            Enabled = false;
        }
    }
}
