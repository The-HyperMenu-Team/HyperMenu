using System;
using UnityEngine;

namespace MalumMenu.routines
{
	public class FungleSporeTriggerRoutine : IRoutine
	{
		private const int HandlingId = 60108;

		public FungleSporeTriggerRoutine() : base("FungleSporeTrigger") { }

		public readonly float SPORE_TRIGGER_DURATION = 5.0f;
		private float timeElapsed = 0f;

		public override void Run()
		{
			try
			{
				if(ShipStatus.Instance == null) return;

				timeElapsed += Time.deltaTime;
				if(timeElapsed < SPORE_TRIGGER_DURATION) return;
				timeElapsed = 0f;

				FungleShipStatus shipStatus = ShipStatus.Instance.Cast<FungleShipStatus>();

				Network.BatchedMessage batch = new Network.BatchedMessage();

				foreach(Mushroom mushroom in shipStatus.sporeMushrooms.Values)
				{
					batch.QueueTriggerSpore(PlayerControl.LocalPlayer, mushroom);
				}

				batch.FinishBatch();
			}
			catch (Exception ex)
			{
				ErrorReporter.Report(ex, HandlingId, "FungleSporeTriggerRoutine.Run: triggering all spore mushrooms");
			}
		}

		protected override void OnEnable()
		{
			try
			{
				if(ShipStatus.Instance == null)
				{
					MalumMenu.notifications.Send("Trigger Spores", "Auto-Trigger Spores can only be used if the game has started.", 10);
					Enabled = false;
					return;
				}

				if(Utilities.GetCurrentMap() != MapNames.Fungle)
				{
					MalumMenu.notifications.Send("Trigger Spores", "Auto-Trigger Spores can only be used in The Fungle.", 10);
					Enabled = false;
					return;
				}
			}
			catch (Exception ex)
			{
				ErrorReporter.Report(ex, HandlingId, "FungleSporeTriggerRoutine.OnEnable: validating Fungle map is loaded");
			}
		}

		public override void OnDisconnect()
		{
			try
			{
				MalumMenu.notifications.Send("Trigger Spores", "Auto-Trigger Spores was disabled as you left the game.", 10);
				Enabled = false;
			}
			catch (Exception ex)
			{
				ErrorReporter.Report(ex, HandlingId, "FungleSporeTriggerRoutine.OnDisconnect: disabling routine");
			}
		}
	}
}
