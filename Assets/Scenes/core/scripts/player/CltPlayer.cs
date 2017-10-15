using System.Collections.Generic;
using FiringSquad.Data;
using FiringSquad.Gameplay;
using FiringSquad.Gameplay.UI;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Networking;

public class CltPlayer : NetworkBehaviour, IWeaponBearer, IDamageReceiver
{
	[SerializeField] private PlayerAssetReferences mAssets;
	[SerializeField] private PlayerDefaultsData mInformation;

	[SerializeField] private Transform mCameraOffset;
	[SerializeField] private Transform mGun1Offset;
	[SerializeField] private Transform mGun2Offset;

	public bool isCurrentPlayer { get { return isLocalPlayer; } }

	public IWeapon weapon { get; private set; }
	public WeaponPartCollection defaultParts { get { return mInformation.defaultWeaponParts; } }
	public Transform eye { get { return mCameraOffset; } }

	private IPlayerHitIndicator mHitIndicator;

	[SyncVar(hook = "OnHealthUpdate")] private float mHealth;
	private BoundProperty<float> mLocalHealthVar;

	[SyncVar(hook = "OnKillsUpdate")] private int mKills;
	private BoundProperty<int> mLocalKillsVar;

	[SyncVar(hook = "OnDeathsUpdate")] private int mDeaths;
	private BoundProperty<int> mLocalDeathsVar;

	public override void OnStartServer()
	{
		// register for server events
		EventManager.Server.OnPlayerFiredWeapon += OnPlayerFiredWeapon;
		EventManager.Server.OnPlayerDied += OnPlayerDied;

		EventManager.Server.OnStartGame += OnStartGame;
		EventManager.Server.OnFinishGame += OnFinishGame;

		mHitIndicator = new NullHitIndicator();
		mHealth = mInformation.defaultHealth;

		// create our weapon & bind
		BaseWeaponScript wep = Instantiate(mAssets.baseWeaponPrefab).GetComponent<BaseWeaponScript>();
		BindWeaponToPlayer(wep);
		AddDefaultPartsToWeapon(wep);
		NetworkServer.SpawnWithClientAuthority(wep.gameObject, gameObject);
	}

	public override void OnStartClient()
	{
		ServiceLocator.Get<IWeaponPartManager>().GetAllPrefabs(false); // force the lazy initialization of the part list
		base.OnStartClient();

		// register for local events that should effect all players (might not be any?)

		// register anything specifically for non-local clients
		// TODO: Make spawning hit particles done through here
		mHitIndicator = gameObject.AddComponent<RemotePlayerHitIndicator>();
		mLocalHealthVar = new BoundProperty<float>(mInformation.defaultHealth);
	}

	public override void OnStartLocalPlayer()
	{
		// register for local events that should effect us

		CltPlayerLocal localScript = Instantiate(mAssets.localPlayerPrefab).GetComponent<CltPlayerLocal>();
		localScript.transform.SetParent(transform);
		localScript.playerRoot = this;

		mHitIndicator = (IPlayerHitIndicator)FindObjectOfType<LocalPlayerHitIndicator>() ?? new NullHitIndicator();
		mLocalHealthVar = new BoundProperty<float>(mInformation.defaultHealth, GameplayUIManager.PLAYER_HEALTH);
		mLocalKillsVar = new BoundProperty<int>(0, GameplayUIManager.PLAYER_KILLS);
		mLocalDeathsVar = new BoundProperty<int>(0, GameplayUIManager.PLAYER_DEATHS);

		StartCoroutine(Coroutines.WaitOneFrame(() =>
		{
			(weapon as BaseWeaponScript).BindPropertiesToUI();
		}));
	}

	public void BindWeaponToPlayer(BaseWeaponScript wep)
	{
		// find attach spot in view and set parent
		wep.transform.SetParent(mGun1Offset);
		wep.aimRoot = eye;
		wep.transform.ResetLocalValues();
		wep.positionOffset = eye.InverseTransformPoint(mGun1Offset.position);
		wep.transform.SetParent(transform);
		wep.bearer = this;
		weapon = wep;
	}

	private void AddDefaultPartsToWeapon(BaseWeaponScript wep)
	{
		foreach (WeaponPartScript part in defaultParts)
			wep.AttachNewPart(part.partId, true);
	}

	[Server][EventHandler]
	private void OnPlayerFiredWeapon(CltPlayer p, List<Ray> shots)
	{
		if (p != this)
			RpcReflectPlayerShotWeapon(p.netId);
	}

	[Server][EventHandler]
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

	[Server][EventHandler]
	private void OnStartGame(long gameEndTime)
	{
		RpcHandleStartGame(gameEndTime);
	}

	[Server][EventHandler]
	private void OnFinishGame()
	{
		RpcHandleFinishGame();
	}


	[Command]
	public void CmdActivateInteract(Vector3 eyePosition, Vector3 eyeForward)
	{
		IInteractable interactable;
		RaycastHit hit;

		Ray ray = new Ray(eyePosition, eyeForward);
		if (!Physics.Raycast(ray, out hit, mInformation.interactDistance))
			return;

		interactable = hit.GetInteractableComponent();

		if (interactable != null)
			interactable.Interact(this);
	}

	[Command]
	public void CmdDebugEquipWeaponPart(string part)
	{
		weapon.AttachNewPart(part);
	}

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
	public void MoveToStartPosition(Vector3 position, Quaternion rotation)
	{
		RpcResetPlayerValues(position, rotation);
	}

	[ClientRpc]
	private void RpcHandleStartGame(long gameEndTime)
	{
		EventManager.Notify(() => EventManager.Local.ReceiveStartEvent(gameEndTime));
	}

	[ClientRpc]
	private void RpcReflectDamageLocally(Vector3 point, Vector3 normal, Vector3 origin, float amount)
	{
		mHitIndicator.NotifyHit(this, origin, point, normal, amount);
	}

	[ClientRpc]
	private void RpcReflectPlayerShotWeapon(NetworkInstanceId playerId)
	{
		if (playerId == netId)
			return;

		CltPlayer p = ClientScene.FindLocalObject(playerId).GetComponent<CltPlayer>();
		p.weapon.PlayFireEffect();
	}

	[ClientRpc]
	private void RpcResetPlayerValues(Vector3 position, Quaternion rotation)
	{
		ResetPlayerValues(position, rotation);
	}

	[ClientRpc]
	private void RpcHandleFinishGame()
	{
		EventManager.Notify(EventManager.Local.ReceiveFinishEvent);
	}

	[ClientRpc]
	private void RpcHandleDeath(Vector3 deathPosition, Vector3 spawnPos, Quaternion spawnRot)
	{
		ParticleSystem particles = Instantiate(mAssets.deathParticlesPrefab, deathPosition, Quaternion.identity).GetComponent<ParticleSystem>();
		particles.Play();
		StartCoroutine(Coroutines.WaitAndDestroyParticleSystem(particles));

		if (isLocalPlayer)
			ResetPlayerValues(spawnPos, spawnRot);
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
}
