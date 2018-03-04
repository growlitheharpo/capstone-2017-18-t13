using System.Collections.Generic;
using UnityEngine;

namespace FiringSquad.Gameplay.Weapons
{
	/// <inheritdoc />
	public class WeaponPartScriptScope : WeaponPartScript
	{
		/// Inspector variables
		[HideInInspector] [SerializeField] private AimDownSightsEffect[] mAimDownSightsEffects;

		/// Private variables
		private List<AimDownSightsEffect> mEffectInstances;

		/// <inheritdoc />
		public override Attachment attachPoint { get { return Attachment.Scope; } }

		/// <summary>
		/// Create a COPY of our effect to avoid shared-resource problems.
		/// </summary>
		private void Awake()
		{
			mEffectInstances = new List<AimDownSightsEffect>();
			foreach (AimDownSightsEffect e in mAimDownSightsEffects)
				mEffectInstances.Add(Instantiate(e));
		}

		/// <summary>
		/// Activate the Aim Down Sights effect for this script.
		/// </summary>
		/// <param name="weapon">The weapon we are attached to.</param>
		public void ActivateAimDownSightsEffect(IWeapon weapon)
		{
			foreach (AimDownSightsEffect e in mEffectInstances)
				e.ActivateEffect(weapon, this);
		}

		/// <summary>
		/// Deativate the Aim Down Sights effect for this script.
		/// </summary>
		/// <param name="weapon">The weapon we are attached to.</param>
		/// <param name="immediate">Whether or not to jump immediately to the "exit" state instead of lerping.</param>
		public void DeactivateAimDownSightsEffect(IWeapon weapon, bool immediate = false)
		{
			foreach (AimDownSightsEffect e in mEffectInstances)
				e.DeactivateEffect(this, immediate);
		}

		/// <inheritdoc />
		protected override void OnDestroy()
		{
			foreach (AimDownSightsEffect e in mEffectInstances)
			{
				if (e == null)
					continue;

				e.DeactivateEffect(this, true);
				Destroy(e);
			}
		}
	}
}
