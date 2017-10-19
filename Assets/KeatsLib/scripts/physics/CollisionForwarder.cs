using System;
using UnityEngine;

namespace KeatsLib.Unity
{
	/// <summary>
	/// A utility component placed on a child object when its parent wants to receive
	/// all Trigger/Collision Enter/Stay/Exit calls as if they were its own.
	/// Requires setup calls in the parent to set the relevant delegates.
	/// </summary>
	public class CollisionForwarder : MonoBehaviour
	{
		public Action<Collision2D> mCollision2DEnterDelegate, mCollision2DExitDelegate, mCollision2DStayDelegate;
		public Action<Collision> mCollisionEnterDelegate, mCollisionExitDelegate, mCollisionStayDelegate;

		public Action<Collider2D> mTrigger2DEnterDelegate, mTrigger2DExitDelegate, mTrigger2DStayDelegate;
		public Action<Collider> mTriggerEnterDelegate, mTriggerExitDelegate, mTriggerStayDelegate;

		// For every Unity signal, call the relevant delegate if we have one:

		private void OnTriggerEnter(Collider col)
		{
			if (mTriggerEnterDelegate != null)
				mTriggerEnterDelegate(col);
		}

		private void OnTriggerExit(Collider col)
		{
			if (mTriggerExitDelegate != null)
				mTriggerExitDelegate(col);
		}

		private void OnTriggerStay(Collider col)
		{
			if (mTriggerStayDelegate != null)
				mTriggerStayDelegate(col);
		}

		private void OnCollisionEnter(Collision col)
		{
			if (mCollisionEnterDelegate != null)
				mCollisionEnterDelegate(col);
		}

		private void OnCollisionExit(Collision col)
		{
			if (mCollisionExitDelegate != null)
				mCollisionExitDelegate(col);
		}

		private void OnCollisionStay(Collision col)
		{
			if (mCollisionStayDelegate != null)
				mCollisionStayDelegate(col);
		}

		private void OnTriggerEnter2D(Collider2D col)
		{
			if (mTrigger2DEnterDelegate != null)
				mTrigger2DEnterDelegate(col);
		}

		private void OnTriggerExit2D(Collider2D col)
		{
			if (mTrigger2DExitDelegate != null)
				mTrigger2DExitDelegate(col);
		}

		private void OnTriggerStay2D(Collider2D col)
		{
			if (mTrigger2DStayDelegate != null)
				mTrigger2DStayDelegate(col);
		}

		private void OnCollisionEnter2D(Collision2D col)
		{
			if (mCollision2DEnterDelegate != null)
				mCollision2DEnterDelegate(col);
		}

		private void OnCollisionExit2D(Collision2D col)
		{
			if (mCollision2DExitDelegate != null)
				mCollision2DExitDelegate(col);
		}

		private void OnCollisionStay2D(Collision2D col)
		{
			if (mCollision2DStayDelegate != null)
				mCollision2DStayDelegate(col);
		}
	}
}
