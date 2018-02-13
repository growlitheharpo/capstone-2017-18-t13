using System;
using System.Collections;
using System.Linq;
using FiringSquad.Core;
using FiringSquad.Core.Audio;
using FiringSquad.Core.State;
using FiringSquad.Core.UI;
using FiringSquad.Core.Weapons;
using FiringSquad.Data;
using FiringSquad.Gameplay.UI;
using FiringSquad.Gameplay.Weapons;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

namespace FiringSquad.Gameplay
{
	/// <summary>
	/// The primary class for the player GameObject.
	/// Implements IWeaponBearer and IDamageReceiver.
	/// Handles all network communication that directly affects a player.
	/// Represents the "local player object" for all clients.
	/// </summary>
	/// <seealso cref="IDamageReceiver"/>
	/// <seealso cref="IWeaponBearer"/>
	public class CltPlayer : NetworkBehaviour, IWeaponBearer, IDamageReceiver
	{
		/// Inspector variables
		[SerializeField] private PlayerAssetReferences mAssets;
		[SerializeField] private PlayerDefaultsData mInformation;

		[Header("References")] [SerializeField] private Transform mCameraOffset;
		[SerializeField] private Transform mGun1Offset;
		[SerializeField] private Transform mGun2Offset;
		[SerializeField] private Animator mAnimator;
		[SerializeField] private NetworkAnimator mNetworkAnimator;

		/// Private variables
		private IPlayerHitIndicator mHitIndicator;
		private CharacterController mCharacterController;
		private CltPlayerLocal mLocalPlayerScript;

		/// Private syncvars
		[SyncVar(hook = "OnHealthUpdate")] private float mHealth;
		private BoundProperty<float> mLocalHealthVar;

		[SyncVar(hook = "OnKillsUpdate")] private int mKills;
		private BoundProperty<int> mLocalKillsVar;

		[SyncVar(hook = "OnDeathsUpdate")] private int mDeaths;
		private BoundProperty<int> mLocalDeathsVar;

		[SyncVar(hook = "OnPlayerNameUpdate")] private string mPlayerName;

		/// <inheritdoc />
		public bool isCurrentPlayer { get { return isLocalPlayer; } }

		/// <inheritdoc />
		public IWeapon weapon { get; private set; }

		/// <inheritdoc />
		public WeaponPartCollection defaultParts { get { return mInformation.defaultWeaponParts; } }

		/// <inheritdoc />
		public Transform eye { get { return mCameraOffset; } }

		/// <inheritdoc />
		public float currentHealth { get { return mHealth; } }

		/// <summary>
		/// The local animator for this player.
		/// </summary>
		public Animator localAnimator { get { return mAnimator; } }

		/// <summary>
		/// The animator for this player that sends events over network.
		/// </summary>
		public NetworkAnimator networkAnimator { get { return mNetworkAnimator; } }

		/// <summary>
		/// The inspector balance data for this player.
		/// </summary>
		public PlayerDefaultsData defaultData { get { return mInformation; } }

		/// <summary>
		/// The transform offset to position the player's weapon.
		/// </summary>
		public Transform gunOffset { get { return mGun1Offset; } }
		
		/// <summary>
		/// The magnet arm attached to this player.
		/// </summary>
		public PlayerMagnetArm magnetArm { get; private set; }

		/// <summary>
		/// The custom player name entered by this player.
		/// </summary>
		public string playerName { get { return mPlayerName; } }

		#region Unity Callbacks

		/// <summary>
		/// Unity's server-side start event.
		/// </summary>
		public override void OnStartServer()
		{
			base.OnStartServer();

			// register for server events
			EventManager.Server.OnPlayerDied += OnPlayerDied;
			EventManager.Server.OnStartGame += OnStartGame;
			EventManager.Server.OnFinishGame += OnFinishGame;

			// register information
			mHitIndicator = new NullHitIndicator();
			mHealth = mInformation.defaultHealth;

			// create our weapon & bind
			BaseWeaponScript wep = Instantiate(mAssets.baseWeaponPrefab).GetComponent<BaseWeaponScript>();
			BindWeaponToBearer(wep, true);
			AddDefaultPartsToWeapon(wep);
			NetworkServer.SpawnWithClientAuthority(wep.gameObject, gameObject);

			// create our magnet arm & bind
			PlayerMagnetArm arm = Instantiate(mAssets.magnetArmPrefab).GetComponent<PlayerMagnetArm>();
			BindMagnetArmToPlayer(arm);
			NetworkServer.SpawnWithClientAuthority(arm.gameObject, gameObject);
		}

		/// <summary>
		/// Unity's client-side start event.
		/// </summary>
		public override void OnStartClient()
		{
			base.OnStartClient();

			mCharacterController = GetComponent<CharacterController>();

			GameObject hitObject = new GameObject("HitIndicator");
			hitObject.transform.SetParent(transform);
			mHitIndicator = hitObject.AddComponent<RemotePlayerHitIndicator>();

			mLocalHealthVar = new BoundProperty<float>(mInformation.defaultHealth);
			defaultData.firstPersonView.SetActive(false);
		}

		/// <summary>
		/// Unity's client-side start event that is ONLY called when this is the local player.
		/// </summary>
		public override void OnStartLocalPlayer()
		{
			// force the lazy initialization of the part list
			ServiceLocator.Get<IWeaponPartManager>().GetAllPrefabs(false);

			// Instantiate the local player controllers.
			mLocalPlayerScript = Instantiate(mAssets.localPlayerPrefab).GetComponent<CltPlayerLocal>();
			mLocalPlayerScript.transform.SetParent(transform);
			mLocalPlayerScript.playerRoot = this;

			// Bind the UI properties that we need.
			mLocalHealthVar = new BoundProperty<float>(mInformation.defaultHealth, UIManager.PLAYER_HEALTH);
			mLocalKillsVar = new BoundProperty<int>(0, UIManager.PLAYER_KILLS);
			mLocalDeathsVar = new BoundProperty<int>(0, UIManager.PLAYER_DEATHS);
			StartCoroutine(GrabLocalHitIndicator());

			// Disable the renderers for the local player.
			StartCoroutine(AdjustToLocalView());

			// Send the "spawned" event.
			EventManager.Notify(() => EventManager.Local.LocalPlayerSpawned(this));
		}

		/// <summary>
		/// Adjust the "view" GameObjects to reflect that this is the local player.
		/// </summary>
		private IEnumerator AdjustToLocalView()
		{
			while (weapon == null)
				yield return null;

			// Enable the first person view.
			defaultData.firstPersonView.SetActive(true);

			// Destroy the third person renderers so that we can disable that view but still
			// let the animator update properly (necessary for UNET).
			var renderers = defaultData.thirdPersonView.GetComponentsInChildren<Renderer>();
			foreach (Renderer r in renderers)
				Destroy(r);

			//we have to do a delicate dance of changing parents now
			Transform viewHolder = weapon.transform.Find("View").Find("ViewHolder");
			Transform gunMesh = viewHolder.GetChild(0);

			// Assign the correct parents and reset their offset to zero.
			defaultData.firstPersonView.transform.SetParent(viewHolder, false);
			defaultData.firstPersonView.transform.ResetLocalValues();
			gunMesh.SetParent(defaultData.firstPersonWeaponBone, false);
			gunMesh.ResetLocalValues();

			// Update the BaseWeaponView of what our arm's animator is.
			BaseWeaponView view = weapon.gameObject.GetComponent<BaseWeaponView>();
			if (view != null)
				view.SetArmAnimator(defaultData.firstPersonView.GetComponentInChildren<Animator>());
		}

		/// <summary>
		/// Grab a reference to the UI-based local player hit indicator once it exists.
		/// </summary>
		private IEnumerator GrabLocalHitIndicator()
		{
			mHitIndicator = new NullHitIndicator(); // a placeholder to avoid errors
			LocalPlayerHitIndicator realIndicator = null;

			while (realIndicator == null)
			{
				realIndicator = FindObjectOfType<LocalPlayerHitIndicator>();
				yield return null;
			}

			mHitIndicator = realIndicator;
		}

		/// <summary>
		/// Cleanup all listeners and event handlers, and spawned items.
		/// </summary>
		private void OnDestroy()
		{
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

		/// <summary>
		/// Client-only Unity Update function.
		/// </summary>
		[ClientCallback]
		private void Update()
		{
			UpdateAnimations();
		}

		#endregion

		/// <summary>
		/// Activate the "interact" input command on the server.
		/// </summary>
		/// <param name="eyePosition">The local eye position of the player.</param>
		/// <param name="eyeForward">The local eye forward of the player.</param>
		[Command]
		public void CmdActivateInteract(Vector3 eyePosition, Vector3 eyeForward)
		{
			IInteractable interactable = null;

			// Prioritize anything held by the magnet arm.
			if (magnetArm != null)
			{
				interactable = magnetArm.currentlyHeldObject;
				if (interactable != null)
					magnetArm.ForceDropItem();
			}

			// If there's nothing that we're holding, look ahead of us.
			if (interactable == null)
			{
				RaycastHit hit;

				Ray ray = new Ray(eyePosition, eyeForward);
				if (!Physics.Raycast(ray, out hit, mInformation.interactDistance, int.MaxValue, QueryTriggerInteraction.Ignore))
					return;

				interactable = hit.GetInteractableComponent();
			}

			if (interactable != null)
				interactable.Interact(this);
		}

		/// <summary>
		/// Activate the "interact" input command on the server on a particular object.
		/// </summary>
		/// <param name="objectId">The network instance ID of the IInteractable to activate.</param>
		[Command]
		public void CmdActivateInteractWithObject(NetworkInstanceId objectId)
		{
			GameObject go = NetworkServer.FindLocalObject(objectId);
			if (go == null)
				return;

			IInteractable interactable = go.GetComponent<IInteractable>();
			if (interactable == null)
				return;

			interactable.Interact(this);
		}

		#region Animations

		/// <summary>
		/// Update our velocity-based animation parameters based on local variables.
		/// </summary>
		[Client]
		private void UpdateAnimations()
		{
			// TODO: spread this calculation out over multiple frames.
			Vector3 relativeVel = mCharacterController.velocity / 6; // 6 is the MOVEMENTDATA SPEED. THIS SHOULD NOT BE HARDCODED
			relativeVel = transform.InverseTransformDirection(relativeVel);
			Vector2 vel = new Vector2(relativeVel.x, relativeVel.z);

			float velX = Mathf.Lerp(AnimationUtility.GetFloat(mAnimator, "VelocityX"), vel.x, Time.deltaTime * 3.0f);
			float velY = Mathf.Lerp(AnimationUtility.GetFloat(mAnimator, "VelocityY"), vel.y, Time.deltaTime * 3.0f);

			AnimationUtility.SetVariable(mAnimator, "VelocityX", velX);
			AnimationUtility.SetVariable(mAnimator, "VelocityY", velY);
		}

		/// <inheritdoc />
		public void PlayFireAnimation()
		{
			localAnimator.SetTrigger("Fire");
			networkAnimator.SetTrigger("Fire");
		}

		#endregion

		#region Weapons

		/// <inheritdoc />
		public void BindWeaponToBearer(IModifiableWeapon wep, bool bindUI = false)
		{
			if (weapon != null)
				throw new InvalidOperationException("This IWeaponBearer already has a weapon bound!");

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

		/// <summary>
		/// Two-way bind a magnet arm to this player.
		/// </summary>
		/// <param name="arm">The magnet arm to be bound.</param>
		public void BindMagnetArmToPlayer(PlayerMagnetArm arm)
		{
			arm.transform.SetParent(mGun2Offset);
			arm.transform.ResetLocalValues();
			arm.transform.SetParent(transform);
			arm.bearer = this;

			magnetArm = arm;
		}

		/// <summary>
		/// Attach parts to the provided weapon script.
		/// </summary>
		private void AddDefaultPartsToWeapon(BaseWeaponScript wep)
		{
			foreach (WeaponPartScript part in defaultParts)
				wep.AttachNewPart(part.partId, WeaponPartScript.INFINITE_DURABILITY);
		}

		/// <summary>
		/// Immediately equip the weapon with the provided part ID.
		/// </summary>
		/// <param name="partId">The unique ID of the part to attach.</param>
		[Command]
		public void CmdDebugEquipWeaponPart(byte partId)
		{
			if (weapon == null)
				return;

			weapon.AttachNewPart(partId);
		}
		
		/// <summary>
		/// Instantiate the non-default weapons this player was holding and drop them where they died.
		/// </summary>
		[Server]
		private void SpawnDeathWeaponParts()
		{
			if (weapon == null)
				return;

			IWeaponPartManager partService = ServiceLocator.Get<IWeaponPartManager>();
			Vector3 weaponPos = weapon.transform.position;

			foreach (WeaponPartScript part in weapon.currentParts)
			{
				// skip default weapon parts
				if (defaultParts.Any(defaultPart => part.partId == defaultPart.partId))
					continue;

				WeaponPartScript prefab = partService.GetPrefabScript(part.partId);
				GameObject instance = prefab.SpawnInWorld();

				instance.transform.position = weaponPos + Random.insideUnitSphere;
				instance.GetComponent<Rigidbody>().AddExplosionForce(40.0f, transform.position, 2.0f);

				// give the part its current durability.
				instance.GetComponent<WeaponPickupScript>().overrideDurability = part.durability;

				NetworkServer.Spawn(instance);

				StartCoroutine(Coroutines.InvokeAfterFrames(2, () => { instance.GetComponent<WeaponPickupScript>().RpcInitializePickupView(); }));
			}
		}

		#endregion

		#region GameState

		/// <summary>
		/// Transition into lobby mode and start ticking down until it is over.
		/// </summary>
		[TargetRpc]
		public void TargetStartLobbyCountdown(NetworkConnection connection, long endTime)
		{
			EventManager.Notify(() => EventManager.Local.ReceiveLobbyEndTime(this, endTime));

			// Lobby means that all players have connected. Let's fire off setting our name now.
			CmdSetPlayerName(ServiceLocator.Get<IGamestateManager>().currentUserName);
		}

		/// <summary>
		/// Move the player to the provided position and rotation immediately and reset all values.
		/// </summary>
		/// <param name="position">The target spawn position of the player.</param>
		/// <param name="rotation">The target spawn rotation of the player.</param>
		[Server]
		public void MoveToStartPosition(Vector3 position, Quaternion rotation)
		{
			TargetResetPlayerValues(connectionToClient, position, rotation);

			if (magnetArm != null)
				magnetArm.ForceDropItem();
		}

		/// <summary>
		/// EVENT HANDLER: Server.OnStartGame
		/// </summary>
		[Server]
		[EventHandler]
		private void OnStartGame(long gameEndTime)
		{
			TargetHandleStartGame(connectionToClient, gameEndTime);
		}

		/// <summary>
		/// Locally handle the match starting on the server by playing a sound and updating our UI.
		/// </summary>
		[TargetRpc]
		private void TargetHandleStartGame(NetworkConnection connection, long gameEndTime)
		{
			EventManager.Notify(() => EventManager.Local.ReceiveStartEvent(gameEndTime));
			ServiceLocator.Get<IAudioManager>()
				.CreateSound(AudioEvent.AnnouncerMatchStarts, transform);

			// Set our name again, just in case it wasn't updated.
			CmdSetPlayerName(ServiceLocator.Get<IGamestateManager>().currentUserName);
		}

		/// <summary>
		/// EVENT HANDLER: Server.OnFinishGame
		/// </summary>
		[Server]
		[EventHandler]
		private void OnFinishGame(PlayerScore[] scores)
		{
			TargetHandleFinishGame(connectionToClient, PlayerScore.SerializeArray(scores));
		}

		/// <summary>
		/// Locally handle the match ending on the server.
		/// </summary>
		[TargetRpc]
		private void TargetHandleFinishGame(NetworkConnection connection, byte[] serializedArray)
		{
			if (!isLocalPlayer)
				return;
			
			var scores = PlayerScore.DeserializeArray(serializedArray);
			EventManager.Notify(() => EventManager.Local.ReceiveFinishEvent(scores));
			ServiceLocator.Get<IAudioManager>()
				.CreateSound(AudioEvent.AnnouncerMatchEnds, transform);
		}

		/// <summary>
		/// Update the player's name to a new value on the server.
		/// </summary>
		[Command]
		private void CmdSetPlayerName(string newValue)
		{
			mPlayerName = newValue;
		}

		#endregion

		#region Player Health/Death

		/// <inheritdoc />
		[Server]
		public void ApplyDamage(float amount, Vector3 point, Vector3 normal, IDamageSource cause)
		{
			if (ReferenceEquals(cause.source, this))
				amount *= 0.5f;

			NetworkInstanceId id = cause.source != null ? cause.source.netId : NetworkInstanceId.Invalid;
			Vector3 pos = cause.source != null ? cause.source.transform.position : transform.position;
			RpcReflectDamageLocally(point, normal, pos, amount, id);

			if (mHealth <= 0.0f)
				return;

			mHealth = Mathf.Clamp(mHealth - amount, 0.0f, float.MaxValue);

			if (mHealth <= 0.0f)
				EventManager.Notify(() => EventManager.Server.PlayerHealthHitZero(this, cause));
		}

		/// <inheritdoc />
		public void HealDamage(float amount)
		{
			mHealth = Mathf.Clamp(mHealth + amount, 0.0f, defaultData.defaultHealth);
		}

		/// <summary>
		/// EVENT HANDLER: Server.OnPlayerDied
		/// </summary>
		[Server]
		[EventHandler]
		private void OnPlayerDied(CltPlayer deadPlayer, ICharacter killer, Transform spawnPos)
		{
			if (deadPlayer == this)
			{
				NetworkInstanceId killerId;

				if (killer != null)
				{
					mDeaths++;
					killerId = killer.netId;
				}
				else
					killerId = NetworkInstanceId.Invalid;

				SpawnDeathWeaponParts();

				if (magnetArm != null)
					magnetArm.ForceDropItem();

				mHealth = mInformation.defaultHealth;
				if (weapon != null)
					weapon.ResetToDefaultParts();

				RpcHandleDeath(transform.position, spawnPos.position, spawnPos.rotation, killerId);
			}
			else if (ReferenceEquals(killer, this))
				mKills++;
		}

		/// <summary>
		/// Reflect damage taken on the server locally.
		/// </summary>
		[ClientRpc]
		private void RpcReflectDamageLocally(Vector3 point, Vector3 normal, Vector3 origin, float amount, NetworkInstanceId source)
		{
			GameObject sourceGo = ClientScene.FindLocalObject(source);
			if (sourceGo == null)
				sourceGo = gameObject;

			ICharacter realSource = sourceGo.GetComponent<ICharacter>();
			if (realSource.isCurrentPlayer)
			{
				EventManager.Notify(() => EventManager.Local.LocalPlayerCausedDamage(amount));
				ServiceLocator.Get<IAudioManager>().CreateSound(AudioEvent.LocalDealDamage, realSource.transform);
			}
			else if (this.isCurrentPlayer)
			{
				// else notify the camera it should shake
				Camera cameraRef = GetComponentInChildren<Camera>();
				if (cameraRef != null)
				{
					ScreenShake screenShake = cameraRef.GetComponent<ScreenShake>();
					if (screenShake != null)
						screenShake.NotifyHit(this, origin, point, normal, amount);
				}
			}

			mHitIndicator.NotifyHit(this, origin, point, normal, amount);
			ServiceLocator.Get<IAudioManager>()
				.CreateSound(AudioEvent.PlayerDamagedGrunt, transform)
				.SetParameter("IsCurrentPlayer", Convert.ToSingle(isCurrentPlayer))
				.AttachToRigidbody(GetComponent<Rigidbody>());

		}

		/// <summary>
		/// Reflect the death of a player locally.
		/// </summary>
		/// <param name="deathPosition">The position where the player died.</param>
		/// <param name="spawnPos">The new target spawn position of the player.</param>
		/// <param name="spawnRot">The new target spawn rotation of the player.</param>
		/// <param name="killer">The network id of the character that killed the player.</param>
		[ClientRpc]
		private void RpcHandleDeath(Vector3 deathPosition, Vector3 spawnPos, Quaternion spawnRot, NetworkInstanceId killer)
		{
			ParticleSystem particles = Instantiate(mAssets.deathParticlesPrefab, deathPosition, Quaternion.identity).GetComponent<ParticleSystem>();
			particles.Play();
			StartCoroutine(Coroutines.WaitAndDestroyParticleSystem(particles));

			IWeaponBearer killerObj = killer == NetworkInstanceId.Invalid ? null : ClientScene.FindLocalObject(killer).GetComponent<IWeaponBearer>();

			if (killerObj != null && killerObj.isCurrentPlayer)
				EventManager.Notify(() => EventManager.Local.LocalPlayerGotKill(this, killerObj.weapon));

			if (!isLocalPlayer)
				return;

			mLocalHealthVar.value = 0.0f;
			EventManager.Local.LocalPlayerDied(spawnPos, spawnRot, killerObj);
		}

		/// <summary>
		/// Reset all the values of this player (position, rotation, health, weapon parts).
		/// </summary>
		/// <param name="connection">The connection to the player to reset.</param>
		/// <param name="position">The new target spawn position of the player.</param>
		/// <param name="rotation">The new target spawn rotation of the player.</param>
		[TargetRpc]
		private void TargetResetPlayerValues(NetworkConnection connection, Vector3 position, Quaternion rotation)
		{
			ResetPlayerValues(position, rotation);
		}

		/// <summary>
		/// Reset all the values of this player (position, rotation, health, weapon parts).
		/// </summary>
		/// <param name="position">The new target spawn position of the player.</param>
		/// <param name="rotation">The new target spawn rotation of the player.</param>
		[Client]
		public void ResetPlayerValues(Vector3 position, Quaternion rotation)
		{
			transform.position = position;
			transform.rotation = rotation;
			mHealth = mInformation.defaultHealth;
			mLocalHealthVar.value = mInformation.defaultHealth;

			if (weapon != null)
				weapon.ResetToDefaultParts();
		}

		#endregion

		#region SyncVars

		/// <summary>
		/// Sync the player's health across the network (and UI).
		/// </summary>
		[Client]
		private void OnHealthUpdate(float value)
		{
			mHealth = value;
			if (mLocalHealthVar != null)
				mLocalHealthVar.value = value;
		}

		/// <summary>
		/// Sync the player's kill count across the network (and UI).
		/// </summary>
		[Client]
		private void OnKillsUpdate(int value)
		{
			if (value > mKills && isCurrentPlayer)
				ServiceLocator.Get<IAudioManager>().CreateSound(AudioEvent.GetKill, transform);

			mKills = value;
			if (mLocalKillsVar != null)
				mLocalKillsVar.value = value;
		}

		/// <summary>
		/// Sync the player's death count across the network (and UI).
		/// </summary>
		[Client]
		private void OnDeathsUpdate(int value)
		{
			mDeaths = value;
			if (mLocalDeathsVar != null)
				mLocalDeathsVar.value = value;
		}
		
		/// <summary>
		/// Sync the player's customized name across the network (and UI).
		/// </summary>
		[Client]
		private void OnPlayerNameUpdate(string value)
		{
			PlayerNameWorldCanvas display = GetComponentInChildren<PlayerNameWorldCanvas>();
			if (display != null)
				display.SetPlayerName(value);

			mPlayerName = value;
		}

		#endregion
	}
}
