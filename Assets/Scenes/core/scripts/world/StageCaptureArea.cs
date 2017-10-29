using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay
{
	public class StageCaptureArea : NetworkBehaviour
	{
		[SerializeField] private CollisionForwarder mTrigger;

		private void Awake()
		{
			mTrigger.mTriggerEnterDelegate = OnTriggerEnter;
			mTrigger.mTriggerExitDelegate = OnTriggerExit;
		}

		private void OnTriggerEnter(Collider other)
		{
			
		}

		private void OnTriggerExit(Collider other)
		{
			
		}
	}
}
