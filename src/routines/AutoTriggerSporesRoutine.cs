using Hazel;
using UnityEngine;

namespace MalumMenu.routines
{
    public class AutoTriggerSporesRoutine : IRoutine
    {
        public AutoTriggerSporesRoutine() : base("AutoTriggerSpores") { }

        private float scanDelay = 0.5f;
        private float timeElapsed = 0f;

        public override void Run()
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

        protected override void OnEnable()
        {
            if(PlayerControl.LocalPlayer == null || ShipStatus.Instance == null)
            {
                MalumMenu.notifications.Send("Auto Medbay Scan", "Auto Medbay Scan can only be used once the game has started.", 10);
                Enabled = false;
                return;
            }
        }

        public override void OnDisconnect()
        {
            MalumMenu.notifications.Send("Auto Medbay Scan", "Auto Medbay Scan was disabled as you left the game.", 10);
            Enabled = false;
        }
    }
}
