﻿using System.Collections.Generic;
using FiringSquad.Core;
using FiringSquad.Core.UI;
using FiringSquad.Core.Weapons;
using FiringSquad.Data;
using FiringSquad.Gameplay.UI;
using FiringSquad.Gameplay.Weapons;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay
{
	public class CltPlayer : NetworkBehaviour, IWeaponBearer, IDamageReceiver
	{
		[SerializeField] private PlayerAssetReferences mAssets;
		[SerializeField] private PlayerDefaultsData mInformation;

		[Header("References")] [SerializeField] private Transform mCameraOffset;
		[SerializeField] private Transform mGun1Offset;
		[SerializeField] private Transform mGun2Offset;
		[SerializeField] private Animator mAnimator;
		[SerializeField] private NetworkAnimator mNetworkAnimator;

		public Animator localAnimator { get { return mAnimator; } }
		public NetworkAnimator networkAnimator { get { return mNetworkAnimator; } }

		public bool isCurrentPlayer { get { return isLocalPlayer; } }

		public IWeapon weapon { get; private set; }
		public WeaponPartCollection defaultParts { get { return mInformation.defaultWeaponParts; } }
		public AudioProfile audioProfile { get { return mInformation.audioProfile; } }
		public Transform eye { get { return mCameraOffset; } }

		private PlayerMagnetArm mMagnetArm;
		public PlayerMagnetArm magnetArm { get { return mMagnetArm; } }

		private IPlayerHitIndicator mHitIndicator;
		private CharacterController mCharacterController;
		private CltPlayerLocal mLocalPlayerScript;

		[SyncVar(hook = "OnHealthUpdate")] private float mHealth;
		private BoundProperty<float> mLocalHealthVar;

		[SyncVar(hook = "OnKillsUpdate")] private int mKills;
		private BoundProperty<int> mLocalKillsVar;

		[SyncVar(hook = "OnDeathsUpdate")] private int mDeaths;
		private BoundProperty<int> mLocalDeathsVar;

		#region Unity Callbacks

		public override void OnStartServer()
		{
			base.OnStartServer();

			// register for server events
			EventManager.Server.OnPlayerFiredWeapon += OnPlayerFiredWeapon;
			EventManager.Server.OnPlayerDied += OnPlayerDied;

			EventManager.Server.OnStartGame += OnStartGame;
			EventManager.Server.OnFinishGame += OnFinishGame;

			mHitIndicator = new NullHitIndicator();
			mHealth = mInformation.defaultHealth;

			// create our weapon & bind
			BaseWeaponScript wep = Instantiate(mAssets.baseWeaponPrefab).GetComponent<BaseWeaponScript>();
			BindWeaponToPlayer(wep, true);
			AddDefaultPartsToWeapon(wep);
			NetworkServer.SpawnWithClientAuthority(wep.gameObject, gameObject);

			// create our magnet arm & bind
			PlayerMagnetArm arm = Instantiate(mAssets.gravityGunPrefab).GetComponent<PlayerMagnetArm>();
			BindMagnetArmToPlayer(arm);
			NetworkServer.SpawnWithClientAuthority(arm.gameObject, gameObject);
		}

		public override void OnStartClient()
		{
			// force the lazy initialization of the part list
			ServiceLocator.Get<IWeaponPartManager>().GetAllPrefabs(false);

			base.OnStartClient();
			mCharacterController = GetComponent<CharacterController>();

			GameObject hitObject = new GameObject("HitIndicator");
			hitObject.transform.SetParent(transform);
			mHitIndicator = hitObject.AddComponent<RemotePlayerHitIndicator>();

			mLocalHealthVar = new BoundProperty<float>(mInformation.defaultHealth);
		}

		public override void OnStartLocalPlayer()
		{
			mLocalPlayerScript = Instantiate(mAssets.localPlayerPrefab).GetComponent<CltPlayerLocal>();
			mLocalPlayerScript.transform.SetParent(transform);
			mLocalPlayerScript.playerRoot = this;

			mHitIndicator = (IPlayerHitIndicator)FindObjectOfType<LocalPlayerHitIndicator>() ?? new NullHitIndicator();
			mLocalHealthVar = new BoundProperty<float>(mInformation.defaultHealth, GameplayUIManager.PLAYER_HEALTH);
			mLocalKillsVar = new BoundProperty<int>(0, GameplayUIManager.PLAYER_KILLS);
			mLocalDeathsVar = new BoundProperty<int>(0, GameplayUIManager.PLAYER_DEATHS);

			var renderers = mAnimator.transform.GetComponentsInChildren<Renderer>();
			foreach (Renderer r in renderers)
				Destroy(r);

			EventManager.Notify(() => EventManager.Local.LocalPlayerSpawned(this));
		}

		private void OnDestroy()
		{
			EventManager.Server.OnPlayerFiredWeapon -= OnPlayerFiredWeapon;
			EventManager.Server.OnPlayerDied -= OnPlayerDied;
			EventManager.Server.OnStartGame -= OnStartGame;
			EventManager.Server.OnFinishGame -= OnFinishGame;

			CltPlayerLocal localPlayer = mLocalPlayerScript;
			if (localPlayer != null)
				localPlayer.CleanupCamera();

			if (mLocalHealthVar != null)
				mLocalHealthVar.Cleanup();

			if (mLocalKillsVar != null)
				mLocalKillsVar.Cleanup();

			if (mLocalDeathsVar != null)
				mLocalDeathsVar.Cleanup();
		}

		[ClientCallback]
		private void Update()
		{
			UpdateAnimations();
		}

		#endregion

		[Command]
		public void CmdActivateInteract(Vector3 eyePosition, Vector3 eyeForward)
		{
			IInteractable interactable = null;

			if (magnetArm != null)
				interactable = magnetArm.heldWeaponPart;

			if (interactable == null)
			{
				RaycastHit hit;

				Ray ray = new Ray(eyePosition, eyeForward);
				if (!Physics.Raycast(ray, out hit, mInformation.interactDistance))
					return;

				interactable = hit.GetInteractableComponent();
			}

			if (interactable != null)
				interactable.Interact(this);
		}

		#region Animations

		[Client]
		private void UpdateAnimations()
		{
			Vector3 relativeVel = mCharacterController.velocity / 6; // 6 is the MOVEMENTDATA SPEED
			relativeVel = transform.InverseTransformDirection(relativeVel);
			Vector2 vel = new Vector2(relativeVel.x, relativeVel.z);

			float velX = Mathf.Lerp(AnimationUtility.GetFloat(mAnimator, "VelocityX"), vel.x, Time.deltaTime * 3.0f);
			float velY = Mathf.Lerp(AnimationUtility.GetFloat(mAnimator, "VelocityY"), vel.y, Time.deltaTime * 3.0f);

			AnimationUtility.SetVariable(mAnimator, "VelocityX", velX);
			AnimationUtility.SetVariable(mAnimator, "VelocityY", velY);
		}

		#endregion

		#region Weapons

		public void BindWeaponToPlayer(BaseWeaponScript wep, bool bindUI = false)
		{
			// find attach spot in view and set parent
			wep.transform.SetParent(mGun1Offset);
			wep.aimRoot = eye;
			wep.transform.ResetLocalValues();
			wep.positionOffset = eye.InverseTransformPoint(mGun1Offset.position);
			wep.transform.SetParent(transform);
			wep.bearer = this;

			if (bindUI || isCurrentPlayer)
				wep.BindPropertiesToUI();

			weapon = wep;
		}

		public void BindMagnetArmToPlayer(PlayerMagnetArm arm)
		{
			arm.transform.SetParent(mGun2Offset);
			arm.transform.ResetLocalValues();
			arm.transform.SetParent(transform);
			arm.bearer = this;

			mMagnetArm = arm;
		}

		private void AddDefaultPartsToWeapon(BaseWeaponScript wep)
		{
			foreach (WeaponPartScript part in defaultParts)
				wep.AttachNewPart(part.partId, true);
		}

		[Command]
		public void CmdDebugEquipWeaponPart(string part)
		{
			weapon.AttachNewPart(part);
		}

		[Server]
		[EventHandler]
		private void OnPlayerFiredWeapon(CltPlayer p, List<Ray> shots)
		{
			if (p != this)
				RpcReflectPlayerShotWeapon(p.netId);
		}

		[ClientRpc]
		private void RpcReflectPlayerShotWeapon(NetworkInstanceId playerId)
		{
			if (playerId == netId)
				return;

			CltPlayer p = ClientScene.FindLocalObject(playerId).GetComponent<CltPlayer>();
			p.weapon.PlayFireEffect();
		}

		#endregion

		#region GameState

		[Server]
		public void MoveToStartPosition(Vector3 position, Quaternion rotation)
		{
			RpcResetPlayerValues(position, rotation);
		}

		[Server]
		[EventHandler]
		private void OnStartGame(long gameEndTime)
		{
			RpcHandleStartGame(gameEndTime);
		}

		[Server]
		[EventHandler]
		private void OnFinishGame()
		{
			RpcHandleFinishGame();
		}

		[ClientRpc]
		private void RpcHandleStartGame(long gameEndTime)
		{
			EventManager.Notify(() => EventManager.Local.ReceiveStartEvent(gameEndTime));
		}

		[ClientRpc]
		private void RpcHandleFinishGame()
		{
			EventManager.Notify(EventManager.Local.ReceiveFinishEvent);
		}

		#endregion

		#region Player Health/Death

		[Server]
		public void ApplyDamage(float amount, Vector3 point, Vector3 normal, IDamageSource cause)
		{
			if (ReferenceEquals(cause.source, this))
				amount *= 0.5f;

			RpcReflectDamageLocally(point, normal, cause.source.gameObject.transform.position, amount);

			if (mHealth <= 0.0f)
				return;

			mHealth = Mathf.Clamp(mHealth - amount, 0.0f, float.MaxValue);

			if (mHealth <= 0.0f)
				EventManager.Notify(() => EventManager.Server.PlayerHealthHitZero(this, cause));
		}

		[Server]
		[EventHandler]
		private void OnPlayerDied(CltPlayer deadPlayer, CltPlayer killer, Transform spawnPos)
		{
			if (deadPlayer == this)
			{
				if (killer != null)
					mDeaths++;

				mHealth = mInformation.defaultHealth;
				weapon.ResetToDefaultParts();
				RpcHandleDeath(transform.position, spawnPos.position, spawnPos.rotation);
			}
			else if (killer == this)
				mKills++;
		}

		[ClientRpc]
		private void RpcReflectDamageLocally(Vector3 point, Vector3 normal, Vector3 origin, float amount)
		{
			mHitIndicator.NotifyHit(this, origin, point, normal, amount);
		}

		[ClientRpc]
		private void RpcHandleDeath(Vector3 deathPosition, Vector3 spawnPos, Quaternion spawnRot)
		{
			ParticleSystem particles =
				Instantiate(mAssets.deathParticlesPrefab, deathPosition, Quaternion.identity).GetComponent<ParticleSystem>();
			particles.Play();
			StartCoroutine(Coroutines.WaitAndDestroyParticleSystem(particles));

			if (isLocalPlayer)
				ResetPlayerValues(spawnPos, spawnRot);
		}

		[ClientRpc]
		private void RpcResetPlayerValues(Vector3 position, Quaternion rotation)
		{
			ResetPlayerValues(position, rotation);
		}

		[Client]
		private void ResetPlayerValues(Vector3 position, Quaternion rotation)
		{
			transform.position = position;
			transform.rotation = rotation;
			mHealth = mInformation.defaultHealth;

			if (weapon != null)
				weapon.ResetToDefaultParts();
		}

		#endregion

		#region SyncVars

		[Client]
		private void OnHealthUpdate(float value)
		{
			mHealth = value;
			if (mLocalHealthVar != null)
				mLocalHealthVar.value = value;
		}

		[Client]
		private void OnKillsUpdate(int value)
		{
			mKills = value;
			if (mLocalKillsVar != null)
				mLocalKillsVar.value = value;
		}

		[Client]
		private void OnDeathsUpdate(int value)
		{
			mDeaths = value;
			if (mLocalDeathsVar != null)
				mLocalDeathsVar.value = value;
		}

		#endregion
	}
}