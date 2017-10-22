using System.Collections.Generic;
using FiringSquad.Data;
using FiringSquad.Gameplay.Weapons;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay.NPC
{
	public class NpcTurret : NetworkBehaviour, IWeaponBearer
	{
		[SerializeField] private AudioProfile mAudioProfile;
		public AudioProfile audioProfile { get { return mAudioProfile; } }

		[SerializeField] private WeaponPartCollection mParts;
		public WeaponPartCollection defaultParts { get { return mParts; } }

		[SerializeField] private Transform mWeaponAttachPoint;
		[SerializeField] private GameObject mBaseWeaponPrefab;

		private Dictionary<int, int> x;

		public IWeapon weapon { get; private set; }
		public Transform eye { get { return transform; } }

		public bool isCurrentPlayer { get { return false; } }

		public override void OnStartServer()
		{
			// create our weapon & bind
			BaseWeaponScript wep = Instantiate(mBaseWeaponPrefab).GetComponent<BaseWeaponScript>();
			BindWeaponToPlayer(wep);
			AddDefaultPartsToWeapon(wep);
			NetworkServer.Spawn(wep.gameObject);
		}

		public void BindWeaponToPlayer(BaseWeaponScript wep)
		{
			// find attach spot in view and set parent
			wep.transform.SetParent(mWeaponAttachPoint);
			wep.transform.ResetLocalValues();
			wep.positionOffset = transform.InverseTransformPoint(mWeaponAttachPoint.position);
			wep.transform.SetParent(transform);
			wep.bearer = this;
			weapon = wep;
		}

		[ServerCallback]
		private void Update()
		{
			weapon.FireWeaponHold();
		}

		private void AddDefaultPartsToWeapon(IWeapon wep)
		{
			foreach (WeaponPartScript part in defaultParts)
				wep.AttachNewPart(part.partId, WeaponPartScript.INFINITE_DURABILITY);
		}

		public void PlayFireAnimation()
		{
			// ignore for now
		}
	}
}
