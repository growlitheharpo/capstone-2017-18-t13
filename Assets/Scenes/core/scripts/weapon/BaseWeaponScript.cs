using System.Collections.Generic;
using FiringSquad.Data;
using FiringSquad.Gameplay;
using UnityEngine;
using UnityEngine.Networking;

public class BaseWeaponScript : NetworkBehaviour, IWeapon
{
	public enum Attachment
	{
		Scope,
		Barrel,
		Mechanism,
		Grip,
	}

	public IWeaponBearer bearer { get; set; }
	private CltPlayer realBearer { get { return bearer as CltPlayer; } }

	[SerializeField] private WeaponData mDefaultData;
	public WeaponData baseData { get { return mDefaultData; } }

	private WeaponPartCollection mCurrentParts;

	private WeaponData mCurrentData;
	private float timePerShot { get { return 1.0f / mCurrentData.fireRate; } }

	private bool mReloading;
	private int mShotsInClip;
	private int mShotsSinceRelease;
	private List<float> mRecentShotTimes;

	[ServerCallback]
	private void Awake()
	{
		mCurrentData = new WeaponData(baseData);
		mRecentShotTimes = new List<float>();
	}

	#region Serialization

	// Todo: Write the parent and weapon variables here
	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		writer.Write(realBearer.netId);
		return true;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		NetworkInstanceId bearerId = reader.ReadNetworkId();
		if (realBearer == null || realBearer.netId != bearerId)
		{
			GameObject bearerObj = ClientScene.FindLocalObject(bearerId);
			if (bearerObj != null)
				bearerObj.GetComponent<CltPlayer>().BindWeaponToPlayer(this);
		}

		base.OnDeserialize(reader, initialState);
	}

	#endregion

	#region Part Attachment



	#endregion

	#region Reloading

	[Server]
	public void Reload()
	{
		
	}

	#endregion

	#region Weapon Firing

	[Server]
	public void FireWeaponHold()
	{
		float lastShotTime = mRecentShotTimes.Count >= 1 ? mRecentShotTimes[mRecentShotTimes.Count - 1] : -1.0f;
		if (mReloading || Time.time - lastShotTime < timePerShot)
			return;

		WeaponPartScriptBarrel barrel = mCurrentParts.barrel;
		if (barrel == null || (barrel.shotsPerClick > 0 && mShotsSinceRelease >= barrel.shotsPerClick))
			return;

		if (mShotsInClip <= 0)
		{
			Reload();
			return;
		}

		int count = barrel.projectileCount;
		var shots = new List<Ray>(count);
		for (int i = 0; i < count; i++)
			shots.Add(CalculateShotDirection(i == 0));

		mRecentShotTimes.Add(Time.time);
		mShotsSinceRelease++;
		mShotsInClip--;

		// Create the projectile

		EventManager.Server.PlayerFiredWeapon(realBearer, shots);
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
	public void FireWeaponUp()
	{
		mShotsSinceRelease = 0;
	}

	#endregion

	#region Data Management

	private void CleanupRecentShots()
	{
		// do some data manipulation
	}

	#endregion
}
