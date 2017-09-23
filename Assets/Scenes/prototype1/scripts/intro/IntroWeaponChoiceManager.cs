using System;
using System.Collections;
using FiringSquad.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FiringSquad.Gameplay
{
	public class IntroWeaponChoiceManager : MonoBehaviour
	{
		[SerializeField] private WeaponDefaultsData mDefaultParts;

		private BoundProperty<float> mSpread, mRecoil, mReload, mFireRate, mDamage, mClipsize;
		private GameObject mScope, mBarrel, mMechanism, mGrip;
		private DemoWeaponScript mDemoWeapon;

		private void Awake()
		{
			mScope = mDefaultParts.scope;
			mBarrel = mDefaultParts.barrel;
			mMechanism = mDefaultParts.mechanism;
			mGrip = mDefaultParts.grip;

			mSpread = new BoundProperty<float>(0.0f, "IntroSpread".GetHashCode());
			mRecoil = new BoundProperty<float>(0.0f, "IntroRecoil".GetHashCode());
			mReload = new BoundProperty<float>(0.0f, "IntroReloadTime".GetHashCode());
			mFireRate = new BoundProperty<float>(0.0f, "IntroFireRate".GetHashCode());
			mDamage = new BoundProperty<float>(0.0f, "IntroDamage".GetHashCode());
			mClipsize = new BoundProperty<float>(0.0f, "IntroClipSize".GetHashCode());
		}

		private void Start()
		{
			DontDestroyOnLoad(gameObject);
			SceneManager.activeSceneChanged += HandleSceneChange;
			mDemoWeapon = FindObjectOfType<DemoWeaponScript>();

			foreach (GameObject part in mDefaultParts)
				Instantiate(part).GetComponent<WeaponPickupScript>().ConfirmAttach(mDemoWeapon);
		}

		public void AttachPart(GameObject part)
		{
			Instantiate(part).GetComponent<WeaponPickupScript>().ConfirmAttach(mDemoWeapon);
			BaseWeaponScript.Attachment type = part.GetComponent<WeaponPartScript>().attachPoint;

			switch (type)
			{
				case BaseWeaponScript.Attachment.Scope:
					mScope = part;
					break;
				case BaseWeaponScript.Attachment.Barrel:
					mBarrel = part;
					break;
				case BaseWeaponScript.Attachment.Mechanism:
					mMechanism = part;
					break;
				case BaseWeaponScript.Attachment.Grip:
					mGrip = part;
					break;
			}
		}

		private void HandleSceneChange(Scene arg0, Scene arg1)
		{
			if (arg1.name == GamestateManager.PROTOTYPE1_SCENE)
			{
				ReferenceForwarder.get.player.GetComponent<PlayerScript>().OverrideDefaultParts(mMechanism, mBarrel, mScope, mGrip);
				UnityEngine.Debug.Log("Destroying because we think we applied our settings.");
				SceneManager.activeSceneChanged -= HandleSceneChange;
				Destroy(gameObject);
			}
			else if (arg1.name != GamestateManager.BASE_WORLD && arg1.name != GamestateManager.PROTOTYPE1_SETUP_SCENE)
			{
				UnityEngine.Debug.Log("Destroying because we're not in base world.");
				SceneManager.activeSceneChanged -= HandleSceneChange;
				Destroy(gameObject);
			}
		}

		private void Update()
		{
			WeaponData data = mDemoWeapon.currentStats;

			mSpread.value = data.spread;
			mRecoil.value = data.recoil;
			mReload.value = data.reloadTime;
			mFireRate.value = data.fireRate;
			mDamage.value = data.damage;
			mClipsize.value = data.clipSize;
		}
	}
}
