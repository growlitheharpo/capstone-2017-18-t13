using System;
using System.Collections.Generic;
using System.Reflection;
using FiringSquad.Core;
using FiringSquad.Core.Input;
using FiringSquad.Data;
using FiringSquad.Debug;
using FiringSquad.Gameplay;
using FiringSquad.Gameplay.UI;
using FiringSquad.Gameplay.Weapons;
using UnityEngine;

/// <summary>
/// A list of game events for this project.
/// </summary>
public partial class EventManager
{
	/// <summary>
	/// Holder class for all events that are only designed to occur on local clients.
	/// </summary>
	public static class Local
	{
		/// <summary>
		/// Event called when the audio system has finished loading and is ready for use.
		/// </summary>
		public static event Action OnInitialAudioLoadComplete = () => { LogEvent(); };

		/// <summary>
		/// Event called when the audio system has finished loading and is ready for use.
		/// </summary>
		public static void InitialAudioLoadComplete()
		{
			OnInitialAudioLoadComplete();
		}

		/// <summary>
		/// Event called when new OptionsData are ready to be applied.
		/// </summary>
		public static event Action<IOptionsData> OnApplyOptionsData = e => { LogEvent(); };

		/// <summary>
		/// Event called when new OptionsData are ready to be applied.
		/// </summary>
		public static void ApplyOptionsData(IOptionsData data)
		{
			OnApplyOptionsData(data);
		}

		/// <summary>
		/// Event called when the player has requested a game pause.
		/// </summary>
		public static event Action OnTogglePause = () => { LogEvent(); };

		/// <summary>
		/// Event called when the player has requested a game pause.
		/// </summary>
		public static void TogglePause()
		{
			OnTogglePause();
		}

		/// <summary>
		/// Event called when this player has received a start event from the server.
		/// PARAMETER 1: The end time in ticks that the game should end.
		/// </summary>
		public static event Action<long> OnReceiveStartEvent = t => { LogEvent(); };

		/// <summary>
		/// Event called when this player has received a start event from the server.
		/// </summary>
		/// <param name="endTime">The end time in ticks that the game should end.</param>
		public static void ReceiveStartEvent(long endTime)
		{
			OnReceiveStartEvent(endTime);
		}

		/// <summary>
		/// Event called when the player has received the end game event from the server.
		/// PARAMETER 1: The list of player scores received from the server.
		/// </summary>
		public static event Action<PlayerScore[]> OnReceiveFinishEvent = (s) => { LogEvent(); };

		/// <summary>
		/// Event called when the player has received the end game event from the server.
		/// </summary>
		/// <param name="scores">The list of player scores received from the server.</param>
		public static void ReceiveFinishEvent(PlayerScore[] scores)
		{
			OnReceiveFinishEvent(scores);
		}

		/// <summary>
		/// Event called when the local player has spawned and is ready to play.
		/// PARAMETER 1: A reference to the local player.
		/// </summary>
		public static event Action<CltPlayer> OnLocalPlayerSpawned = p => { LogEvent(); };

		/// <summary>
		/// Event called when the local player has spawned and is ready to play.
		/// </summary>
		/// <param name="p">A reference to the local player.</param>
		public static void LocalPlayerSpawned(CltPlayer p)
		{
			OnLocalPlayerSpawned(p);
		}

		/// <summary>
		/// Event called when the local player has attached a new part onto their weapon.
		/// PARAMETER 1: The weapon of the local player, which has had a part attached.
		/// PARAMETER 2: The weapon part that was attached.
		/// </summary>
		public static event Action<BaseWeaponScript, WeaponPartScript> OnLocalPlayerAttachedPart = (w, p) => { LogEvent(); };

		/// <summary>
		/// Event called when the local player has attached a new part onto their weapon.
		/// </summary>
		/// <param name="weapon">The weapon of the local player, which has had a part attached.</param>
		/// <param name="part">The weapon part that was attached.</param>
		public static void LocalPlayerAttachedPart(BaseWeaponScript weapon, WeaponPartScript part)
		{
			OnLocalPlayerAttachedPart(weapon, part);
		}

		/// <summary>
		/// Event called when the local player has picked up a weapon part and is holding it with the magnet arm.
		/// PARAMETER 1: The weapon part that is currently being held.
		/// </summary>
		public static event Action<WeaponPartScript> OnLocalPlayerHoldingPart = w => { LogEvent(); };

		/// <summary>
		/// Event called when the local player has picked up a weapon part and is holding it with the magnet arm.
		/// </summary>
		/// <param name="part">The weapon part that is currently being held.</param>
		public static void LocalPlayerHoldingPart(WeaponPartScript part)
		{
			OnLocalPlayerHoldingPart(part);
		}

		/// <summary>
		/// Event called when the local player has released a weapon part that they were holding in the magnet arm.
		/// PARAMETER 1: The weapon part that was previously being held.
		/// </summary>
		public static event Action<WeaponPartScript> OnLocalPlayerReleasedPart = w => { LogEvent(); };

		/// <summary>
		/// Event called when the local player has released a weapon part that they were holding in the magnet arm.
		/// </summary>
		/// <param name="part">The weapon part that was previously being held.</param>
		public static void LocalPlayerReleasedPart(WeaponPartScript part)
		{
			OnLocalPlayerReleasedPart(part);
		}

		/// <summary>
		/// Event called when the local player has entered Aim Down Sights mode.
		/// </summary>
		public static event Action OnEnterAimDownSightsMode = () => { LogEvent(); };

		/// <summary>
		/// Event called when the local player has entered Aim Down Sights mode.
		/// </summary>
		public static void EnterAimDownSightsMode()
		{
			OnEnterAimDownSightsMode();
		}

		/// <summary>
		/// Event called when the local player has exited Aim Down Sights mode.
		/// </summary>
		public static event Action OnExitAimDownSightsMode = () => { LogEvent(); };

		/// <summary>
		/// Event called when the local player has exited Aim Down Sights mode.
		/// </summary>
		public static void ExitAimDownSightsMode()
		{
			OnExitAimDownSightsMode();
		}

		/// <summary>
		/// Event called when the local player has caused any amount of damage confirmed by the server.
		/// PARAMETER 1: The amount of damage that was caused.
		/// </summary>
		public static event Action<float> OnLocalPlayerCausedDamage = a => { LogEvent(); };

		/// <summary>
		/// Event called when the local player has caused any amount of damage confirmed by the server.
		/// </summary>
		/// <param name="amount">The amount of damage that was caused.</param>
		public static void LocalPlayerCausedDamage(float amount)
		{
			OnLocalPlayerCausedDamage(amount);
		}

		/// <summary>
		/// Event called when the local player has died according to the server.
		/// PARAMETER 1: The target spawn location of the player.
		/// PARAMETER 2: The target spawn rotation of the player.
		/// PARAMETER 3: The character that killed the player. Can be null.
		/// </summary>
		public static event Action<Vector3, Quaternion, ICharacter> OnLocalPlayerDied = (p, r, k) => { LogEvent(); };

		/// <summary>
		/// Event called when the local player has died according to the server.
		/// </summary>
		/// <param name="spawnPos">The target spawn location of the player.</param>
		/// <param name="spawnRot">The target spawn rotation of the player.</param>
		/// <param name="killer">The character that killed the player. Can be null.</param>
		public static void LocalPlayerDied(Vector3 spawnPos, Quaternion spawnRot, ICharacter killer)
		{
			OnLocalPlayerDied(spawnPos, spawnRot, killer);
		}

		/// <summary>
		/// Event called when the local player has received an "enter lobby" event from the server.
		/// PARAMETER 1: A reference to the local player.
		/// PARAMETER 2: The end time (in ticks) of the lobby.
		/// </summary>
		public static event Action<CltPlayer, long> OnReceiveLobbyEndTime = (p, t) => { LogEvent(); };
		
		/// <summary>
		/// Event called when the local player has received an "enter lobby" event from the server.
		/// </summary>
		/// <param name="localPlayer">A reference to the local player.</param>
		/// <param name="endTime">The end time (in ticks) of the lobby.</param>
		public static void ReceiveLobbyEndTime(CltPlayer localPlayer, long endTime)
		{
			OnReceiveLobbyEndTime(localPlayer, endTime);
		}

		/// <summary>
		/// Event called when the local player's input rules have changed.
		/// PARAMETER 1: The input type that was changed.
		/// PARAMETER 2: Whether that input was enabled (true) or disabled (false).
		/// </summary>
		public static event Action<InputLevel, bool> OnInputLevelChanged = (a, b) => { LogEvent(); };

		/// <summary>
		/// Event called when the local player's input rules have changed.
		/// </summary>
		/// <param name="level">The input type that was changed.</param>
		/// <param name="state">Whether that input was enabled (true) or disabled (false).</param>
		public static void InputLevelChanged(InputLevel level, bool state)
		{
			OnInputLevelChanged(level, state);
		}
	}

	/// <summary>
	/// Holder class for all events that are designed to occur on local clients and only affect UI elements.
	/// </summary>
	public static class LocalGUI
	{
		/// <summary>
		/// Event called to push a UI hint onto the hint stack.
		/// PARAMETER 1: Which hint to push.
		/// PARAMETER 2: Whether to push or pop the given state.
		/// </summary>
		public static event Action<CrosshairHintText.Hint, bool> OnSetHintState = (h, b) => { LogEvent(); };

		/// <summary>
		/// Event called to push a UI hint onto the hint stack.
		/// </summary>
		/// <param name="hint">Which hint to push.</param>
		/// <param name="state">Whether to push or pop the given state.</param>
		public static void SetHintState(CrosshairHintText.Hint hint, bool state)
		{
			OnSetHintState(hint, state);
		}
	}

	/// <summary>
	/// Holder class for all events that are only designed to occur on the server.
	/// </summary>
	public static class Server
	{
		/// <summary>
		/// Event called when a player has joined.
		/// PARAMETER 1: The new total count of players.
		/// </summary>
		public static event Action<int> OnPlayerJoined = i => { LogEvent(); };

		/// <summary>
		/// Event called when a player has joined.
		/// </summary>
		/// <param name="newCount">The new total count of players.</param>
		public static void PlayerJoined(int newCount)
		{
			OnPlayerJoined(newCount);
		}

		/// <summary>
		/// Event called when a player has left the game.
		/// PARAMETER 1: The new total count of players.
		/// </summary>
		public static event Action<int> OnPlayerLeft = i => { LogEvent(); };

		/// <summary>
		/// Event called when a player has left the game.
		/// </summary>
		/// <param name="newCount">The new total count of players.</param>
		public static void PlayerLeft(int newCount)
		{
			OnPlayerLeft(newCount);
		}

		/// <summary>
		/// Event called when a player's health has hit zero, but they have not officially "died".
		/// PARAMETER 1: The player whose health is at zero.
		/// PARAMETER 2: The IDamageSource that caused the player's health to reach this point.
		/// </summary>
		public static event Action<CltPlayer, IDamageSource> OnPlayerHealthHitsZero = (p, r) => { LogEvent(); };

		/// <summary>
		/// Event called when a player's health has hit zero, but they have not officially "died".
		/// </summary>
		/// <param name="player">The player whose health is at zero.</param>
		/// <param name="reason">The IDamageSource that caused the player's health to reach this point.</param>
		public static void PlayerHealthHitZero(CltPlayer player, IDamageSource reason)
		{
			OnPlayerHealthHitsZero(player, reason);
		}

		/// <summary>
		/// Event called when a player's death is confirmed by the server.
		/// PARAMETER 1: The player that died.
		/// PARAMETER 2: The character that killed the player, or null.
		/// PARAMETER 3: The transform of the dead player's target respawn.
		/// </summary>
		public static event Action<CltPlayer, ICharacter, Transform> OnPlayerDied = (d, k, p) => { LogEvent(); };

		/// <summary>
		/// Event called when a player's death is confirmed by the server.
		/// </summary>
		/// <param name="deadPlayer">The player that died.</param>
		/// <param name="killer">The character that killed the player, or null.</param>
		/// <param name="respawnPosition">The transform of the dead player's target respawn.</param>
		public static void PlayerDied(CltPlayer deadPlayer, ICharacter killer, Transform respawnPosition)
		{
			OnPlayerDied(deadPlayer, killer, respawnPosition);
		}

		/// <summary>
		/// Event called when the server has entered the "in-game" state.
		/// PARAMETER 1: The time in ticks when this game will end.
		/// </summary>
		public static event Action<long> OnStartGame = d => { LogEvent(); };

		/// <summary>
		/// Event called when the server has entered the "in-game" state.
		/// </summary>
		/// <param name="endTime">The time in ticks when this game will end.</param>
		public static void StartGame(long endTime)
		{
			OnStartGame(endTime);
		}

		/// <summary>
		/// Event called when the server has determined the game is over.
		/// PARAMETER 1: The array of player scores from this game.
		/// </summary>
		public static event Action<PlayerScore[]> OnFinishGame = (s) => { LogEvent(); };

		/// <summary>
		/// Event called when the server has determined the game is over.
		/// </summary>
		/// <param name="scores">The array of player scores from this game.</param>
		public static void FinishGame(PlayerScore[] scores)
		{
			OnFinishGame(scores);
		}

		/// <summary>
		/// Event called when the server has determined that a player captured a stage.
		/// PARAMETER 1: The stage that was captured.
		/// PARAMETER 2: The player who captured the stage.
		/// </summary>
		public static event Action<StageCaptureArea, CltPlayer> OnPlayerCapturedStage = (s, p) => { LogEvent(); };

		/// <summary>
		/// Event called when the server has determined that a player captured a stage.
		/// </summary>
		/// <param name="area">The stage that was captured.</param>
		/// <param name="player">The player who captured the stage.</param>
		public static void PlayerCapturedStage(StageCaptureArea area, CltPlayer player)
		{
			OnPlayerCapturedStage(area, player);
		}

		/// <summary>
		/// Event called when the server has determined that a stage has timed out and is no longer capturable.
		/// PARAMETER 1: The stage that is no longer available.
		/// </summary>
		public static event Action<StageCaptureArea> OnStageTimedOut = (s) => { LogEvent(); };

		/// <summary>
		/// Event called when the server has determined that a stage has timed out and is no longer capturable.
		/// </summary>
		/// <param name="stage">The stage that is no longer available.</param>
		public static void StageTimedOut(StageCaptureArea stage)
		{
			OnStageTimedOut(stage);
		}
	}

	/// <summary>
	/// Unity's Start function.
	/// </summary>
	private void Start()
	{
		ServiceLocator.Get<IGameConsole>().RegisterCommand("event_fire", FireEventFromConsole);
	}

	/// <summary>
	/// Force-fire an event from the console in Debug or Development builds.
	/// Don't look at this function. Really. Just don't.
	/// </summary>
	private void FireEventFromConsole(string[] args)
	{
#if DEBUG || DEVELOPMENT_BUILD
		string method = args[0];
		MethodInfo info = GetType().GetMethod(method);
		if (info == null)
			return;

		var parameters = info.GetParameters();

		var realParameters = new List<object>();
		for (int i = 0; i < parameters.Length; i++)
		{
			Type type = parameters[i].ParameterType;
			MethodInfo parseMethod = type.GetMethod("Parse",
				BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

			if (type == typeof(string))
				realParameters.Add(args[i + 1]);
			else if (parseMethod != null)
				realParameters.Add(parseMethod.Invoke(null, new object[] { args[i + 1] }));
			else if (type.IsValueType)
				realParameters.Add(Activator.CreateInstance(type));
			else
				realParameters.Add(null);
		}

		info.Invoke(this, realParameters.ToArray());
#else
		FiringSquad.Debug.Logger.Error("Firing events is not supported in a non-development build.", Logger.System.Event);
#endif
	}
}
