using UnityEngine;

namespace FiringSquad.Data
{
	[CreateAssetMenu(menuName = "Characters/AI Navigation Weights")]
	public class AIHintValueData : ScriptableObject
	{
		// TODO: Fix the player distance weight
		[Header("DO NOT USE - Player Distance Variables")]
		[Tooltip("DO NOT USE")][SerializeField] [Range(0.05f, 50.0f)] private float mPlayerDistanceExponent = 1.0f;
		[Tooltip("DO NOT USE")][SerializeField] [Range(0.00f, 50.0f)] private float mPlayerDistanceWeight = 0.0f;

		[Header("\"Stay in Player Field Of View\" Variables")]
		[Tooltip("Higher value = we need to be closer to where the player is looking for a position to be considered good.")]
		[SerializeField] [Range(0.05f, 50.0f)] private float mPlayerForwardExponent = 1.0f;
		[Tooltip("How important should staying within the player's view be to this AI.")]
		[SerializeField] [Range(0.05f, 50.0f)] private float mPlayerForwardWeight = 1.0f;

		[Header("\"Stay Close to Where We Are\" Variables")]
		[Tooltip("Higher value = we need to be closer to our current position for a position to be considered good.")]
		[SerializeField] [Range(0.05f, 50.0f)] private float mEnemyDistanceExponent = 1.0f;
		[Tooltip("How important should staying near where we currently are be to this AI.")]
		[SerializeField] [Range(0.05f, 50.0f)] private float mEnemyDistanceWeight = 1.0f;

		public float playerDistanceExponent { get { return mPlayerDistanceExponent; } }
		public float playerDistanceWeight { get { return mPlayerDistanceWeight; } }
		public float playerForwardExponent { get { return mPlayerForwardExponent; } }
		public float playerForwardWeight { get { return mPlayerForwardWeight; } }
		public float enemyDistanceExponent { get { return mEnemyDistanceExponent; } }
		public float enemyDistanceWeight { get { return mEnemyDistanceWeight; } }
	}
}
