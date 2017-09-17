using UnityEngine;

namespace FiringSquad.Data
{
	[CreateAssetMenu(menuName = "Characters/AI Navigation Weights")]
	public class AIHintValueData : ScriptableObject
	{
		[SerializeField] [Range(0.05f, 50.0f)] private float mPlayerDistanceExponent = 1.0f;
		[SerializeField] [Range(0.05f, 50.0f)] private float mPlayerDistanceWeight = 1.0f;
		[SerializeField] [Range(0.05f, 50.0f)] private float mPlayerForwardExponent = 1.0f;
		[SerializeField] [Range(0.05f, 50.0f)] private float mPlayerForwardWeight = 1.0f;
		[SerializeField] [Range(0.05f, 50.0f)] private float mEnemyDistanceExponent = 1.0f;
		[SerializeField] [Range(0.05f, 50.0f)] private float mEnemyDistanceWeight = 1.0f;

		public float playerDistanceExponent { get { return mPlayerDistanceExponent; } }
		public float playerDistanceWeight { get { return mPlayerDistanceWeight; } }
		public float playerForwardExponent { get { return mPlayerForwardExponent; } }
		public float playerForwardWeight { get { return mPlayerForwardWeight; } }
		public float enemyDistanceExponent { get { return mEnemyDistanceExponent; } }
		public float enemyDistanceWeight { get { return mEnemyDistanceWeight; } }
	}
}
