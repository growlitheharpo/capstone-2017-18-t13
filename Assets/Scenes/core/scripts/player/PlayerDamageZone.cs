using FiringSquad.Data;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	/// <summary>
	/// Script for attaching to the different "body parts" of the player that affect damage.
	/// </summary>
	public class PlayerDamageZone : MonoBehaviour, IDamageZone
	{
		/// Inspector variables
		[SerializeField] private Modifier.Float mDamagePercentage;

		/// Private variables
		private CltPlayer mPlayer;

		/// <inheritdoc />
		public Modifier.Float damageModification { get { return mDamagePercentage; } }

		/// <inheritdoc />
		public IDamageReceiver receiver { get { return mPlayer; } }

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		private void Awake()
		{
			mPlayer = GetComponentInParent<CltPlayer>();
		}
	}
}
