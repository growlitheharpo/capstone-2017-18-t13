using System;
using FiringSquad.Networking;
using UnityEngine;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// Prettier UI panel element for joining or starting a networked game.
	/// </summary>
	public class JoinGamePanel : MonoBehaviour
	{
		[SerializeField] private ActionProvider mHostGameButton;
		[SerializeField] private ActionProvider mFindGameButton;
		[SerializeField] private UnityEngine.UI.Text mStatusText;

		private OverrideNetworkDiscovery mDiscoveryHandler;
		private NetworkGameManager mNetworkManager;

		/// <summary>
		/// Unity's Awake function.
		/// </summary>
		private void Awake()
		{
			mDiscoveryHandler = FindObjectOfType<OverrideNetworkDiscovery>();
			mNetworkManager = FindObjectOfType<NetworkGameManager>();

			if (mDiscoveryHandler == null || mNetworkManager == null)
			{
				Destroy(this);
				throw new InvalidOperationException("Cannot use a join game panel without the network managers in the scene!");
			}

			mHostGameButton.OnClick += OnClickHostGameButton;
			mFindGameButton.OnClick += OnClickFindGameButton;
			EventManager.Local.OnLocalPlayerSpawned += OnLocalPlayerSpawned;
		}

		/// <summary>
		/// Cleanup listeners and event handlers.
		/// </summary>
		private void OnDestroy()
		{
			mHostGameButton.OnClick -= OnClickHostGameButton;
			mFindGameButton.OnClick -= OnClickFindGameButton;
			EventManager.Local.OnLocalPlayerSpawned -= OnLocalPlayerSpawned;
		}

		/// <summary>
		/// Unity's Update function.
		/// </summary>
		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.H))
				OnClickHostGameButton();
			else if (Input.GetKeyDown(KeyCode.C))
				OnClickFindGameButton();
		}

		/// <summary>
		/// Handle creating and hosting a game.
		/// </summary>
		private void OnClickHostGameButton()
		{
			mDiscoveryHandler.Initialize();
			mDiscoveryHandler.StartAsServer();
			mNetworkManager.StartHost();
		}

		/// <summary>
		/// Handle finding a game.
		/// </summary>
		private void OnClickFindGameButton()
		{
			mDiscoveryHandler.Initialize();
			mDiscoveryHandler.StartAsClient();

			Destroy(mHostGameButton.gameObject);
			Destroy(mFindGameButton.gameObject);
			mStatusText.text = "Searching for game...";
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnLocalPlayerSpawned
		/// </summary>
		private void OnLocalPlayerSpawned(CltPlayer obj)
		{
			Destroy(gameObject);
		}
	}
}
