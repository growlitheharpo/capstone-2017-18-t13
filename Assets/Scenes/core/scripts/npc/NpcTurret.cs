using System;
using System.Collections;
using System.Linq;
using FiringSquad.Core;
using FiringSquad.Core.Weapons;
using FiringSquad.Data;
using FiringSquad.Gameplay.UI;
using FiringSquad.Gameplay.Weapons;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

namespace FiringSquad.Gameplay.NPC
{
	/// <summary>
	/// Component that exists on the turret and handles character functions.
	/// Does not do the "thinking" for the turret, that exists in NpcTurretBrain.
	/// </summary>
	/// <inheritdoc cref="IWeaponBearer" />
	/// <seealso cref="NpcTurretBrain"/>
	public class NpcTurret : NetworkBehaviour, IWeaponBearer, IDamageReceiver
	{
		/// Inspector variables
		[SerializeField] private WeaponPartCollection mParts;
		[SerializeField] private NpcTurretData mData;
		[SerializeField] private Transform mWeaponAttachPoint;
		[SerializeField] private GameObject mBaseWeaponPrefab;

		/// Private variables
		private NpcTurretBrain mBrain;
		private GameObject mAliveView, mDeadView;
		private UICircleTimer mTimer;
		private IPlayerHitIndicator mHitIndicator;
		private float mHealth;

		/// <inheritdoc />
		public WeaponPartCollection defaultParts { get { return mParts; } }

		/// <summary> The balancing data for this turret. </summary>
		public NpcTurretData data { get { return mData; } }

		/// <inheritdoc />
		public IWeapon weapon { get; private set; }

		/// <inheritdoc />
		public Transform eye { get { return transform; } }

		/// <inheritdoc />
		public bool isCurrentPlayer { get { return false; } }

		/// <inheritdoc />
		public float currentHealth { get { return mHealth; } }

		#region Unity Callbacks

		/// <summary>
		/// Unity function: first frame on server.
		/// </summary>
		public override void OnStartServer()
		{
			mBrain = new NpcTurretBrain(this);
			mHealth = mData.defaultHealth;

			EventManager.Server.OnPlayerJoined += HandlePlayerCountChanged;
			EventManager.Server.OnPlayerLeft += HandlePlayerCountChanged;

			// create our weapon & bind
			BaseWeaponScript wep = Instantiate(mBaseWeaponPrefab).GetComponent<BaseWeaponScript>();
			BindWeaponToBearer(wep);
			AddDefaultPartsToWeapon();
			NetworkServer.Spawn(wep.gameObject);
		}

		/// <summary>
		/// Unity function: first frame on client.
		/// </summary>
		public override void OnStartClient()
		{
			mAliveView = transform.Find("AliveView").gameObject;
			mDeadView = transform.Find("DeadView").gameObject;
			mTimer = transform.Find("UICanvas").GetComponentInChildren<UICircleTimer>();
			mDeadView.SetActive(false);

			// create a hit indicator
			GameObject hitObject = new GameObject("HitIndicator");
			hitObject.transform.SetParent(transform);
			mHitIndicator = hitObject.AddComponent<RemotePlayerHitIndicator>();
		}

		/// <summary>
		/// Cleanup all listeners and event handlers.
		/// </summary>
		private void OnDestroy()
		{
			EventManager.Server.OnPlayerJoined -= HandlePlayerCountChanged;
			EventManager.Server.OnPlayerLeft -= HandlePlayerCountChanged;
		}

		/// <summary>
		/// Unity's Update function.
		/// </summary>
		[ServerCallback]
		private void Update()
		{
			if (mHealth <= 0.0f)
				return;

			mBrain.Think();
		}

		#endregion

		/// <summary>
		/// EVENT HANDLER: Server.OnPlayerJoined && Server.OnPlayerLeft
		/// </summary>
		[Server]
		[EventHandler]
		private void HandlePlayerCountChanged(int playerCount)
		{
			mBrain.UpdateTargetList(FindObjectsOfType<CltPlayer>().Select(x => x as ICharacter).ToArray());
		}

		#region Weapons

		/// <inheritdoc />
		public void BindWeaponToBearer(IModifiableWeapon wep, bool bindUI = false)
		{
			// find attach spot in view and set parent
			wep.transform.SetParent(mWeaponAttachPoint);
			wep.transform.ResetLocalValues();
			wep.positionOffset = transform.InverseTransformPoint(mWeaponAttachPoint.position);
			wep.transform.SetParent(transform);
			wep.bearer = this;
			weapon = wep;
		}

		/// <summary>
		/// Sets up the turret's default parts on its weapon.
		/// </summary>
		[Server]
		private void AddDefaultPartsToWeapon()
		{
			if (weapon == null)
				throw new InvalidOperationException("Cannot add default parts to a null weapon.");

			foreach (WeaponPartScript part in defaultParts)
				weapon.AttachNewPart(part.partId, WeaponPartScript.INFINITE_DURABILITY);
		}

		/// <inheritdoc />
		public void PlayFireAnimation()
		{
			// ignore for now
		}

		/// <summary>
		/// Spawn the parts on death that the player is able to pick up.
		/// </summary>
		[Server]
		private void SpawnDeathWeaponParts()
		{
			if (weapon == null)
				return;

			Vector3 weaponPos = weapon.transform.position;
			IWeaponPartManager partService = ServiceLocator.Get<IWeaponPartManager>();

			foreach (WeaponPartScript part in weapon.currentParts)
			{
				// skip parts that are not in our attach point list.
				if ((mData.partsToDrop & part.attachPoint) != part.attachPoint)
					continue;

				WeaponPartScript prefab = partService.GetPrefabScript(part.partId);
				GameObject instance = prefab.SpawnInWorld();

				instance.transform.position = weaponPos * 1.5f + Random.insideUnitSphere;
				instance.GetComponent<Rigidbody>().AddExplosionForce(40.0f, transform.position, 2.0f);

				NetworkServer.Spawn(instance);

				StartCoroutine(Coroutines.InvokeAfterFrames(2, () => { instance.GetComponent<WeaponPickupScript>().RpcInitializePickupView(); }));
			}
		}

		#endregion

		#region Health and Damage

		/// <inheritdoc />
		[Server]
		public void ApplyDamage(float amount, Vector3 point, Vector3 normal, IDamageSource cause)
		{
			RpcReflectDamageLocally(point, normal, cause.source.gameObject.transform.position, amount, cause.source.netId);

			if (mHealth <= 0.0f)
				return;

			mHealth = Mathf.Clamp(mHealth - amount, 0.0f, float.MaxValue);

			if (mHealth <= 0.0f)
				HandleTurretDeath();
		}

		/// <inheritdoc />
		public void HealDamage(float amount)
		{
			mHealth = Mathf.Clamp(mHealth + amount, 0.0f, mData.defaultHealth);
		}

		/// <summary>
		/// Reflect damage that occured on the server on each local client.
		/// </summary>
		/// <param name="point">The point where the damage occurred.</param>
		/// <param name="normal">The normal of the hit.</param>
		/// <param name="origin">The position where the hit originated from.</param>
		/// <param name="amount">The amount of damage that was caused.</param>
		/// <param name="source">The network id of the source of the damage.</param>
		[ClientRpc]
		private void RpcReflectDamageLocally(Vector3 point, Vector3 normal, Vector3 origin, float amount, NetworkInstanceId source)
		{
			ICharacter realSource = ClientScene.FindLocalObject(source).GetComponent<ICharacter>();
			if (realSource.isCurrentPlayer)
				EventManager.Notify(() => EventManager.Local.LocalPlayerCausedDamage(amount));

			mHitIndicator.NotifyHit(this, origin, point, normal, amount);
		}

		/// <summary>
		/// Handle the turret's health reaching zero by spawning parts and beginning our respawn routine.
		/// </summary>
		[Server]
		private void HandleTurretDeath()
		{
			long endTime = DateTime.Now.Ticks + (long)(data.respawnTime * TimeSpan.TicksPerSecond);

			SpawnDeathWeaponParts();
			RpcReflectDeathLocally(endTime);
			StartCoroutine(WaitAndRespawn());
		}

		/// <summary>
		/// Wait a set number of seconds and then respawn.
		/// </summary>
		[Server]
		private IEnumerator WaitAndRespawn()
		{
			yield return new WaitForSeconds(data.respawnTime);
			mHealth = data.defaultHealth;
			RpcReflectRespawnLocally();
		}

		/// <summary>
		/// Set our view to the "dead" view and display our UI respawn timer.
		/// </summary>
		/// <param name="respawnTime">The ticks at which we will respawn.</param>
		[ClientRpc]
		private void RpcReflectDeathLocally(long respawnTime)
		{
			long remainingTicks = respawnTime - DateTime.Now.Ticks;
			float currentTime = Time.time;
			float endTime = currentTime + (float)remainingTicks / TimeSpan.TicksPerSecond;
			mTimer.SetTimes(currentTime, endTime);

			mAliveView.SetActive(false);
			mDeadView.SetActive(true);
		}

		/// <summary>
		/// Reset our view to the "alive" view.
		/// </summary>
		[ClientRpc]
		private void RpcReflectRespawnLocally()
		{
			mAliveView.SetActive(true);
			mDeadView.SetActive(false);
		}

		#endregion
	}
}
