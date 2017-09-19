using System;
using FiringSquad.Gameplay.AI;
using UnityEngine;

public class ReferenceForwarder : MonoBehaviour
{
	[Serializable]
	public class References
	{
		[SerializeField] private GameObject mPlayerRef;
		public GameObject player { get { return mPlayerRef; } }

		[SerializeField] private AIHintingSystem mAIHintRef;
		public AIHintingSystem aiHintSystem { get { return mAIHintRef; } }

		[SerializeField] private GameObject mDroppedWeaponParticles;
		public GameObject droppedWeaponParticlesPrefab { get { return mDroppedWeaponParticles; } }
	}

	[SerializeField] private References mReferences;
	public static References get { get { return instance.mReferences; } }

	private static ReferenceForwarder instance { get; set; }

	private void Awake()
	{
		if (instance == null)
			instance = this;
		else
			Destroy(this);
	}
}
