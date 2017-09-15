using System;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	public class AIDecisionMaker
	{
		private Action mCurrentState;

		public AIDecisionMaker()
		{
			mCurrentState = TickIdle;
		}

		public void Tick()
		{
			mCurrentState.Invoke();
		}

		#region State Functions

		private void TickIdle()
		{
			// just hang out. play a bark. scratch your head.
		}

		private void TickPurposefullyMissPlayer()
		{
			// instruct our gun to shoot near the player, but not AT them.
		}

		private void TickShootAtPlayer()
		{
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

		private bool KnowsOfPlayer()
		{
			return CanSeePlayer() || AttackedByPlayer();
		}

		private bool CanSeePlayer()
		{
			return false;
		}

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
			return null;
		}

		#endregion
	}
}
