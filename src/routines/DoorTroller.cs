using System.Collections.Generic;
using UnityEngine;

namespace MalumMenu.routines
{
    public class DoorTrollerRoutine : IRoutine
    {
        public DoorTrollerRoutine() : base("DoorTroller") { }

        public Dictionary<string, SystemTypes> openDoors = new Dictionary<string, SystemTypes>();
        public Dictionary<string, SystemTypes> closedDoors = new Dictionary<string, SystemTypes>();

        public float doorDelay = 5f;
        private float timeElapsed = 0f;
        private bool open = false;

        public override void Run()
        {
            if(ShipStatus.Instance == null) return;

            timeElapsed += Time.deltaTime;
            if(timeElapsed < doorDelay) return;
            timeElapsed = 0f;

            switch(open)
            {
                case false:
                    foreach(string door in Sabotage.GetDoors().Keys)
                    {
                        ShipStatus.Instance.RpcCloseDoorsOfType(Sabotage.GetDoors()[door]);
                    }
                    open = true;
                    break;

                case true:
                    foreach(string door in Sabotage.GetDoors().Keys)
                    {
                        ShipStatus.Instance.RpcUpdateSystem(Sabotage.GetDoors()[door], 0);
                    }
                    open = false;
                    break;
            }
        }

        protected override void OnEnable()
        {
            if(ShipStatus.Instance == null)
            {
                MalumMenu.notifications.Send("Door Troller", "Door Troller can only be used once the game has started.", 10);
                Enabled = false;
                return;
            }
        }

        public override void OnDisconnect()
        {
            MalumMenu.notifications.Send("Door Troller", "Door Troller was disabled as you left the game.", 10);
            Enabled = false;
        }
    }
}
