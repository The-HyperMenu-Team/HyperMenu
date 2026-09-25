using System;
using Hazel;
using UnityEngine;

namespace MalumMenu.routines
{
    public class AutoTriggerSporesRoutine : IRoutine
    {
        private const int HandlingId = 60104;

        public AutoTriggerSporesRoutine() : base("AutoTriggerSpores") { }

        private float scanDelay = 0.5f;
        private float timeElapsed = 0f;

        public override void Run()
        {
            try
            {
                if(ShipStatus.Instance == null) return;

                timeElapsed += Time.deltaTime;
                if(timeElapsed < scanDelay) return;
                timeElapsed = 0f;

                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SetScanner, SendOption.Reliable, -1);
                writer.Write(true);
                writer.Write(++PlayerControl.LocalPlayer.scannerCount);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
            catch (Exception ex)
            {
                ErrorReporter.Report(ex, HandlingId, "AutoTriggerSporesRoutine.Run: sending medbay scan RPC");
            }
        }

        protected override void OnEnable()
        {
            try
            {
                if(PlayerControl.LocalPlayer == null || ShipStatus.Instance == null)
                {
                    MalumMenu.notifications.Send("Auto Medbay Scan", "Auto Medbay Scan can only be used once the game has started.", 10);
                    Enabled = false;
                    return;
                }
            }
            catch (Exception ex)
            {
                ErrorReporter.Report(ex, HandlingId, "AutoTriggerSporesRoutine.OnEnable: validating game state");
            }
        }

        public override void OnDisconnect()
        {
            try
            {
                MalumMenu.notifications.Send("Auto Medbay Scan", "Auto Medbay Scan was disabled as you left the game.", 10);
                Enabled = false;
            }
            catch (Exception ex)
            {
                ErrorReporter.Report(ex, HandlingId, "AutoTriggerSporesRoutine.OnDisconnect: disabling routine");
            }
        }
    }
}
