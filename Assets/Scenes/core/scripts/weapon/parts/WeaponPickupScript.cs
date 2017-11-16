using System;
using System.Collections;
using FiringSquad.Gameplay.UI;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay.Weapons
{
	/// <summary>
	/// The script to manage weapon parts when they exist with physics in the world.
	/// </summary>
	public class WeaponPickupScript : NetworkBehaviour, IInteractable, INetworkGrabbable
	{
		private const float PICKUP_LIFETIME = 30.0f; // in seconds

		/// Inspector variables
		[SerializeField] private GameObject mGunView;
		[SerializeField] private GameObject mPickupView;
		[SerializeField] private GameObject mParticleSystem;

		/// Sync variables
		[SyncVar] private long mDeathTimeTicks;

		/// Private variables
		private int mOverrideDurability = WeaponPartScript.USE_DEFAULT_DURABILITY;
		private WeaponPartWorldCanvas mCanvas;
		private WeaponPartScript mPartScript;
		private Rigidbody mRigidbody;
		private Coroutine mTimeoutRoutine;

		/// <inheritdoc />
		public CltPlayer currentHolder { get; private set; }

		/// <inheritdoc />
		public bool currentlyHeld { get { return currentHolder != null; } }

		/// <summary>
		/// The durability of this weapon part when it is picked up.
		/// </summary>
		public int overrideDurability { get { return mOverrideDurability; } set { mOverrideDurability = value; } }

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		private void Awake()
		{
			mPartScript = GetComponent<WeaponPartScript>();
			mRigidbody = GetComponent<Rigidbody>();
		}

		/// <summary>
		/// Unity's Start function
		/// </summary>
		private void Start()
		{
			if (isServer)
				InitializePickupView();
			else
				StartCoroutine(Coroutines.InvokeAfterFrames(5, InitializePickupView));
		}

		/// <summary>
		/// Cleanup all listeners and event handlers
		/// Prepare this part to exist on a weapon, if applicable.
		/// </summary>
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

		/// <summary>
		/// Destroy the pickup view for this part and activate the gun view.
		/// </summary>
		private void DestroyPickupView()
		{
			mGunView.SetActive(true);

			if (mPickupView != null)
				Destroy(mPickupView);
		}

		/// <summary>
		/// Reflect the pickup view state for this part across all clients.
		/// </summary>
		[ClientRpc]
		public void RpcInitializePickupView()
		{
			InitializePickupView();
		}

		/// <summary>
		/// Initialize this part to exist in the world by toggling the different views and setting a death time.
		/// </summary>
		private void InitializePickupView()
		{
			if (!mGunView.activeInHierarchy && mPickupView.activeInHierarchy)
				return;

			if (isServer)
				mDeathTimeTicks = DateTime.Now.Ticks + (int)(PICKUP_LIFETIME * TimeSpan.TicksPerSecond);

			// flip the model views
			mGunView.SetActive(false);
			mPickupView.SetActive(true);

			// create the vfx
			GameObject ps = Instantiate(mParticleSystem);
			ps.transform.SetParent(mPickupView.transform);
			ps.transform.ResetLocalValues();

			// Update the particle systems to the correct lifetime, then start them.
			var psScripts = ps.GetComponentsInChildren<ParticleSystem>();
			float remaining = (mDeathTimeTicks - DateTime.Now.Ticks) / (float)TimeSpan.TicksPerSecond;
			foreach (ParticleSystem psScript in psScripts)
			{
				ParticleSystem.MainModule main = psScript.main;
				main.duration = remaining - 2.0f; // the extra two seconds are for the lifetime of the particles
				psScript.Play(false);
			}

			// Spawn and setup the UI name canvas
			GameObject cvPrefab = Resources.Load<GameObject>("prefabs/weapons/effects/p_partWorldCanvas");
			GameObject cv = Instantiate(cvPrefab, transform);
			mCanvas = cv.GetComponent<WeaponPartWorldCanvas>();
			mCanvas.LinkToObject(mPartScript);

			// tick our lifetime timer
			mTimeoutRoutine = StartCoroutine(Timeout(ps));
		}

		/// <summary>
		/// Handle watching the lifetime of this part, and destroy it when it's over.
		/// </summary>
		/// <param name="vfxPack">The gameobject of our effect pack.</param>
		private IEnumerator Timeout(GameObject vfxPack)
		{
			Light vfxLight = vfxPack.GetComponentInChildren<Light>();
			float originalIntensity = vfxLight.intensity;

			while (true)
			{
				yield return null;

				float remaining = (mDeathTimeTicks - DateTime.Now.Ticks) / (float)TimeSpan.TicksPerSecond;
				if (remaining <= 0.0f)
				{
					if (isServer)
						break;
					continue;
				}

				vfxLight.intensity = Mathf.Lerp(0.0f, originalIntensity, remaining / 5.0f);
				mCanvas.SetMaxAlpha(Mathf.Clamp(remaining / 5.0f, 0.0f, 1.0f));
			}

			if (isServer)
				NetworkServer.Destroy(gameObject);
		}

		/// <inheritdoc />
		[Server]
		public void Interact(ICharacter source)
		{
			IWeaponBearer wepBearer = source as IWeaponBearer;
			if (wepBearer == null)
				return;

			if (wepBearer.weapon != null)
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

		/// <inheritdoc />
		public void PullTowards(CltPlayer player)
		{
			if (currentlyHeld)
				return;

			Vector3 direction = player.magnetArm.transform.position - transform.position;
			direction = direction.normalized * player.magnetArm.pullForce;

			mRigidbody.AddForce(direction, ForceMode.Force);
		}

		/// <inheritdoc />
		public void GrabNow(CltPlayer player)
		{
			currentHolder = player;

			// TODO: Lerp this?

			mPickupView.transform.localScale = Vector3.one * 0.45f;
			mRigidbody.isKinematic = true;

			transform.SetParent(player.magnetArm.transform);
			transform.ResetLocalValues();

			if (player.isCurrentPlayer)
				EventManager.Notify(() => EventManager.Local.LocalPlayerHoldingPart(mPartScript));
		}

		/// <inheritdoc />
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

		/// <inheritdoc />
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
