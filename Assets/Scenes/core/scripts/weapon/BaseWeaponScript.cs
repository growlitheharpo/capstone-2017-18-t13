using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
	public class BaseWeaponScript : NetworkBehaviour, IModifiableWeapon
	{
		[Flags]
		public enum Attachment
		{
			Scope = 0x1,
			Barrel = 0x2,
			Mechanism = 0x4,
			Grip = 0x8,
		}

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

		private IWeaponBearer mBearer;
		public IWeaponBearer bearer
		{
			get { return mBearer; }
			set
			{
				mBearer = value;
				SetDirtyBit(syncVarDirtyBits | (uint)DirtyBitFlags.Bearer);
			}
		}

		public Transform aimRoot { get; set; }
		public Vector3 positionOffset { get; set; }

		[SerializeField] private Transform mBarrelAttach;
		[SerializeField] private Transform mScopeAttach;
		[SerializeField] private Transform mMechanismAttach;
		[SerializeField] private Transform mGripAttach;
		[SerializeField] private float mAimDownSightsDispersionMod;
		[SerializeField] private WeaponData mDefaultData;
		public WeaponData baseData { get { return mDefaultData; } }

		private WeaponPartCollection mCurrentParts;
		public WeaponPartCollection currentParts { get { return mCurrentParts; } }

		private Dictionary<Attachment, Transform> mAttachPoints;
		private WeaponData mCurrentData, mAimDownSightsData;

		public WeaponData currentData
		{
			get
			{
				return mAimDownSightsActive ? mAimDownSightsData : mCurrentData;
			}
		}

		private float timePerShot { get { return 1.0f / currentData.fireRate; } }

		private bool mReloading;
		private BoundProperty<int> mShotsInClip, mTotalClipSize;
		private int mShotsSinceRelease;
		private List<float> mRecentShotTimes;
		private ParticleSystem mShotParticles, mPartBreakPrefab;
		private Animator mAnimator;
		private bool mAimDownSightsActive;

		private const float CAMERA_FOLLOW_FACTOR = 10.0f;
		private float currentTime { get { return Time.time; } }

		private void Awake()
		{
			mCurrentData = new WeaponData(baseData);
			mRecentShotTimes = new List<float>();
			mCurrentParts = new WeaponPartCollection();

			mAttachPoints = new Dictionary<Attachment, Transform>
			{
				{ Attachment.Scope, mScopeAttach },
				{ Attachment.Barrel, mBarrelAttach },
				{ Attachment.Mechanism, mMechanismAttach },
				{ Attachment.Grip, mGripAttach }
			};

			mShotsInClip = new BoundProperty<int>();
			mTotalClipSize = new BoundProperty<int>();
			mShotParticles = transform.Find("shot_particles").GetComponent<ParticleSystem>();
			mAnimator = GetComponent<Animator>();

			mPartBreakPrefab = Resources.Load<GameObject>("prefabs/weapons/effects/p_vfxPartBreak").GetComponent<ParticleSystem>();
		}

		public override void OnStartClient()
		{
			EventManager.Local.OnEnterAimDownSightsMode += OnEnterAimDownSightsMode;
			EventManager.Local.OnExitAimDownSightsMode += OnExitAimDownSightsMode;
		}

		private void OnDestroy()
		{
			mShotsInClip.Cleanup();
			mTotalClipSize.Cleanup();
			EventManager.Local.OnEnterAimDownSightsMode -= OnEnterAimDownSightsMode;
			EventManager.Local.OnExitAimDownSightsMode -= OnExitAimDownSightsMode;
		}

		public void BindPropertiesToUI()
		{
			mShotsInClip = new BoundProperty<int>(mShotsInClip == null ? 0 : mShotsInClip.value, GameplayUIManager.CLIP_CURRENT);
			mTotalClipSize = new BoundProperty<int>(mTotalClipSize == null ? 0 : mTotalClipSize.value, GameplayUIManager.CLIP_TOTAL);
		}

		// [Client] AND [Server]
		private void Update()
		{
			// Follow my player
			if (bearer == null || bearer.eye == null)
				return;

			Vector3 location = transform.position;
			Vector3 targetLocation = bearer.eye.TransformPoint(positionOffset);

			Quaternion rot = transform.rotation;
			Quaternion targetRot = bearer.eye.rotation;

			transform.position = Vector3.Lerp(location, targetLocation, Time.deltaTime * CAMERA_FOLLOW_FACTOR);
			transform.rotation = Quaternion.Lerp(rot, targetRot, Time.deltaTime * CAMERA_FOLLOW_FACTOR);
		}

		#region Serialization

		// Todo: Optimize these to only send changes
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
				if ((flags & DirtyBitFlags.ScopeId) != 0)
				{
					currentParts.scope.SerializeId(writer);
					currentParts.scope.SerializeDurability(writer);
				}
				if ((flags & DirtyBitFlags.BarrelId) != 0)
				{
					currentParts.barrel.SerializeId(writer);
					currentParts.barrel.SerializeDurability(writer);
				}
				if ((flags & DirtyBitFlags.MechanismId) != 0)
				{
					currentParts.mechanism.SerializeId(writer);
					currentParts.mechanism.SerializeDurability(writer);
				}
				if ((flags & DirtyBitFlags.GripId) != 0)
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

		public override void OnDeserialize(NetworkReader reader, bool forceAll)
		{
			if (forceAll)
			{
				NetworkInstanceId bearerId = reader.ReadNetworkId();
				StartCoroutine(BindToBearer(bearerId));

				for (int i = 0; i < 4; i++)
				{
					string id = WeaponPartScript.DeserializeId(reader);
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
					string id = WeaponPartScript.DeserializeId(reader);
					int durability = WeaponPartScript.DeserializeDurability(reader);

					AttachNewPart(id, durability);
				}
				if ((flags & DirtyBitFlags.BarrelId) != 0)
				{
					string id = WeaponPartScript.DeserializeId(reader);
					int durability = WeaponPartScript.DeserializeDurability(reader);

					AttachNewPart(id, durability);
				}
				if ((flags & DirtyBitFlags.MechanismId) != 0)
				{
					string id = WeaponPartScript.DeserializeId(reader);
					int durability = WeaponPartScript.DeserializeDurability(reader);

					AttachNewPart(id, durability);
				}
				if ((flags & DirtyBitFlags.GripId) != 0)
				{
					string id = WeaponPartScript.DeserializeId(reader);
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

		public void ResetToDefaultParts()
		{
			foreach (WeaponPartScript p in bearer.defaultParts)
				AttachNewPart(p.partId, p.durability);
		}

		public void AttachNewPart(string partId, int durability = WeaponPartScript.USE_DEFAULT_DURABILITY)
		{
			if (string.IsNullOrEmpty(partId))
				return;

			WeaponPartScript prefab = ServiceLocator.Get<IWeaponPartManager>().GetPrefabScript(partId);
			SetDirtyBit(syncVarDirtyBits | (uint)GetBitFromAttach(prefab.attachPoint));
			WeaponPartScript instance = prefab.SpawnForWeapon(this);

			int originalClipsize = mCurrentData.clipSize;

			MoveAttachmentToPoint(instance);
			mCurrentParts[instance.attachPoint] = instance;

			instance.durability = durability == WeaponPartScript.USE_DEFAULT_DURABILITY ? prefab.durability : durability;

			mCurrentData = mAimDownSightsData = ActivatePartEffects(mCurrentParts, baseData);
			mAimDownSightsData.ForceModifyMinDispersion(new Modifier.Float(mAimDownSightsDispersionMod, Modifier.ModType.SetPercentage));
			mAimDownSightsData.ForceModifyMaxDispersion(new Modifier.Float(mAimDownSightsDispersionMod, Modifier.ModType.SetPercentage));

			if (instance.attachPoint == Attachment.Mechanism || mCurrentData.clipSize != originalClipsize)
			{
				if (mCurrentParts.mechanism == null)
					return;

				mShotsInClip.value = mCurrentData.clipSize;
				mTotalClipSize.value = mCurrentData.clipSize;
			}


			if (bearer != null)
			{
				ServiceLocator.Get<IAudioManager>().CreateSound(AudioEvent.EquipItem, transform);
				
				if (bearer.isCurrentPlayer)
					EventManager.Notify(() => EventManager.Local.LocalPlayerAttachedPart(this, instance));
			}

			if (instance.attachPoint == Attachment.Scope && mAimDownSightsActive)
				currentParts.scope.ActivateAimDownSightsEffect(this);
		}

		private void MoveAttachmentToPoint(WeaponPartScript instance)
		{
			Attachment place = instance.attachPoint;

			WeaponPartScript current = mCurrentParts[place];
			if (current != null)
				Destroy(current.gameObject);

			instance.transform.SetParent(mAttachPoints[place]);
			instance.transform.ResetLocalValues();
		}

		public static WeaponData ActivatePartEffects(WeaponPartCollection parts, WeaponData startingData, IEnumerable<WeaponPartData> otherVars = null)
		{
			WeaponData start = new WeaponData(startingData);

			if (otherVars != null)
				start = otherVars.Aggregate(start, (current, v) => new WeaponData(current, v));

			Action<WeaponPartScript> apply = part =>
			{
				foreach (WeaponPartData data in part.data)
					start = new WeaponData(start, data);
			};

			var partOrder = new[] { Attachment.Mechanism, Attachment.Barrel, Attachment.Scope, Attachment.Grip };

			foreach (Attachment part in partOrder)
			{
				if (parts[part] != null)
					apply(parts[part]);
			}

			return start;
		}

		#endregion

		#region Reloading

		public void Reload()
		{
			if (mReloading)
				return;

			mReloading = true;
			PlayReloadEffect(currentData.reloadTime);
			Invoke("FinishReload", currentData.reloadTime);
		}

		private void FinishReload()
		{
			mShotsInClip.value = currentData.clipSize;
			mReloading = false;
		}

		private void PlayReloadEffect(float time)
		{
			IAudioReference effect = ServiceLocator.Get<IAudioManager>().CreateSound(AudioEvent.Reload, transform, false);
			if (mCurrentParts.mechanism != null)
				effect.weaponType = mCurrentParts.mechanism.audioOverrideWeaponType;
			effect.AttachToRigidbody(bearer.gameObject.GetComponent<Rigidbody>()); // TODO: Cache this??
			effect.Start();

			AnimationUtility.PlayAnimation(mAnimator, "reload");
			StartCoroutine(WaitForReload(time));
		}

		private IEnumerator WaitForReload(float time)
		{
			mReloading = true;

			yield return null;
			yield return null;
			mAnimator.speed = 1.0f / time;
			yield return new WaitForAnimation(mAnimator);
			mAnimator.speed = 1.0f;

			mReloading = false;
			mShotsInClip.value = currentData.clipSize;
		}

		#endregion

		#region Weapon Firing

		public void FireWeaponHold()
		{
			TryFireShot();
		}

		public void FireWeaponUp()
		{
			mShotsSinceRelease = 0;
		}

		private void TryFireShot()
		{
			CleanupRecentShots();
			if (!CanFireShotNow())
				return;

			int count = mCurrentParts.barrel.projectileCount;
			var shots = new List<Ray>(count);
			for (int i = 0; i < count; i++)
				shots.Add(CalculateShotDirection(i == 0));

			mRecentShotTimes.Add(currentTime);
			mShotsSinceRelease++;
			mShotsInClip.value--;

			foreach (Ray shot in shots)
				CmdInstantiateShot(shot.origin, shot.direction);

			CmdOnShotFireComplete();
			PlayFireEffect();
			OnPostFireShot();
		}

		[Command]
		private void CmdInstantiateShot(Vector3 origin, Vector3 direction)
		{
			Ray shot = new Ray(origin, direction);

			GameObject projectile = Instantiate(mCurrentParts.mechanism.projectilePrefab, mCurrentParts.barrel.barrelTip.position,
				Quaternion.identity);
			projectile.GetComponent<IProjectile>().PreSpawnInitialize(this, shot, currentData);
			NetworkServer.Spawn(projectile);
			projectile.GetComponent<IProjectile>().PostSpawnInitialize(this, shot, currentData);
		}

		[Command]
		private void CmdOnShotFireComplete()
		{
			RpcReflectPlayerShotWeapon();
			EventManager.Server.PlayerFiredWeapon(bearer, null);
		}

		[ClientRpc]
		private void RpcReflectPlayerShotWeapon()
		{
			if (!bearer.isCurrentPlayer)
				PlayFireEffect();
		}

		private bool CanFireShotNow()
		{
			float lastShotTime = mRecentShotTimes.Count >= 1 ? mRecentShotTimes[mRecentShotTimes.Count - 1] : -1.0f;
			if (mReloading || currentTime - lastShotTime < timePerShot)
				return false;

			WeaponPartScriptBarrel barrel = mCurrentParts.barrel;
			if (barrel == null || barrel.shotsPerClick > 0 && mShotsSinceRelease >= barrel.shotsPerClick)
				return false;

			if (mShotsInClip.value <= 0)
			{
				Reload();
				return false;
			}

			return true;
		}

		private Ray CalculateShotDirection(bool firstShot)
		{
			float dispersionFactor = GetCurrentDispersionFactor(!firstShot);
			Vector3 randomness = Random.insideUnitSphere * dispersionFactor;

			Transform root = GetAimRoot();
			return new Ray(root.position, root.forward + randomness);
		}

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

		private Transform GetAimRoot()
		{
			if (!mCurrentParts.mechanism.overrideHitscanMethod && aimRoot != null)
				return aimRoot;

			return mCurrentParts.barrel.barrelTip;
		}

		private void OnPostFireShot()
		{
			CmdDegradeDurability();
		}

		[Command]
		private void CmdDegradeDurability()
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

		[Server]
		private void BreakPart(WeaponPartScript part)
		{
			RpcCreateBreakPartEffect();
			AttachNewPart(bearer.defaultParts[part.attachPoint].partId, WeaponPartScript.INFINITE_DURABILITY);

			// TODO: Send "break" event here (which will then spawn particles)
			// TODO: spawn "break" particle system here
		}

		[ClientRpc]
		private void RpcCreateBreakPartEffect()
		{
			ParticleSystem instance = Instantiate(mPartBreakPrefab.gameObject).GetComponent<ParticleSystem>();
			instance.transform.SetParent(transform);
			instance.transform.ResetLocalValues();
			instance.Play();

			StartCoroutine(Coroutines.WaitAndDestroyParticleSystem(instance));
		}

		#endregion

		#region Aim Down Sights

		[Client]
		public void OnEnterAimDownSightsMode()
		{
			if (!bearer.isCurrentPlayer)
				return;

			//AnimationUtility.SetVariable(mAnimator, "AimDownSights", true);
			//StartCoroutine(Coroutines.LerpPosition(mView, new Vector3(-0.33f, 0.0f, 0.0f), 0.2f, Space.Self, Coroutines.MATHF_SMOOTHSTEP));
			mAimDownSightsActive = true;
			if (mCurrentParts.scope != null)
				mCurrentParts.scope.ActivateAimDownSightsEffect(this);
		}

		[Client]
		public void OnExitAimDownSightsMode()
		{
			if (!bearer.isCurrentPlayer)
				return;

			//AnimationUtility.SetVariable(mAnimator, "AimDownSights", false);
			//StartCoroutine(Coroutines.LerpPosition(mView, new Vector3(0.0f, 0.0f, 0.0f), 0.2f, Space.Self, Coroutines.MATHF_SMOOTHSTEP));
			mAimDownSightsActive = false;
			if (mCurrentParts.scope != null)
				mCurrentParts.scope.DeactivateAimDownSightsEffect(this);
		}

		#endregion

		#region Data Management

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

		public void PlayFireEffect()
		{
			bearer.PlayFireAnimation();
			mShotParticles.transform.position = currentParts.barrel.barrelTip.position;
			mShotParticles.Play();

			IAudioReference effect = ServiceLocator.Get<IAudioManager>().CreateSound(AudioEvent.Shoot, transform, false);
			effect.weaponType = mCurrentParts.mechanism.audioOverrideWeaponType;
			effect.Start();
		}

		#endregion
	}
}
