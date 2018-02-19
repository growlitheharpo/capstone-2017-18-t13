using System.Collections.Generic;
using KeatsLib.Unity;
using UnityEngine;

namespace FiringSquad.Gameplay.Weapons
{
	/// <inheritdoc />
	[CreateAssetMenu(menuName = "reMod Data/ADS Effect - Tag Enemy")]
	public class TagEnemyAimDownSightsEffect : AimDownSightsEffect
	{
		/// Inspector variables
		[SerializeField] private float mTagHoverTime;

		/// Private variables
		private IWeapon mWeapon;
		private bool mInAimDownSights;
		private Dictionary<IDamageReceiver, float> mHighlightCandidates;
		private List<Renderer> mActiveHighlights;

		/// <inheritdoc />
		public override void ActivateEffect(IWeapon weapon, WeaponPartScript part)
		{
			if (ObjectHighlight.instance == null || !weapon.bearer.isCurrentPlayer)
				return;

			if (mActiveHighlights == null)
				mActiveHighlights = new List<Renderer>();

			mHighlightCandidates = new Dictionary<IDamageReceiver, float>();

			mWeapon = weapon;
			mInAimDownSights = true;
			part.StartCoroutine(Coroutines.InvokeEveryTick(TickAimDownSights));
		}

		/// <summary>
		/// Handle the player remaining in ADS by checking for enemies to tag.
		/// </summary>
		/// <returns>False if we should stop calling this Update, true if we should.</returns>
		private bool TickAimDownSights(float time)
		{
			if (!mInAimDownSights || mWeapon == null)
				return false;

			Ray ray = new Ray(mWeapon.aimRoot.position, mWeapon.aimRoot.forward);
			RaycastHit info;

			// Check if we hit anything
			if (!Physics.Raycast(ray, out info))
				return true;

			// Check if what we hit is a DamageReceiver
			IDamageReceiver d = info.GetDamageReceiver();
			if (d == null)
				return true;

			// Check if we're "counting" for this object
			if (mHighlightCandidates.ContainsKey(d))
				mHighlightCandidates[d] += Time.deltaTime;
			else
				mHighlightCandidates[d] = 0.0f;

			// Keep going if we're not at the time yet.
			if (mHighlightCandidates[d] < mTagHoverTime)
				return true;

			// Set it to an impossible time to avoid calling this more than once, and add all the renderers.
			mHighlightCandidates[d] = float.NegativeInfinity;
			var renderers = d.gameObject.GetComponentsInChildren<Renderer>();
			foreach (Renderer r in renderers)
			{
				mActiveHighlights.Add(r);
				ObjectHighlight.instance.AddRendererToHighlightList(r);
			}

			return true;
		}

		/// <inheritdoc />
		public override void DeactivateEffect(WeaponPartScript part, bool immediate)
		{
			if (!immediate)
			{
				mInAimDownSights = false;
				return;
			}

			if (ObjectHighlight.instance == null || mActiveHighlights == null)
				return;

			foreach (Renderer r in mActiveHighlights)
				ObjectHighlight.instance.RemoveRendererFromHighlightList(r);
		}
	}
}
