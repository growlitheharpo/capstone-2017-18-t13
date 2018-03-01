using System;
using System.Collections;
using System.Collections.Generic;
using FiringSquad.Core;
using FiringSquad.Core.Audio;
using FiringSquad.Core.UI;
using FiringSquad.Core.Weapons;
using FiringSquad.Data;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

namespace FiringSquad.Gameplay.Weapons
{
	/// <summary>
	/// The implementation of the basic modifiable weapon.
	/// </summary>
	/// <inheritdoc cref="IModifiableWeapon" />
	public class BaseWeaponScript : NetworkBehaviour, IModifiableWeapon
	{
		/// <summary>
		/// The enum for masking what network updates must be sent.
		/// </summary>
		[Flags]
		private enum DirtyBitFlags
		{
			None = 0x0,
			Bearer = 0x1,
			ScopeId = 0x2,
			BarrelId = 0x4,
			MechanismId = 0x8,
			GripId = 0x10,
			Durability = 0x20,
		}

		/// Inspector variables
		[SerializeField] private float mAimDownSightsDispersionMod;
		[SerializeField] private WeaponData mDefaultData;

		/// Private variables
		private IWeaponBearer mBearer;
		private WeaponPartCollection mCurrentParts;
		private WeaponData mCurrentData, mAimDownSightsData;
		private BaseWeaponView mWeaponView;

		private BoundProperty<int> mShotsInClip, mTotalClipSize;

		private List<float> mRecentShotTimes;
		private int mShotsSinceRelease;
		private bool mReloading, mAimDownSightsActive;

		/// <inheritdoc cref="IModifiableWeapon" />
		public Transform aimRoot { get; set; }

		/// <inheritdoc cref="IModifiableWeapon" />
		public Vector3 positionOffset { get; set; }

		/// <inheritdoc />
		public WeaponData baseData { get { return mDefaultData; } }

		/// <inheritdoc />
		public WeaponPartCollection currentParts { get { return mCurrentParts; } }

		/// <inheritdoc />
		public bool aimDownSightsActive {get { return mAimDownSightsActive; }}

		/// <inheritdoc />
		public WeaponData currentData
		{
			get
			{
				return mAimDownSightsActive ? mAimDownSightsData : mCurrentData;
			}
		}

		/// <inheritdoc cref="IModifiableWeapon.bearer" />
		public IWeaponBearer bearer
		{
			get { return mBearer; }
			set
			{
				mBearer = value;
				SetDirtyBit(syncVarDirtyBits | (uint)DirtyBitFlags.Bearer);
			}
		}
		
		/// <summary>
		/// The amount of time between each shot.
		/// </summary>
		private float timePerShot { get { return 1.0f / currentData.fireRate; } }

		/// <summary>
		/// The current time of the game.
		/// </summary>
		private float currentTime { get { return Time.time; } }

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		private void Awake()
		{
			mCurrentData = new WeaponData(baseData);
			mRecentShotTimes = new List<float>();
			mCurrentParts = new WeaponPartCollection();
			mShotsInClip = new BoundProperty<int>();
			mTotalClipSize = new BoundProperty<int>();

			mWeaponView = GetComponent<BaseWeaponView>();
		}
		
		/// <summary>
		/// Cleanup all listeners and event handlers.
		/// </summary>
		private void OnDestroy()
		{
			mShotsInClip.Cleanup();
			mTotalClipSize.Cleanup();
		}

		/// <inheritdoc />
		public void BindPropertiesToUI()
		{
			mShotsInClip = new BoundProperty<int>(mShotsInClip == null ? 0 : mShotsInClip.value, UIManager.CLIP_CURRENT);
			mTotalClipSize = new BoundProperty<int>(mTotalClipSize == null ? 0 : mTotalClipSize.value, UIManager.CLIP_TOTAL);
		}


		#region Serialization

		/// <summary>
		/// Write the changed data for this weapon to the stream.
		/// </summary>
		/// <inheritdoc />
		public override bool OnSerialize(NetworkWriter writer, bool forceAll)
		{
			if (forceAll)
			{
				writer.Write(bearer.netId);

				foreach (WeaponPartScript p in currentParts)
				{
					p.SerializeId(writer);
					p.SerializeDurability(writer);
				}
			}
			else // if (!forceAll)
			{
				DirtyBitFlags flags = (DirtyBitFlags)syncVarDirtyBits;

				writer.Write((byte)flags);

				if ((flags & DirtyBitFlags.Bearer) != 0)
					writer.Write(bearer.netId);
				if ((flags & DirtyBitFlags.ScopeId) != 0 && currentParts.scope != null)
				{
					currentParts.scope.SerializeId(writer);
					currentParts.scope.SerializeDurability(writer);
				}
				if ((flags & DirtyBitFlags.BarrelId) != 0 && currentParts.barrel != null)
				{
					currentParts.barrel.SerializeId(writer);
					currentParts.barrel.SerializeDurability(writer);
				}
				if ((flags & DirtyBitFlags.MechanismId) != 0 && currentParts.mechanism != null)
				{
					currentParts.mechanism.SerializeId(writer);
					currentParts.mechanism.SerializeDurability(writer);
				}
				if ((flags & DirtyBitFlags.GripId) != 0 && currentParts.grip != null)
				{
					currentParts.grip.SerializeId(writer);
					currentParts.grip.SerializeDurability(writer);
				}
				if ((flags & DirtyBitFlags.Durability) != 0)
				{
					foreach (WeaponPartScript p in currentParts)
						p.SerializeDurability(writer);
				}
			}

			ClearAllDirtyBits();
			return true;
		}

		/// <summary>
		/// Update the changed data for this weapon.
		/// </summary>
		/// <inheritdoc />
		public override void OnDeserialize(NetworkReader reader, bool forceAll)
		{
			if (forceAll)
			{
				NetworkInstanceId bearerId = reader.ReadNetworkId();
				StartCoroutine(BindToBearer(bearerId));

				for (int i = 0; i < 4; i++)
				{
					byte id = WeaponPartScript.DeserializeId(reader);
					int durability = WeaponPartScript.DeserializeDurability(reader);

					AttachNewPart(id, durability);
				}
			}
			else // if (!forceAll)
			{
				DirtyBitFlags flags = (DirtyBitFlags)reader.ReadByte();

				if ((flags & DirtyBitFlags.Bearer) != 0)
				{
					NetworkInstanceId bearerId = reader.ReadNetworkId();
					StartCoroutine(BindToBearer(bearerId));
				}

				if ((flags & DirtyBitFlags.ScopeId) != 0)
				{
					byte id = WeaponPartScript.DeserializeId(reader);
					int durability = WeaponPartScript.DeserializeDurability(reader);

					AttachNewPart(id, durability);
				}
				if ((flags & DirtyBitFlags.BarrelId) != 0)
				{
					byte id = WeaponPartScript.DeserializeId(reader);
					int durability = WeaponPartScript.DeserializeDurability(reader);

					AttachNewPart(id, durability);
				}
				if ((flags & DirtyBitFlags.MechanismId) != 0)
				{
					byte id = WeaponPartScript.DeserializeId(reader);
					int durability = WeaponPartScript.DeserializeDurability(reader);

					AttachNewPart(id, durability);
				}
				if ((flags & DirtyBitFlags.GripId) != 0)
				{
					byte id = WeaponPartScript.DeserializeId(reader);
					int durability = WeaponPartScript.DeserializeDurability(reader);

					AttachNewPart(id, durability);
				}

				if ((flags & DirtyBitFlags.Durability) != 0)
				{
					foreach (WeaponPartScript p in currentParts)
						p.durability = WeaponPartScript.DeserializeDurability(reader);
				}
			}
		}

		/// <summary>
		/// Bind this weapon to its bearer once they become available.
		/// </summary>
		/// <param name="bearerId">The network instance ID of our bearer.</param>
		private IEnumerator BindToBearer(NetworkInstanceId bearerId)
		{
			while (bearer == null || bearer.netId != bearerId)
			{
				GameObject obj = ClientScene.FindLocalObject(bearerId);
				if (obj != null)
					bearer = obj.GetComponent<IWeaponBearer>();

				if (bearer != null)
				{
					bearer.BindWeaponToBearer(this);
					yield break;
				}

				yield return null;
			}
		}

		/// <summary>
		/// Get the dirty flag enum from an attachment enum.
		/// TODO: Should this really be a function? Seems like a lot.
		/// </summary>
		/// <param name="a">The attachment slot to map.</param>
		private DirtyBitFlags GetBitFromAttach(Attachment a)
		{
			switch (a)
			{
				case Attachment.Scope:
					return DirtyBitFlags.ScopeId;
				case Attachment.Barrel:
					return DirtyBitFlags.BarrelId;
				case Attachment.Mechanism:
					return DirtyBitFlags.MechanismId;
				case Attachment.Grip:
					return DirtyBitFlags.GripId;
				default:
					return DirtyBitFlags.None;
			}
		}

		#endregion

		#region Part Attachment

		/// <inheritdoc />
		public void ResetToDefaultParts()
		{
			foreach (WeaponPartScript p in bearer.defaultParts)
				AttachNewPart(p.partId, p.durability);
		}

		/// <inheritdoc />
		public void AttachNewPart(byte partId, int durability = WeaponPartScript.USE_DEFAULT_DURABILITY)
		{
			if (partId == 0)
				return;

			WeaponPartScript prefab = ServiceLocator.Get<IWeaponPartManager>().GetPrefabScript(partId);
			SetDirtyBit(syncVarDirtyBits | (uint)GetBitFromAttach(prefab.attachPoint));
			WeaponPartScript instance = prefab.SpawnForWeapon(this);

			int originalClipsize = mCurrentData.clipSize;

			mWeaponView.MoveAttachmentToPoint(instance);
			mCurrentParts[instance.attachPoint] = instance;

			instance.durability = durability == WeaponPartScript.USE_DEFAULT_DURABILITY ? prefab.durability : durability;

			mCurrentData = mAimDownSightsData = WeaponData.ActivatePartEffects(baseData, mCurrentParts);
			mAimDownSightsData.ForceModifyMinDispersion(new Modifier.Float(mAimDownSightsDispersionMod, Modifier.ModType.SetPercentage));
			mAimDownSightsData.ForceModifyMaxDispersion(new Modifier.Float(mAimDownSightsDispersionMod, Modifier.ModType.SetPercentage));

			if (instance.attachPoint == Attachment.Mechanism || mCurrentData.clipSize != originalClipsize)
			{
				if (mCurrentParts.mechanism != null)
				{
					mShotsInClip.value = mCurrentData.clipSize;
					mTotalClipSize.value = mCurrentData.clipSize;
				}
			}


			if (bearer != null)
			{
				ServiceLocator.Get<IAudioManager>().CreateSound(AudioEvent.EquipItem, transform);
				
				if (bearer.isCurrentPlayer)
					EventManager.Notify(() => EventManager.Local.LocalPlayerAttachedPart(this, instance));
			}

			if (instance.attachPoint == Attachment.Scope && mAimDownSightsActive && currentParts.scope != null)
				currentParts.scope.ActivateAimDownSightsEffect(this);
		}

		#endregion

		#region Reloading

		/// <inheritdoc />
		public void Reload()
		{
			if (mReloading || mAimDownSightsActive)
				return;

			mReloading = true;
			mWeaponView.UpdateArmRecoilAnimation(false, false);
			mWeaponView.PlayReloadEffect(currentData.reloadTime);
			StartCoroutine(Coroutines.InvokeAfterSeconds(currentData.reloadTime, FinishReload));
		}

		/// <summary>
		/// Finalize the "reloading" state after the reload time has completed.
		/// </summary>
		private void FinishReload()
		{
			mShotsInClip.value = currentData.clipSize;
			mReloading = false;
		}

		#endregion

		#region Weapon Firing

		/// <inheritdoc />
		public void FireWeaponHold()
		{
			TryFireShot();
		}

		/// <inheritdoc />
		public void FireWeaponUp()
		{
			mShotsSinceRelease = 0;
			mWeaponView.UpdateArmRecoilAnimation(false, false);
		}

		/// <summary>
		/// Fire our weapon if we are able to.
		/// Calculates shot dispersion, number of shots, etc.
		/// Spawns the shots over the network.
		/// </summary>
		private void TryFireShot()
		{
			CleanupRecentShots();
			if (!CanFireShotNow())
				return;

			int count = mCurrentParts.barrel != null ? mCurrentParts.barrel.projectileCount : 1;
			List<Vector3> origins = new List<Vector3>(), directions = new List<Vector3>();
			for (int i = 0; i < count; i++)
			{
				Ray shot = CalculateShotDirection(i == 0);
				origins.Add(shot.origin);
				directions.Add(shot.direction);
			}

			mRecentShotTimes.Add(currentTime);
			mShotsSinceRelease++;
			mShotsInClip.value--;

			CmdOnShotFireComplete(origins.ToArray(), directions.ToArray());
			mWeaponView.PlayFireEffect();
		}

		/// <summary>
		/// Notify the server that the weapon has finished a "shot" after instantiating all of its projectiles.
		/// </summary>
		[Command]
		private void CmdOnShotFireComplete(Vector3[] origins, Vector3[] directions)
		{
			for (int i = 0; i < origins.Length; ++i)
				InstantiateShot(origins[i], directions[i]);

			RpcReflectPlayerShotWeapon();
			DegradeDurability();
		}

		/// <summary>
		/// Instantiate a shot immediately on the network.
		/// </summary>
		/// <param name="origin">The origin of the shot. The local position of the aimRoot on fire.</param>
		/// <param name="direction">The direction of the shot. The forward of the aimRoot on fire.</param>
		[Server]
		private void InstantiateShot(Vector3 origin, Vector3 direction)
		{
			Ray shot = new Ray(origin, direction);

			if (mCurrentParts.mechanism == null || mCurrentParts.barrel == null)
				throw new InvalidOperationException("Attempted to fire a weapon without setting the mechanism and barrel!");

			GameObject instance = Instantiate(currentParts.mechanism.projectilePrefab, currentParts.barrel.barrelTip.position, Quaternion.identity);
			IProjectile projectile = instance.GetComponent<IProjectile>();

			if (projectile.PreSpawnInitialize(this, shot, currentData))
				NetworkServer.Spawn(instance);
			projectile.PostSpawnInitialize(this, shot, currentData);
		}

		/// <summary>
		/// Reflect the fire effect across all clients.
		/// </summary>
		[ClientRpc]
		private void RpcReflectPlayerShotWeapon()
		{
			if (!bearer.isCurrentPlayer)
				mWeaponView.PlayFireEffect();
		}

		/// <summary>
		/// Returns true if the player is allowed to shoot right now, false if not.
		/// Checks the number of shots in clip, the time between shots, the shots per click, etc.
		/// </summary>
		private bool CanFireShotNow()
		{
			float lastShotTime = mRecentShotTimes.Count >= 1 ? mRecentShotTimes[mRecentShotTimes.Count - 1] : -1.0f;
			if (mReloading || currentTime - lastShotTime < timePerShot)
				return false;

			WeaponPartScriptBarrel barrel = mCurrentParts.barrel;
			if (barrel == null || barrel.shotsPerClick > 0 && mShotsSinceRelease >= barrel.shotsPerClick)
			{
				mWeaponView.UpdateArmRecoilAnimation(false, false);
				return false;
			}

			if (mShotsInClip.value <= 0)
			{
				Reload();
				return false;
			}

			return true;
		}

		/// <summary>
		/// Determine the direction of the next shot based on our aim root.
		/// </summary>
		/// <param name="firstShot">If true, forces, the shot to be perfectly straight.</param>
		/// <returns></returns>
		private Ray CalculateShotDirection(bool firstShot)
		{
			float dispersionFactor = GetCurrentDispersionFactor(!firstShot);
			Vector3 randomness = Random.insideUnitSphere * dispersionFactor;

			Transform root = GetAimRoot();
			return new Ray(root.position, root.forward + randomness);
		}

		/// <inheritdoc />
		public float GetCurrentDispersionFactor(bool forceNotZero)
		{
			float percentage = 0.0f;
			float inverseFireRate = 1.0f / currentData.fireRate;

			foreach (float shot in mRecentShotTimes)
			{
				float timeSinceShot = currentTime - shot;
				if (timeSinceShot > inverseFireRate * 2.0f)
					continue;

				float p = Mathf.Pow(Mathf.Clamp(inverseFireRate / timeSinceShot, 0.0f, 1.0f), 2);
				percentage += p * currentData.dispersionRamp;
			}

			if (!forceNotZero && percentage <= 0.005f)
				return 0.0f;

			return Mathf.Lerp(currentData.minimumDispersion, currentData.maximumDispersion, percentage);
		}

		/// <summary>
		/// Get the Transform where the shots of this weapon originate from.
		/// </summary>
		private Transform GetAimRoot()
		{
			if (mCurrentParts.mechanism != null && !mCurrentParts.mechanism.overrideHitscanMethod && aimRoot != null)
				return aimRoot;

			if (mCurrentParts.barrel != null)
				return mCurrentParts.barrel.barrelTip;

			return transform;
		}

		/// <summary>
		/// Immediately degrade the weapon's durability.
		/// </summary>
		[Server]
		private void DegradeDurability()
		{
			foreach (WeaponPartScript part in mCurrentParts)
			{
				if (part.durability == WeaponPartScript.INFINITE_DURABILITY)
					continue;

				part.durability -= 1;
				if (part.durability == 0)
					BreakPart(part);
			}

			SetDirtyBit(syncVarDirtyBits | (uint)DirtyBitFlags.Durability);
		}

		/// <summary>
		/// Break a part immediately.
		/// </summary>
		/// <param name="part">The weapon part to break.</param>
		[Server]
		private void BreakPart(WeaponPartScript part)
		{
			CmdRequestBreakEffect();
			AttachNewPart(bearer.defaultParts[part.attachPoint].partId, WeaponPartScript.INFINITE_DURABILITY);
		}

		/// <summary>
		/// Play the break effect across all clients.
		/// </summary>
		[Command]
		private void CmdRequestBreakEffect()
		{
			RpcCreateBreakPartEffect();
		}

		/// <summary>
		/// Activate the break part effect across all clients.
		/// TODO: Does this need to be done with an RPC??
		/// </summary>
		[ClientRpc]
		private void RpcCreateBreakPartEffect()
		{
			mWeaponView.CreateBreakPartEffect();
		}

		#endregion

		#region Aim Down Sights

		/// <summary>
		/// Activate Aim Down Sights mode based on whatever scope is currently attached to this weapon.
		/// </summary>
		[Client]
		public void EnterAimDownSightsMode()
		{
			if (!bearer.isCurrentPlayer)
				return;

			mAimDownSightsActive = true;
			if (mCurrentParts.scope != null)
				mCurrentParts.scope.ActivateAimDownSightsEffect(this);
		}

		/// <summary>
		/// Deactivate aim down sights mode.
		/// </summary>
		[Client]
		public void ExitAimDownSightsMode()
		{
			if (!bearer.isCurrentPlayer)
				return;

			mAimDownSightsActive = false;
			if (mCurrentParts.scope != null)
				mCurrentParts.scope.DeactivateAimDownSightsEffect(this);
		}

		#endregion

		#region Data Management

		/// <summary>
		/// Cleanup recent shots stored in our data that are no longer relevant.
		/// </summary>
		private void CleanupRecentShots()
		{
			float inverseFireRate = 1.0f / currentData.fireRate * 10.0f;

			for (int i = 0; i < mRecentShotTimes.Count; i++)
			{
				float timeSinceShot = currentTime - mRecentShotTimes[i];

				if (timeSinceShot < inverseFireRate)
					continue;

				mRecentShotTimes.RemoveAt(i);
				--i;
			}
		}

		/// <inheritdoc />
		[Client]
		public float GetCurrentRecoil()
		{
			float value = 0.0f;
			foreach (float v in mRecentShotTimes)
			{
				float timeSinceShot = currentTime - v;
				float percent = Mathf.Clamp(timeSinceShot / currentData.recoilTime, 0.0f, 1.0f);
				float sample = currentData.recoilCurve.Evaluate(percent);
				value += sample;
			}

			return value * currentData.recoilAmount;
		}

		#endregion
	}
}
