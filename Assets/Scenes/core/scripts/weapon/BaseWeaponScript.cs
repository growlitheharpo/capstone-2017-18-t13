using System.Collections.Generic;
using FiringSquad.Data;
using FiringSquad.Gameplay;
using UnityEngine;
using UnityEngine.Networking;

public abstract class BaseWeaponScript : NetworkBehaviour
{
	public enum Attachment
	{
		Scope,
		Barrel,
		Mechanism,
		Grip,
	}

	public abstract CltPlayer bearer { get; }

	[SerializeField] private WeaponData mDefaultData;
	public WeaponData baseData { get { return mDefaultData; } }

	public abstract void AttachNewPart(WeaponPartScript part);
	public abstract void FireShotImmediate(List<Ray> shotDirections);
	public abstract float GetCurrentRecoil();
	public abstract void Reload();

	// Todo: Write the parent and weapon variables here

	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		return base.OnSerialize(writer, initialState);
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}
}
