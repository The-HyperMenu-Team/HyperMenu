using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MalumMenu.routines
{
    public class DiscoHostRoutine : IRoutine
    {
        private const int HandlingId = 60109;

        public DiscoHostRoutine() : base("DiscoHost") { }
        public HashSet<int> targets = new HashSet<int>();

        public float randomizationDelay = 0.5f;
        private float timeElapsed = 0f;

        private System.Random rnd = new System.Random();

        public override void Run()
        {
            try
            {
                timeElapsed += Time.deltaTime;
                if(timeElapsed < randomizationDelay) return;
                timeElapsed = 0f;

                List<int> colors = Enumerable.Range(0, 18).ToList();

                Network.BatchedMessage batch = new Network.BatchedMessage();

                foreach(PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if(!IsGlobal && !targets.Contains(player.GetHashCode())) continue;

                    int color;
                    if(colors.Count != 0)
                    {
                        color = colors[rnd.Next(0, colors.Count)];
                        colors.Remove(color);
                    }
                    else
                    {
                        color = rnd.Next(0, 18);
                    }

                    batch.QueueSetColor(player, (byte)color);
                }

                batch.FinishBatch();
            }
            catch (Exception ex)
            {
                ErrorReporter.Report(ex, HandlingId, "DiscoHostRoutine.Run: randomizing target colors");
            }
        }

        public bool IsGlobal
        {
            get { return targets.Count == 1 && targets.Contains(int.MaxValue); }
        }

        protected override void OnEnable()
        {
            try
            {
                if(PlayerControl.LocalPlayer == null)
                {
                    MalumMenu.notifications.Send("Disco Party", "Disco Party can only be used inside of a game.", 10);
                    Enabled = false;
                    return;
                }

                if(Utilities.IsAnticheatPresent() && !AmongUsClient.Instance.AmHost)
                {
                    MalumMenu.notifications.Send("Disco Party", "Disco Party can only be used if you are the host of the lobby.", 10);
                    Enabled = false;
                    return;
                }
            }
            catch (Exception ex)
            {
                ErrorReporter.Report(ex, HandlingId, "DiscoHostRoutine.OnEnable: validating host permissions");
            }
        }

        protected override void OnDisable()
        {
            try
            {
                targets.Clear();
            }
            catch (Exception ex)
            {
                ErrorReporter.Report(ex, HandlingId, "DiscoHostRoutine.OnDisable: clearing disco targets");
            }
        }

        public override void OnDisconnect()
        {
            try
            {
                MalumMenu.notifications.Send("Disco Party", "Disco Party was disabled as you left the game.", 10);
                Enabled = false;
            }
            catch (Exception ex)
            {
                ErrorReporter.Report(ex, HandlingId, "DiscoHostRoutine.OnDisconnect: disabling routine");
            }
        }
    }
}
