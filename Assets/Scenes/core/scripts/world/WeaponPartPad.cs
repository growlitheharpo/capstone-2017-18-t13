using System;
using System.Collections;
using System.Collections.Generic;
using FiringSquad.Gameplay.Weapons;
using KeatsLib.Collections;
using KeatsLib.Unity;
using UnityEngine.Networking;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	/// <summary>
	/// Component for crates that contain weapon parts.
	/// Explodes and spawns a part upon receiving damage.
	/// </summary>
	public class WeaponPartPad : NetworkBehaviour
	{
		/// <summary>
		/// Struct to bind a prefab to a weight (0 - 1).
		/// </summary>
		[Serializable]
		public struct PartWeightSet
		{
			[SerializeField] private GameObject mPrefab;
			[Range(0.0f, 1.0f)] [SerializeField] private float mWeight;

			public GameObject prefab { get { return mPrefab; } }
			public float weight { get { return mWeight; } }
		}

		/// Inspector variables
		[HideInInspector] [SerializeField] private List<PartWeightSet> mParts; // [HideInInspector] because this is drawn with custom editor
		[SerializeField] private float mRespawnTimePlayerGrabbed;
		[SerializeField] private float mRespawnTimeTimeout;

		/// Syncvars
		[SyncVar(hook = "OnChangeActivated")] private bool mActivated = true;

		/// Private variables
		private Color mActiveColor, mInactiveColor;
		private Material mPadMaterial;
		private Light mLight;

		/// <summary>
		/// Unity's Awake function.
		/// </summary>
		private void Awake()
		{
			mPadMaterial = GetComponentInChildren<MeshRenderer>().material;
			mLight = GetComponentInChildren<Light>();

			mActiveColor = mPadMaterial.GetColor("_EmissionColor");
			mInactiveColor = mActiveColor * 0.25f;
		}

		/// <summary>
		/// Handle starting on the server.
		/// </summary>
		public override void OnStartServer()
		{
			SpawnPart();
		}

		/// <summary>
		/// Handle starting on the client.
		/// </summary>
		/// <inheritdoc />
		public override void OnStartClient()
		{
			base.OnStartClient();

			if (!mActivated)
				SetViewDeactivated();
		}

		/// <summary>
		/// Called on the server. Spawn a part above this pad and start watching it for player interaction.
		/// </summary>
		[Server]
		private void SpawnPart()
		{
			GameObject prefab = mParts.ChooseRandomWeighted(part => part.weight).prefab;
			GameObject instance = prefab.GetComponent<WeaponPartScript>().SpawnInWorld();
			instance.transform.position = transform.position + Vector3.up * 0.5f;
			instance.name = prefab.name;

			mActivated = true;

			instance.GetComponent<Rigidbody>().isKinematic = true;
			NetworkServer.Spawn(instance);

			StartCoroutine(WatchForPartMovement(instance));
			StartCoroutine(Coroutines.InvokeAfterFrames(2, () => { instance.GetComponent<WeaponPickupScript>().RpcInitializePickupView(); }));
		}

		/// <summary>
		/// Watch for the part that we spawned to move or time out.
		/// </summary>
		/// <param name="instance">A reference to the instance of the part we just spawned.</param>
		[Server]
		private IEnumerator WatchForPartMovement(GameObject instance)
		{
			Vector3 originalPos = instance.transform.position;
			while (true)
			{
				// ReSharper disable once ConditionIsAlwaysTrueOrFalse
				if (instance == null || !instance.activeInHierarchy)
				{
					yield return PrepRespawn(false);
					yield break;
				}

				if (Vector3.Distance(originalPos, instance.transform.position) > 1.0f)
				{
					yield return PrepRespawn(true);
					yield break;
				}

				yield return null;
			}
		}

		/// <summary>
		/// Prepare this part to respawn after a certain amount of time.
		/// </summary>
		/// <param name="playerGrabbed">Whether the part was picked up by a player (true) or timed out (false).</param>
		[Server]
		private IEnumerator PrepRespawn(bool playerGrabbed)
		{
			mActivated = false;
			yield return new WaitForSeconds(playerGrabbed ? mRespawnTimePlayerGrabbed : mRespawnTimeTimeout);
			
			SpawnPart();
		}

		/// <summary>
		/// Handle our activation state.
		/// </summary>
		/// <param name="activated">Whether to enable or disable the pad.</param>
		private void OnChangeActivated(bool activated)
		{
			mActivated = activated;

			if (mActivated)
				SetViewActivated();
			else
				SetViewDeactivated();
		}

		/// <summary>
		/// Called to set the local "view" of the pad to the activated state.
		/// </summary>
		private void SetViewActivated()
		{
			mPadMaterial.SetColor("_EmissionColor", mActiveColor);

			if (mLight != null)
			{
				mLight.color = mActiveColor;
				mLight.intensity = 3.0f;
			}
		}

		/// <summary>
		/// Called to set the local "view" of the pad to the deactivated state.
		/// </summary>
		private void SetViewDeactivated()
		{
			mPadMaterial.SetColor("_EmissionColor", mInactiveColor);

			if (mLight != null)
			{
				mLight.color = mInactiveColor;
				mLight.intensity = 1.0f;
			}
		}
	}
}
