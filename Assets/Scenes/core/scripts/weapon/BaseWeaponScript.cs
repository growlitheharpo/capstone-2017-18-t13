using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using FiringSquad.Data;
using FiringSquad.Gameplay;
using UnityEngine;
using UnityEngine.Networking;

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

	[SerializeField] private WeaponData mDefaultData;
	public WeaponData baseData { get { return mDefaultData; } }

	private WeaponPartCollection mCurrentParts;

	private WeaponData mCurrentData;
	private float timePerShot { get { return 1.0f / mCurrentData.fireRate; } }

	private bool mReloading;
	private int mShotsInClip;
	private int mShotsSinceRelease;
	private List<float> mRecentShotTimes;

	private const float CAMERA_FOLLOW_FACTOR = 10.0f;

	[ServerCallback]
	private void Awake()
	{
		mCurrentData = new WeaponData(baseData);
		mRecentShotTimes = new List<float>();
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

	// Todo: Write the parent and weapon variables here
	// Todo: Optimize these to only send changes
	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		// write our bearer
		writer.Write(realBearer.netId);

		if (isServer)
			CleanupRecentShots();

		// serialize our times
		MemoryStream memstream = new MemoryStream();
		new BinaryFormatter().Serialize(memstream, mRecentShotTimes);
		writer.WriteBytesAndSize(memstream.ToArray(), memstream.ToArray().Length);

		// serialize our weapon parts

		return true;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		// read our bearer
		NetworkInstanceId bearerId = reader.ReadNetworkId();
		if (realBearer == null || realBearer.netId != bearerId)
		{
			GameObject bearerObj = ClientScene.FindLocalObject(bearerId);
			if (bearerObj != null)
				bearerObj.GetComponent<CltPlayer>().BindWeaponToPlayer(this);
		}

		// read our times
		var bytearray = reader.ReadBytesAndSize();
		BinaryFormatter binFormatter = new BinaryFormatter();
		mRecentShotTimes = binFormatter.Deserialize(new MemoryStream(bytearray)) as List<float>;

		// read our weapon parts
	}

	#endregion

	#region Part Attachment



	#endregion

	#region Reloading

	[Server]
	public void Reload()
	{
		if (mReloading)
			return;

		mReloading = true;
		RpcPlayReloadEffect(mCurrentData.reloadTime);

		Invoke("FinishReload", mCurrentData.reloadTime);
	}

	[Server]
	private void FinishReload()
	{
		mReloading = false;
	}

	#endregion

	#region Weapon Firing

	[Server]
	public void FireWeaponHold()
	{
		CleanupRecentShots();
		if (!CanFireShotNow())
			return;

		int count = mCurrentParts.barrel.projectileCount;
		var shots = new List<Ray>(count);
		for (int i = 0; i < count; i++)
			shots.Add(CalculateShotDirection(i == 0));

		mRecentShotTimes.Add(Time.time);
		mShotsSinceRelease++;
		mShotsInClip--;

		// Todo: create the projectile here

		EventManager.Server.PlayerFiredWeapon(realBearer, shots);

		OnPostFireShot();
	}

	[Server]
	public void FireWeaponUp()
	{
		mShotsSinceRelease = 0;
	}

	[Server]
	private bool CanFireShotNow()
	{
		float lastShotTime = mRecentShotTimes.Count >= 1 ? mRecentShotTimes[mRecentShotTimes.Count - 1] : -1.0f;
		if (mReloading || Time.time - lastShotTime < timePerShot)
			return false;

		WeaponPartScriptBarrel barrel = mCurrentParts.barrel;
		if (barrel == null || (barrel.shotsPerClick > 0 && mShotsSinceRelease >= barrel.shotsPerClick))
			return false;

		if (mShotsInClip <= 0)
		{
			Reload();
			return false;
		}

		return true;
	}

	[Server]
	private Ray CalculateShotDirection(bool firstShot)
	{
		float dispersionFactor = GetCurrentDispersionFactor(forceNotZero: !firstShot);
		Vector3 randomness = Random.insideUnitSphere * dispersionFactor;

		Transform root = GetAimRoot();
		return new Ray(root.position, root.forward + randomness);
	}

	[Server]
	private float GetCurrentDispersionFactor(bool forceNotZero)
	{
		float percentage = 0.0f;
		float inverseFireRate = 1.0f / mCurrentData.fireRate;

		foreach (float shot in mRecentShotTimes)
		{
			float timeSinceShot = Time.time - shot;
			if (timeSinceShot > inverseFireRate * 2.0f)
				continue;

			float p = Mathf.Pow(Mathf.Clamp(inverseFireRate / timeSinceShot, 0.0f, 1.0f), 2);
			percentage += p * mCurrentData.dispersionRamp;
		}

		if (!forceNotZero && percentage <= 0.005f)
			return 0.0f;

		return Mathf.Lerp(mCurrentData.minimumDispersion, mCurrentData.maximumDispersion, percentage);
	}

	[Server]
	private Transform GetAimRoot()
	{
		if (!mCurrentParts.mechanism.overrideHitscanMethod && aimRoot != null)
			return aimRoot;

		return mCurrentParts.barrel.barrelTip;
	}

	[Server]
	private void OnPostFireShot()
	{
		DegradeDurability();
	}

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
	}

	[Server]
	private void BreakPart(WeaponPartScript part)
	{
		GameObject defaultPart = bearer.defaultParts.gameObjects[part.attachPoint];
		GameObject instance = Instantiate(defaultPart); 

		// TODO: override durability to infinite

		instance.name = defaultPart.name;
		// TODO: equip here

		// TODO: Send "break" event here (which will then spawn particles)
		// TODO: spawn "break" particle system here
	}

	#endregion

	#region Data Management

	[Server]
	private void CleanupRecentShots()
	{
		float inverseFireRate = (1.0f / mCurrentData.fireRate) * 10.0f;

		for (int i = 0; i < mRecentShotTimes.Count; i++)
		{
			float timeSinceShot = Time.time - mRecentShotTimes[i];

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
			float timeSinceShot = Time.time - v;
			float percent = Mathf.Clamp(timeSinceShot / mCurrentData.recoilTime, 0.0f, 1.0f);
			float sample = mCurrentData.recoilCurve.Evaluate(percent);
			value += sample;
		}

		return value * mCurrentData.recoilAmount;
	}

	[ClientRpc]
	private void RpcPlayReloadEffect(float time)
	{
		AnimationUtility.PlayAnimation(gameObject, "reload");
		StartCoroutine(WaitForReload(time));
	}

	[Client]
	private IEnumerator WaitForReload(float time)
	{
		yield return null;
		Animator anim = GetComponent<Animator>();
		anim.speed = 1.0f / time;
		yield return new WaitForAnimation(anim);
		anim.speed = 1.0f;
	}

	#endregion
}
