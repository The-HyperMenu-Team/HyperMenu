using System;
using UnityEngine;

namespace MalumMenu.routines
{
    public class ReportBodySpam : IRoutine
    {
        private const int HandlingId = 60111;

        public ReportBodySpam() : base("ReportBodySpam") { }

        private float reportDelay = 0.5f;
        private float timeElapsed = 0f;

        public override void Run()
        {
            try
            {
                timeElapsed += Time.deltaTime;
                if(timeElapsed < reportDelay) return;
                timeElapsed = 0f;

                PlayerControl.LocalPlayer.CmdReportDeadBody(null);
            }
            catch (Exception ex)
            {
                ErrorReporter.Report(ex, HandlingId, "ReportBodySpam.Run: sending report-dead-body RPC");
            }
        }

        protected override void OnEnable()
        {
            try
            {
                if(PlayerControl.LocalPlayer == null || ShipStatus.Instance == null)
                {
                    MalumMenu.notifications.Send("Report Body Spam", "Report Body Spam can only be used once the game has started.", 10);
                    Enabled = false;
                    return;
                }
            }
            catch (Exception ex)
            {
                ErrorReporter.Report(ex, HandlingId, "ReportBodySpam.OnEnable: validating game state");
            }
        }

        public override void OnDisconnect()
        {
            try
            {
                MalumMenu.notifications.Send("Report Body Spam", "Report Body Spam was disabled as you left the game.", 10);
                Enabled = false;
            }
            catch (Exception ex)
            {
                ErrorReporter.Report(ex, HandlingId, "ReportBodySpam.OnDisconnect: disabling routine");
            }
        }
    }
}
