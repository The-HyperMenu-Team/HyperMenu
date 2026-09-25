using System;
using Hazel;
using UnityEngine;

namespace MalumMenu.routines
{
	public class PetPlayerRoutine : IRoutine
	{
		private const int HandlingId = 60106;

		public PetPlayerRoutine() : base("PetPlayer") { }

		public readonly float PET_DELAY = 0.60f;

		public PlayerControl target;
		private float timeElapsed = 0.0f;

		public override void Run()
		{
			try
			{
				if(PlayerControl.LocalPlayer == null || target == null) return;

				timeElapsed += Time.deltaTime;
				if(timeElapsed < PET_DELAY) return;
				timeElapsed = 0.0f;

				Vector2 petPosition = target.transform.position;
				petPosition.y -= PlayerControl.LocalPlayer.cosmetics.currentPet.yOffset * 2;

				PlayerControl.LocalPlayer.cosmetics.CurrentPet.SetGettingPet(true, petPosition);
				PlayerControl.LocalPlayer.cosmetics.PettingHand.StartPet(PlayerControl.LocalPlayer.cosmetics.currentPet);

				MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
					PlayerControl.LocalPlayer.MyPhysics.NetId,
					(byte)RpcCalls.Pet,
					SendOption.Reliable,
					-1
				);

				NetHelpers.WriteVector2(PlayerControl.LocalPlayer.GetTruePosition(), writer);
				NetHelpers.WriteVector2(petPosition, writer);

				AmongUsClient.Instance.FinishRpcImmediately(writer);
			}
			catch (Exception ex)
			{
				ErrorReporter.Report(ex, HandlingId, "PetPlayerRoutine.Run: sending pet RPC at target position");
			}
		}

		protected override void OnEnable()
		{
			try
			{
				PlayerControl.LocalPlayer.moveable = false;
				PlayerControl.LocalPlayer.NetTransform.body.velocity = Vector2.zero;
			}
			catch (Exception ex)
			{
				ErrorReporter.Report(ex, HandlingId, "PetPlayerRoutine.OnEnable: freezing local player");
			}
		}

		protected override void OnDisable()
		{
			try
			{
				target = null;

				if(PlayerControl.LocalPlayer != null)
				{
					PlayerControl.LocalPlayer.moveable = true;
					PlayerControl.LocalPlayer.MyPhysics.RpcCancelPet();
				}
			}
			catch (Exception ex)
			{
				ErrorReporter.Report(ex, HandlingId, "PetPlayerRoutine.OnDisable: restoring movement and cancelling pet");
			}
		}

		public override void OnDisconnect()
		{
			try
			{
				MalumMenu.notifications.Send("Pet Player", "Pet Player was disabled as you left the game.", 10);
				Enabled = false;
			}
			catch (Exception ex)
			{
				ErrorReporter.Report(ex, HandlingId, "PetPlayerRoutine.OnDisconnect: disabling routine");
			}
		}
	}
}
