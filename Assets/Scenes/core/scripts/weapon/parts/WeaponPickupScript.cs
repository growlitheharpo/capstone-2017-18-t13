using System;
using System.Collections;
using FiringSquad.Gameplay.UI;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay.Weapons
{
	public class WeaponPickupScript : NetworkBehaviour, IInteractable, INetworkGrabbable
	{
		private const float PICKUP_LIFETIME = 30.0f; // in seconds

		[SerializeField] private GameObject mGunView;
		[SerializeField] private GameObject mPickupView;
		[SerializeField] private GameObject mParticleSystem;

		public CltPlayer currentHolder { get; private set; }
		public bool currentlyHeld { get { return currentHolder != null; } }

		private int mOverrideDurability = WeaponPartScript.USE_DEFAULT_DURABILITY;
		public int overrideDurability { get { return mOverrideDurability; } set { mOverrideDurability = value; } }

		private WeaponPartWorldCanvas mCanvas;
		private WeaponPartScript mPartScript;
		private Rigidbody mRigidbody;

		private Coroutine mTimeoutRoutine;

		private void Awake()
		{
			mPartScript = GetComponent<WeaponPartScript>();
			mRigidbody = GetComponent<Rigidbody>();
		}

		private void Start()
		{
			InitializePickupView();
		}

		private void OnDestroy()
		{
			if (mTimeoutRoutine != null)
				StopCoroutine(mTimeoutRoutine);

			DestroyPickupView();

			transform.ResetLocalValues();
			mGunView.transform.ResetLocalValues();

			if (mRigidbody != null)
				Destroy(mRigidbody);
		}

		private void DestroyPickupView()
		{
			mGunView.SetActive(true);

			if (mPickupView != null)
				Destroy(mPickupView);
		}

		[ClientRpc]
		public void RpcInitializePickupView()
		{
			InitializePickupView();
		}

		private void InitializePickupView()
		{
			if (!mGunView.activeInHierarchy && mPickupView.activeInHierarchy)
				return;

			// flip the model views
			mGunView.SetActive(false);
			mPickupView.SetActive(true);

			// create the vfx
			GameObject ps = Instantiate(mParticleSystem);
			ps.transform.SetParent(mPickupView.transform);
			ps.transform.ResetLocalValues();

			// Update the particle systems to the correct lifetime, then start them.
			var psScripts = ps.GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem psScript in psScripts)
			{
				ParticleSystem.MainModule main = psScript.main;
				main.duration = PICKUP_LIFETIME - 2.0f;
				psScript.Play(false);
			}

			// Spawn and setup the UI name canvas
			GameObject cvPrefab = Resources.Load<GameObject>("prefabs/weapons/effects/p_partWorldCanvas");
			GameObject cv = Instantiate(cvPrefab, transform);
			mCanvas = cv.GetComponent<WeaponPartWorldCanvas>();
			mCanvas.LinkToObject(mPartScript);

			// if we're the server, destroy ourselves when the timeout is up
			if (isServer)
				mTimeoutRoutine = StartCoroutine(Timeout(ps));
		}

		[Server]
		private IEnumerator Timeout(GameObject vfxPack)
		{
			Light vfxLight = vfxPack.GetComponentInChildren<Light>();
			float originalIntensity = vfxLight.intensity;
			float currentTime = PICKUP_LIFETIME;

			while (currentTime >= 0.0f)
			{
				vfxLight.intensity = Mathf.Lerp(0.0f, originalIntensity, currentTime / 5.0f);
				mCanvas.SetMaxAlpha(Mathf.Clamp(currentTime / 5.0f, 0.0f, 1.0f));

				currentTime -= Time.deltaTime;
				yield return null;
			}

			NetworkServer.Destroy(gameObject);
		}

		[Server]
		public void Interact(ICharacter source)
		{
			IWeaponBearer wepBearer = source as IWeaponBearer;
			if (wepBearer == null)
				return;

			wepBearer.weapon.AttachNewPart(GetComponent<WeaponPartScript>().partId, overrideDurability);

			try
			{
				NetworkServer.Destroy(gameObject);
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogException(e);
			}
		}

		public void PullTowards(CltPlayer player)
		{
			if (currentlyHeld)
				return;

			Vector3 direction = player.magnetArm.transform.position - transform.position;
			direction = direction.normalized * player.magnetArm.pullForce;

			mRigidbody.AddForce(direction, ForceMode.Force);
		}

		public void GrabNow(CltPlayer player)
		{
			currentHolder = player;

			// TODO: Lerp this?

			mPickupView.transform.localScale = Vector3.one * 0.45f;
			mRigidbody.isKinematic = true;

			transform.SetParent(currentHolder.magnetArm.transform);
			transform.ResetLocalValues();

			if (player.isCurrentPlayer)
				EventManager.Notify(() => EventManager.Local.LocalPlayerHoldingPart(mPartScript));
		}

		public void Throw()
		{
			if (currentHolder == null)
				return;

			Vector3 direction = currentHolder.eye.forward;

			transform.SetParent(null);
			mRigidbody.isKinematic = false;
			mPickupView.transform.localScale = Vector3.one;

			mRigidbody.AddForce(direction * 30.0f, ForceMode.Impulse);

			if (currentHolder.isCurrentPlayer)
				EventManager.Notify(() => EventManager.Local.LocalPlayerReleasedPart(mPartScript));

			currentHolder = null;
		}

		public void Release()
		{
			transform.SetParent(null);

			mRigidbody.isKinematic = false;
			mPickupView.transform.localScale = Vector3.one;

			if (currentHolder != null && currentHolder.isCurrentPlayer)
				EventManager.Notify(() => EventManager.Local.LocalPlayerReleasedPart(mPartScript));

			currentHolder = null;
		}
	}
}
