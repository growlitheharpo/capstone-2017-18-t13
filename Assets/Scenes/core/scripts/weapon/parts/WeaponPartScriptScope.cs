using UnityEngine;

namespace FiringSquad.Gameplay.Weapons
{
	/// <inheritdoc />
	public class WeaponPartScriptScope : WeaponPartScript
	{
		/// Inspector variables
		[SerializeField] private AimDownSightsEffect mAimDownSightsEffect;

		/// <inheritdoc />
		public override BaseWeaponScript.Attachment attachPoint { get { return BaseWeaponScript.Attachment.Scope; } }

		/// <summary>
		/// Activate the Aim Down Sights effect for this script.
		/// </summary>
		/// <param name="weapon">The weapon we are attached to.</param>
		public void ActivateAimDownSightsEffect(IWeapon weapon)
		{
			mAimDownSightsEffect.ActivateEffect(weapon, this);
		}

		/// <summary>
		/// Deativate the Aim Down Sights effect for this script.
		/// </summary>
		/// <param name="weapon">The weapon we are attached to.</param>
		/// <param name="immediate">Whether or not to jump immediately to the "exit" state instead of lerping.</param>
		public void DeactivateAimDownSightsEffect(IWeapon weapon, bool immediate = false)
		{
			mAimDownSightsEffect.DeactivateEffect(weapon, this, immediate);
		}

		/// <summary>
		/// Unity Event Handler. Called when scope is destroyed.
		/// </summary>
		public void OnDestroy()
		{
			if (mAimDownSightsEffect != null)
				mAimDownSightsEffect.DeactivateEffect(null, this, true);
		}
	}
}
