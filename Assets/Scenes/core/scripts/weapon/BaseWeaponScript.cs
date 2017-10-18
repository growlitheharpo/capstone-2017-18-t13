using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using FiringSquad.Data;
using FiringSquad.Gameplay;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class BaseWeaponScript : NetworkBehaviour, IWeapon
{
	public static class DebugHelper
	{
		public static WeaponData GetWeaponData(BaseWeaponScript p)
		{
			return new WeaponData(p.mCurrentData);
		}

		public static WeaponPartCollection GetAttachments(BaseWeaponScript p)
		{
			return new WeaponPartCollection(p.mCurrentParts);
		}

		public static Transform GetWeaponAimRoot(BaseWeaponScript p, bool forceBarrel = false)
		{
			return !forceBarrel ? p.GetAimRoot() : p.mCurrentParts.barrel.barrelTip;
		}

		public static float GetCurrentDispersion(BaseWeaponScript p)
		{
			return p.GetCurrentDispersionFactor(false);
		}
	}

	public enum Attachment
	{
		Scope,
		Barrel,
		Mechanism,
		Grip,
	}

	public IWeaponBearer bearer { get; set; }
	private CltPlayer realBearer { get { return bearer as CltPlayer; } }
	public Transform aimRoot { get; set; }
	public Vector3 positionOffset { get; set; }

	[SerializeField] private AudioProfile mAudioProfile;
	private AudioProfile audioProfile
	{
		get
		{
			if (mCurrentParts.mechanism != null && mCurrentParts.mechanism.audioOverride != null)
				return mCurrentParts.mechanism.audioOverride;
			return mAudioProfile;
		}
	}

	[SerializeField] private Transform mBarrelAttach;
	[SerializeField] private Transform mScopeAttach;
	[SerializeField] private Transform mMechanismAttach;
	[SerializeField] private Transform mGripAttach;
	[SerializeField] private WeaponData mDefaultData;
	public WeaponData baseData { get { return mDefaultData; } }

	private WeaponPartCollection mCurrentParts;
	public WeaponPartCollection currentParts { get { return mCurrentParts; } }

	private Dictionary<Attachment, Transform> mAttachPoints;
	private WeaponData mCurrentData;
	public WeaponData currentData { get { return mCurrentData; } }
	private float timePerShot { get { return 1.0f / mCurrentData.fireRate; } }

	private bool mReloading;
	private BoundProperty<int> mShotsInClip, mTotalClipSize;
	private int mShotsSinceRelease;
	private List<float> mRecentShotTimes;
	private ParticleSystem mShotParticles;

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
			{ Attachment.Grip, mGripAttach },
		};

		mShotsInClip = new BoundProperty<int>();
		mTotalClipSize = new BoundProperty<int>();
		mShotParticles = transform.Find("shot_particles").GetComponent<ParticleSystem>();
	}

	private void OnDestroy()
	{
		mShotsInClip.Cleanup();
		mTotalClipSize.Cleanup();
	}

	public void BindPropertiesToUI()
	{
		mShotsInClip = new BoundProperty<int>(mShotsInClip == null ? 0 : mShotsInClip.value, GameplayUIManager.CLIP_CURRENT);
		mTotalClipSize = new BoundProperty<int>(mTotalClipSize == null ? 0 : mTotalClipSize.value, GameplayUIManager.CLIP_TOTAL);
	}

	// [Client] AND [Server]
	private void Update()
	{
		SetDirtyBit(99999);

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
	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		BinaryFormatter bf = new BinaryFormatter();
		using (MemoryStream memstream = new MemoryStream())
		{
			// write our bearer
			writer.Write(realBearer.netId);

			// serialize our part ids
			var partIds = mCurrentParts.allParts.Select(x => x.partId).ToArray();
			bf.Serialize(memstream, partIds);
			writer.WriteBytesAndSize(memstream.ToArray(), memstream.ToArray().Length);
			memstream.Dispose();
		}

		return true;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		BinaryFormatter binFormatter = new BinaryFormatter();

		// read our bearer
		NetworkInstanceId bearerId = reader.ReadNetworkId();
		if (realBearer == null || realBearer.netId != bearerId)
		{
			GameObject bearerObj = ClientScene.FindLocalObject(bearerId);
			if (bearerObj != null)
				bearerObj.GetComponent<CltPlayer>().BindWeaponToPlayer(this);
		}

		// read our weapon parts
		var bytearray = reader.ReadBytesAndSize();
		var partList = (string[])binFormatter.Deserialize(new MemoryStream(bytearray));
		if (mCurrentParts == null)
			mCurrentParts = new WeaponPartCollection();

		if (mCurrentParts.scope == null || mCurrentParts.scope.partId != partList[0])
			AttachNewPart(partList[0], true);
		if (mCurrentParts.barrel == null || mCurrentParts.barrel.partId != partList[1])
			AttachNewPart(partList[1], true);
		if (mCurrentParts.mechanism == null || mCurrentParts.mechanism.partId != partList[2])
			AttachNewPart(partList[2], true);
		if (mCurrentParts.grip == null || mCurrentParts.grip.partId != partList[3])
			AttachNewPart(partList[3], true);
	}

	#endregion

	#region Part Attachment

	public void AttachNewPart(string partId)
	{
		AttachNewPart(partId, false);
	}

	public void ResetToDefaultParts()
	{
		foreach (WeaponPartScript p in bearer.defaultParts)
			AttachNewPart(p.partId, true);
	}

	public void AttachNewPart(string partId, bool forceInfiniteDurability)
	{
		if (string.IsNullOrEmpty(partId))
			return;

		GameObject prefab = ServiceLocator.Get<IWeaponPartManager>().GetPartPrefab(partId);
		WeaponPartScript instance = prefab.GetComponent<WeaponPartScript>().SpawnForWeapon(this);

		int originalClipsize = mCurrentData.clipSize;

		MoveAttachmentToPoint(instance);
		mCurrentParts[instance.attachPoint] = instance;

		if (forceInfiniteDurability)
			instance.durability = WeaponPartScript.INFINITE_DURABILITY;

		mCurrentData = ActivatePartEffects(mCurrentParts, baseData);

		if (instance.attachPoint == Attachment.Mechanism || mCurrentData.clipSize != originalClipsize)
		{
			if (mCurrentParts.mechanism == null)
				return;

			mShotsInClip.value = mCurrentData.clipSize;
			mTotalClipSize.value = mCurrentData.clipSize;
		}

		EventManager.Notify(() => EventManager.Local.LocalPlayerAttachedPart(this, instance));
		if (realBearer !=  null && realBearer.audioProfile != null)
		{
			ServiceLocator.Get<IAudioManager>()
				.PlaySound(AudioManager.AudioEvent.InteractReceive, realBearer.audioProfile, transform);
		}
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

	public static WeaponData ActivatePartEffects(WeaponPartCollection parts, WeaponData startingData)
	{
		WeaponData start = new WeaponData(startingData);

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
		PlayReloadEffect(mCurrentData.reloadTime);
		Invoke("FinishReload", mCurrentData.reloadTime);
	}

	private void FinishReload()
	{
		mShotsInClip.value = mCurrentData.clipSize;
		mReloading = false;
	}

	private void PlayReloadEffect(float time)
	{
		ServiceLocator.Get<IAudioManager>()
			.PlaySound(AudioManager.AudioEvent.Reload, audioProfile, transform);

		AnimationUtility.PlayAnimation(gameObject, "reload");
		StartCoroutine(WaitForReload(time));
	}

	private IEnumerator WaitForReload(float time)
	{
		mReloading = true;

		yield return null;
		yield return null;
		Animator anim = GetComponent<Animator>();
		anim.speed = 1.0f / time;
		yield return new WaitForAnimation(anim);
		anim.speed = 1.0f;

		mReloading = false;
		mShotsInClip.value = mCurrentData.clipSize;
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

		realBearer.localAnimator.SetTrigger("Fire");
		realBearer.networkAnimator.SetTrigger("Fire");
		CmdOnShotFireComplete();
		PlayFireEffect();
		OnPostFireShot();
	}

	[Command]
	private void CmdInstantiateShot(Vector3 origin, Vector3 direction)
	{
		Ray shot = new Ray(origin, direction);

		GameObject projectile = Instantiate(mCurrentParts.mechanism.projectilePrefab, mCurrentParts.barrel.barrelTip.position, Quaternion.identity);
		projectile.GetComponent<IProjectile>().PreSpawnInitialize(this, shot, mCurrentData);
		NetworkServer.Spawn(projectile);
		projectile.GetComponent<IProjectile>().PostSpawnInitialize(this, shot, mCurrentData);
	}

	[Command]
	private void CmdOnShotFireComplete()
	{
		EventManager.Server.PlayerFiredWeapon(realBearer, null);
	}

	private bool CanFireShotNow()
	{
		float lastShotTime = mRecentShotTimes.Count >= 1 ? mRecentShotTimes[mRecentShotTimes.Count - 1] : -1.0f;
		if (mReloading || currentTime - lastShotTime < timePerShot)
			return false;

		WeaponPartScriptBarrel barrel = mCurrentParts.barrel;
		if (barrel == null || (barrel.shotsPerClick > 0 && mShotsSinceRelease >= barrel.shotsPerClick))
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
		float dispersionFactor = GetCurrentDispersionFactor(forceNotZero: !firstShot);
		Vector3 randomness = Random.insideUnitSphere * dispersionFactor;

		Transform root = GetAimRoot();
		return new Ray(root.position, root.forward + randomness);
	}

	private float GetCurrentDispersionFactor(bool forceNotZero)
	{
		float percentage = 0.0f;
		float inverseFireRate = 1.0f / mCurrentData.fireRate;

		foreach (float shot in mRecentShotTimes)
		{
			float timeSinceShot = currentTime - shot;
			if (timeSinceShot > inverseFireRate * 2.0f)
				continue;

			float p = Mathf.Pow(Mathf.Clamp(inverseFireRate / timeSinceShot, 0.0f, 1.0f), 2);
			percentage += p * mCurrentData.dispersionRamp;
		}

		if (!forceNotZero && percentage <= 0.005f)
			return 0.0f;

		return Mathf.Lerp(mCurrentData.minimumDispersion, mCurrentData.maximumDispersion, percentage);
	}

	private Transform GetAimRoot()
	{
		if (!mCurrentParts.mechanism.overrideHitscanMethod && aimRoot != null)
			return aimRoot;

		return mCurrentParts.barrel.barrelTip;
	}

	private void OnPostFireShot()
	{
		DegradeDurability();
	}

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
	}

	private void BreakPart(WeaponPartScript part)
	{
		AttachNewPart(bearer.defaultParts[part.attachPoint].partId);

		// TODO: Send "break" event here (which will then spawn particles)
		// TODO: spawn "break" particle system here
	}

	#endregion

	#region Data Management
	
	private void CleanupRecentShots()
	{
		float inverseFireRate = (1.0f / mCurrentData.fireRate) * 10.0f;

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
			float percent = Mathf.Clamp(timeSinceShot / mCurrentData.recoilTime, 0.0f, 1.0f);
			float sample = mCurrentData.recoilCurve.Evaluate(percent);
			value += sample;
		}

		return value * mCurrentData.recoilAmount;
	}
	
	public void PlayFireEffect()
	{
		mShotParticles.transform.position = currentParts.barrel.barrelTip.position;
		mShotParticles.Play();

		ServiceLocator.Get<IAudioManager>()
			.PlaySound(AudioManager.AudioEvent.Shoot, audioProfile, transform);
	}

	#endregion
}
