using System;
using UnityEngine;
using GameLogger = Logger;

namespace FiringSquad.Gameplay
{
	public class AIDecisionMaker
	{
		private Action mCurrentState;
		private IWeapon mWeapon;
		private Transform mEye;

		public AIDecisionMaker(IWeapon weapon, Transform eye)
		{
			mCurrentState = TickIdle;
			mWeapon = weapon;
			mEye = eye;
		}

		public void Tick()
		{
			mCurrentState.Invoke();
		}

		#region State Functions

		private void TickIdle()
		{
			// just hang out. play a bark. scratch your head.

			GameLogger.Info("IDLE");

			if (KnowsOfPlayer())
				mCurrentState = DecideToChaseOrShoot();
		}

		private void TickPurposefullyMissPlayer()
		{
			// instruct our gun to shoot near the player, but not AT them.
		}

		private void TickShootAtPlayer()
		{
			GameLogger.Info("SHOOT");
			// shoot at the player, with some level of inaccuracy.
		}

		private void TickChaseAfterPlayer()
		{
			// run towards the player with a set offset.
		}

		private void TickChaseAndShoot()
		{
			// do both of the previous
			TickChaseAfterPlayer();
			TickShootAtPlayer();
		}

		private void TickLostPlayer()
		{
			// we lost the player. check for a timeout, then return to idle or the appropriate state.
		}

		#endregion

		#region Common Utility Funcions

		/// <summary>
		/// Returns true if we currently have any senses that let us know where the player is.
		/// This means whether we can see the player or have been recently attacked by the player.
		/// </summary>
		private bool KnowsOfPlayer()
		{
			return CanSeePlayer() || AttackedByPlayer();
		}

		/// <summary>
		/// If the player is within our line-of-sight
		/// </summary>
		private bool CanSeePlayer()
		{
			return false;
		}

		/// <summary>
		/// Whether or not we were hit by the player within the threshold.
		/// </summary>
		private bool AttackedByPlayer()
		{
			return false;
		}

		private Transform GetPlayerTransform()
		{
			return null;
		}

		private Action DecideToChaseOrShoot()
		{
			// return TickShootAtPlayer, TickChaseAfterPlayer, or TickChaseAndShoot
			return TickShootAtPlayer;
			return null;
		}

		#endregion
	}
}
