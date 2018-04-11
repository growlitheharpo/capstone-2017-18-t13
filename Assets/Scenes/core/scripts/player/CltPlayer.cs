using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FiringSquad.Core;
using FiringSquad.Core.Audio;
using FiringSquad.Core.State;
using FiringSquad.Core.UI;
using FiringSquad.Core.Weapons;
using FiringSquad.Data;
using FiringSquad.Gameplay.UI;
using FiringSquad.Gameplay.Weapons;
using FiringSquad.Networking;
using JetBrains.Annotations;
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
		private PlayerThirdPersonView mThirdPersonView;

		/// Private syncvars
		[SyncVar(hook = "OnHealthUpdate")] private float mHealth;
		private BoundProperty<float> mLocalHealthVar;

		[SyncVar(hook = "OnKillsUpdate")] private int mKills;
		private BoundProperty<int> mLocalKillsVar;

		[SyncVar(hook = "OnDeathsUpdate")] private int mDeaths;
		private BoundProperty<int> mLocalDeathsVar;

		[SyncVar(hook = "OnPlayerNameUpdate")] private string mPlayerName;

		[SyncVar(hook = "OnPlayerTeamUpdate")] private GameData.PlayerTeam mTeam = GameData.PlayerTeam.Deathmatch;

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

		/// <inheritdoc />
		public GameData.PlayerTeam playerTeam { get { return mTeam; } }

		/// <summary>
		/// The correct color for this player based on their team.
		/// </summary>
		public Color teamColor
		{
			get 
			{
				return mTeam == GameData.PlayerTeam.Orange ? defaultData.orangeTeamColor : defaultData.blueTeamColor;
			}
		}

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

		/// <summary>
		/// A reference directly to the local player, if available.
		/// </summary>
		[CanBeNull] public static CltPlayer localPlayerReference { get; private set; }

		#region Unity Callbacks and Initialization

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
			EventManager.Server.OnPlayerCapturedStage += OnPlayerCapturedStage;
			EventManager.Server.OnStartIntroSequence += OnStartIntroSequence;

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

			EventManager.LocalGeneric.OnPlayerDied += OnPlayerDied;
			EventManager.LocalGeneric.OnPlayerEquippedLegendaryPart += OnPlayerEquippedLegendaryPart;

			GameObject hitObject = new GameObject("HitIndicator");
			hitObject.transform.SetParent(transform);
			mHitIndicator = hitObject.AddComponent<RemotePlayerHitIndicator>();

			mLocalHealthVar = new BoundProperty<float>(mInformation.defaultHealth);
			defaultData.firstPersonView.SetActive(false);
			mThirdPersonView = GetComponentInChildren<PlayerThirdPersonView>();
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

			// Enable the magnet arm view
			if (magnetArm != null)
				magnetArm.SetViewVisible();

			// Send the "spawned" event.
			localPlayerReference = this;
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
			// ReSharper disable once PossibleNullReferenceException
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
		/// Assign the player to a particular team.
		/// </summary>
		public void AssignPlayerTeam(GameData.PlayerTeam newTeam)
		{
			mTeam = newTeam;
		}

		/// <summary>
		/// Debug assign the player team from a local command.
		/// </summary>
		/// <param name="team">Which team to change to.</param>
		public void CmdDebugSetTeam(GameData.PlayerTeam team)
		{
			AssignPlayerTeam(team);
		}

		/// <summary>
		/// Cleanup all listeners and event handlers, and spawned items.
		/// </summary>
		private void OnDestroy()
		{
			EventManager.Server.OnPlayerDied -= OnPlayerDied;
			EventManager.Server.OnStartGame -= OnStartGame;
			EventManager.Server.OnFinishGame -= OnFinishGame;
			EventManager.Server.OnPlayerCapturedStage -= OnPlayerCapturedStage;
			EventManager.Server.OnStartIntroSequence -= OnStartIntroSequence;

			EventManager.LocalGeneric.OnPlayerDied -= OnPlayerDied;
			EventManager.LocalGeneric.OnPlayerEquippedLegendaryPart -= OnPlayerEquippedLegendaryPart;

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

		#region Interaction

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

		#endregion

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

			AnimationUtility.SetVariable(mNetworkAnimator.animator, "VelocityX", velX);
			AnimationUtility.SetVariable(mNetworkAnimator.animator, "VelocityY", velY);

			// Update the third person firing animations
			if (weapon == null || mNetworkAnimator.animator == null)
				return;

			// Set fire rate and auto info
			mNetworkAnimator.animator.SetBool("WeaponIsAuto", weapon.currentData.fireRate >= 3.5f);
			mNetworkAnimator.animator.SetFloat("RecoilAmount", weapon.currentData.recoilAmount);
			mNetworkAnimator.animator.SetFloat("FireRate", weapon.currentData.fireRate * 1.1f);
		}

		/// <inheritdoc />
		public void PlayFireAnimation()
		{
			localAnimator.SetTrigger("Fire");

			if (weapon != null)
				mNetworkAnimator.animator.SetFloat("FireRate", weapon.currentData.fireRate);
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
			magnetArm.OnPostBind();
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

			EventManager.Server.PlayerCheated(this);
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
			if (magnetArm != null)
				magnetArm.ForceDropItem();

			TargetResetPlayerValues(connectionToClient, position, rotation);
		}

		/// <summary>
		/// EVENT HANDLER: Server.OnStartIntroSequence
		/// </summary>
		[Server]
		[EventHandler]
		private void OnStartIntroSequence()
		{
			TargetHandleStartIntroSequence(connectionToClient);
		}
		
		/// <summary>
		/// Locally handle the server instructing us to start our intro sequence.
		/// </summary>
		/// <param name="connection"></param>
		[TargetRpc]
		private void TargetHandleStartIntroSequence(NetworkConnection connection)
		{
			EventManager.Local.ReceiveStartIntroNotice();
		}

		/// <summary>
		/// EVENT HANDLER: Server.OnStartGame
		/// </summary>
		[Server]
		[EventHandler]
		private void OnStartGame(long gameEndTime)
		{
			if (magnetArm != null)
				magnetArm.ForceDropItem();

			TargetHandleInitialGameTime(connectionToClient, gameEndTime);
		}

		/// <summary>
		/// Locally handle the match starting on the server by playing a sound and updating our UI.
		/// </summary>
		[TargetRpc]
		private void TargetHandleInitialGameTime(NetworkConnection connection, long gameEndTime)
		{
			if (magnetArm != null)
				magnetArm.DropItemDown();

			EventManager.Notify(() => EventManager.Local.ReceiveGameEndTime(gameEndTime));

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
			//EventManager.Notify(() => EventManager.Local.ReceiveFinishEvent(scores));
			EventManager.Notify(() => EventManager.Local.TeamVictoryScreen(scores));
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

		/// <summary>
		/// EVENT HANDLER: EventManager.Server.OnPlayerCapturedStage
		/// </summary>
		[EventHandler]
		[Server]
		private void OnPlayerCapturedStage(StageCaptureArea stage, IList<CltPlayer> players)
		{
			if (players.Contains(this))
				RpcNotifyCapturedStage();
		}

		/// <summary>
		/// Notify the client that the server has confirmed that we've captured a stage.
		/// </summary>
		/// <para>
		/// NOTE: This used to be a TargetRPC but we've converted it to a broadcast so that everyone
		/// can keep their scorecards accurate.
		/// </para>
		[ClientRpc]
		private void RpcNotifyCapturedStage()
		{
			if (isCurrentPlayer)
				EventManager.Notify(EventManager.Local.LocalPlayerCapturedStage);

			EventManager.Notify(() => 
				EventManager.LocalGeneric.PlayerScoreChanged(this, NetworkServerGameManager.STAGE_CAPTURE_POINTS, 0, 0));
		}

		#endregion

		#region Player Health/Death

		/// <inheritdoc />
		[Server]
		public void ApplyDamage(float amount, Vector3 point, Vector3 normal, IDamageSource cause, bool wasHeadshot)
		{
			if (ReferenceEquals(cause.source, this))
				amount *= 0.5f;
			else if (cause.source is IWeaponBearer)
			{
				// Reject any damage from teammates
				IWeaponBearer b = (IWeaponBearer)cause.source;
				if (b.playerTeam != GameData.PlayerTeam.Deathmatch && b.playerTeam == mTeam)
					return;
			}

			NetworkInstanceId id = cause.source != null ? cause.source.netId : NetworkInstanceId.Invalid;
			Vector3 pos = cause.source != null ? cause.source.transform.position : transform.position;
			RpcReflectDamageLocally(point, normal, pos, amount, id);

			if (mHealth <= 0.0f)
				return;

			mHealth = Mathf.Clamp(mHealth - amount, 0.0f, float.MaxValue);

			if (mHealth <= 0.0f)
				EventManager.Notify(() => EventManager.Server.PlayerHealthHitZero(this, cause, wasHeadshot));
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
		private void OnPlayerDied(CltPlayer deadPlayer, PlayerKill killInfo)
		{
			ICharacter killer = killInfo.killer;

			if (deadPlayer == this)
			{
				if (killer != null)
					mDeaths++;

				SpawnDeathWeaponParts();

				if (magnetArm != null)
					magnetArm.ForceDropItem();

				mHealth = mInformation.defaultHealth;
				if (weapon != null)
					weapon.ResetToDefaultParts();

				killInfo.mDeathPosition = transform.position;
				RpcHandleDeath(killInfo);
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
			else if (isCurrentPlayer)
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

			if (mThirdPersonView != null)
				mThirdPersonView.ReflectTookDamage(mHealth);

			mHitIndicator.NotifyHit(this, origin, point, normal, amount);
			ServiceLocator.Get<IAudioManager>()
				.CreateSound(AudioEvent.PlayerDamagedGrunt, transform)
				.SetParameter("IsCurrentPlayer", Convert.ToSingle(isCurrentPlayer))
				.AttachToRigidbody(GetComponent<Rigidbody>());
		}

		/// <summary>
		/// Reflect the death of a player locally.
		/// </summary>
		/// <param name="killInfo">The struct collection of data about the kill, provided by the server.</param>
		[ClientRpc]
		private void RpcHandleDeath(PlayerKill killInfo)
		{
			ReflectLocalAgnosticDeathItems(killInfo);
			SendLocalDeathEvents(killInfo);
		}

		/// <summary>
		/// Displays all of the effects and runs any code that needs to happen when a player dies.
		/// This code doesn't care if the player that died is the local player or any other player in the match,
		/// it must be run no matter what.
		/// </summary>
		/// <param name="killInfo">The kill info provided by the server, explaining what happened.</param>
		private void ReflectLocalAgnosticDeathItems(PlayerKill killInfo)
		{
			// Show the death particles at the death location (and destroy them after they've expired)
			ParticleSystem particles = Instantiate(mAssets.deathParticlesPrefab, killInfo.mDeathPosition, Quaternion.identity).GetComponent<ParticleSystem>();
			particles.Play();
			StartCoroutine(Coroutines.WaitAndDestroyParticleSystem(particles));

			// Show our corpse at our location.
			GameObject corpse = Instantiate(mAssets.corpsePrefab, killInfo.mDeathPosition, transform.rotation);
			corpse.GetComponent<PlayerCorpseView>().UpdateColor(mTeam == GameData.PlayerTeam.Orange ? defaultData.orangeTeamColor : defaultData.blueTeamColor);

			// If we died, we should get removed from any potential highlight list.
			if (ObjectHighlight.instance != null)
			{
				var renderers = GetComponentsInChildren<Renderer>();
				foreach (Renderer r in renderers)
					ObjectHighlight.instance.RemoveRendererFromHighlightList(r);
			}
		}

		/// <summary>
		/// Send death-related events on the client based on who died and who the killer was.
		/// </summary>
		/// <param name="killInfo">The kill info provided by the server, explaining what happened.</param>
		[Client]
		private void SendLocalDeathEvents(PlayerKill killInfo)
		{
			ICharacter killer = killInfo.killer;
			CltPlayer killerAsClient = killer as CltPlayer;
			
			// Check if the killer was an actual player
			if (killerAsClient != null)
			{
				// If yes AND they're the current player, let the game know the local player got a kill.
				// If they're not the current player, do their face animation for a kill.
				if (killerAsClient.isCurrentPlayer)
					EventManager.Notify(() => EventManager.Local.LocalPlayerGotKill(this, killerAsClient.weapon, killInfo.mFlags));
				else
					killerAsClient.mThirdPersonView.ReflectGotKill(killInfo);

				// Either way, update the killer's score
				EventManager.LocalGeneric.PlayerScoreChanged(killerAsClient, NetworkServerGameManager.GetScoreForKillFlags(killInfo.mFlags), 1, 0);
			}

			// If the person who died is the local player, notify the game that the client has died.
			if (isCurrentPlayer)
			{
				mLocalHealthVar.value = 0.0f;
				EventManager.Local.LocalPlayerDied(killInfo, killer);
			}

			// No matter what, let the game know that SOMEONE has died (primarily for crowd audio).
			EventManager.Notify(() => EventManager.LocalGeneric.PlayerDied(this));

			// Also notify that this player got one more death.
			EventManager.LocalGeneric.PlayerScoreChanged(this, 0, 0, 1);
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
			if (magnetArm != null)
				magnetArm.DropItemDown();

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

		/// <summary>
		/// EVENT HANDLER: LocalGeneric.OnPlayerDied
		/// </summary>
		private void OnPlayerDied(CltPlayer player)
		{
			if (mThirdPersonView != null && player.mTeam == mTeam)
				mThirdPersonView.ReflectTeammateDied();
		}

		/// <summary>
		/// EVENT HANDLER: LocalGeneric.OnPlayerEquippedLegendaryPart
		/// </summary>
		private void OnPlayerEquippedLegendaryPart(CltPlayer player)
		{
			if (mThirdPersonView == null)
				return;

			if (player == this)
				mThirdPersonView.ReflectGotLegendaryPart();
			else if (player.playerTeam != mTeam)
				mThirdPersonView.ReflectEnemyGotLegendaryPart();
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
			{
				mLocalHealthVar.value = value;

				if (isCurrentPlayer)
					EventManager.LocalGUI.SetHintState(CrosshairHintText.Hint.LowHealth, mHealth <= 25.0f);
			}

			if (mThirdPersonView != null)
				mThirdPersonView.UpdateHealthAmount(value);
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

		/// <summary>
		/// Sync the player's assigned team.
		/// </summary>
		[Client]
		private void OnPlayerTeamUpdate(GameData.PlayerTeam value)
		{
			mTeam = value;

			// Update all of our child renderers
			var components = GetComponentsInChildren<ColormaskUpdateUtility>();
			foreach (ColormaskUpdateUtility updater in components)
				updater.UpdateDisplayedColor(teamColor);

			if (magnetArm != null)
				magnetArm.ApplyTeamColor();

			// Update the UI appropriately
			if (isCurrentPlayer)
				EventManager.Notify(() => EventManager.LocalGUI.LocalPlayerAssignedTeam(this));
			else
			{
				bool isEnemy = value == GameData.PlayerTeam.Deathmatch || localPlayerReference != null && localPlayerReference.mTeam != value;
				PlayerNameWorldCanvas display = GetComponentInChildren<PlayerNameWorldCanvas>();
				if (display != null)
					display.SetIsEnemyPlayer(isEnemy);
			}
		}

		#endregion
	}
}
