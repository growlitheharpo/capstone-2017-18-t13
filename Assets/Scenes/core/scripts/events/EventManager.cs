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
		public static event Action<long> OnReceiveGameEndTime = t => { LogEvent(); };

		/// <summary>
		/// Event called when this player has received a start event from the server.
		/// </summary>
		/// <param name="endTime">The end time in ticks that the game should end.</param>
		public static void ReceiveGameEndTime(long endTime)
		{
			OnReceiveGameEndTime(endTime);
		}

		/// <summary>
		/// Event called when the player has received the end game event from the server.
		/// PARAMETER 1: The list of player scores received from the server.
		/// </summary>
		public static event Action<IList<PlayerScore>> OnReceiveFinishEvent = (s) => { LogEvent(); };

		/// <summary>
		/// Event called when the player has received the end game event from the server.
		/// </summary>
		/// <param name="scores">The list of player scores received from the server.</param>
		public static void ReceiveFinishEvent(IList<PlayerScore> scores)
		{
			OnReceiveFinishEvent(scores);
		}

		/// <summary>
		/// Event called when the player has confirmed they want to quit a game
		/// in-progress, either through the pause menu or through the "game over" panel.
		/// </summary>
		public static event Action OnConfirmQuitGame = () => { LogEvent(); };

		/// <summary>
		/// Event called when the player has confirmed they want to quit a game
		/// in-progress, either through the pause menu or through the "game over" panel.
		/// </summary>
		public static void ConfirmQuitGame()
		{
			OnConfirmQuitGame();
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
		/// Event called when the local player has killed another character.
		/// PARAMETER 1: The other character that was killed.
		/// PARAMETER 2: The player's current weapon.
		/// PARAMETER 3: The flags tied to this kill.
		/// </summary>
		public static event Action<CltPlayer, IWeapon, KillFlags> OnLocalPlayerGotKill = (d, w, f) => { LogEvent(); };

		/// <summary>
		/// Event called when the local player has killed another character.
		/// </summary>
		/// <param name="deadPlayer">The other character that was killed.</param>
		/// <param name="currentWeapon">The player's current weapon.</param>
		/// <param name="killInfoFlags">The flags tied to this kill.</param>
		public static void LocalPlayerGotKill(CltPlayer deadPlayer, IWeapon currentWeapon, KillFlags killInfoFlags)
		{
			OnLocalPlayerGotKill(deadPlayer, currentWeapon, killInfoFlags);
		}

		/// <summary>
		/// Event called when the local player has died according to the server.
		/// PARAMETER 1: The PlayerKill data related to this kill.
		/// PARAMETER 2: The character that killed the player. Can be null.
		/// </summary>
		public static event Action<PlayerKill, ICharacter> OnLocalPlayerDied = (ki, k) => { LogEvent(); };

		/// <summary>
		/// Event called when the local player has died according to the server.
		/// </summary>
		/// <param name="killInfo">The PlayerKill data related to this kill.</param>
		/// <param name="killer">The character that killed the player. Can be null.</param>
		public static void LocalPlayerDied(PlayerKill killInfo, ICharacter killer)
		{
			OnLocalPlayerDied(killInfo, killer);
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

		public static event Action<float, CltPlayer> OnZoomLevelChanged = (a, b) => { LogEvent(); };

		/// <summary>
		/// Event called when the player changes their zoom level in the variable sight
		/// </summary>
		/// <param name="zoom"></param>
		/// <param name="player"></param>
		public static void ZoomLevelChanged(float zoom, CltPlayer player)
		{
			OnZoomLevelChanged(zoom, player);
		}

		/// <summary>
		/// Event called when the server has notified this client to play its intro sequence
		/// </summary>
		public static event Action OnReceiveStartIntroNotice = () => { LogEvent(); };

		/// <summary>
		/// Event called when the server has notified this client to play its intro sequence
		/// </summary>
		public static void ReceiveStartIntroNotice()
		{
			OnReceiveStartIntroNotice();
		}

		/// <summary>
		/// Event called when Timeline has activated the intro sequence.
		/// </summary>
		public static event Action OnIntroBegin = () => { LogEvent(); };

		/// <summary>
		/// Event called when Timeline has activated the intro sequence.
		/// </summary>
		public static void IntroBegin()
		{
			OnIntroBegin();
		}

		/// <summary>
		/// Event called when Timeline has completed the intro sequence.
		/// </summary>
		public static event Action OnIntroEnd = () => { LogEvent(); };

		/// <summary>
		/// Event called when Timeline has completed the intro sequence.
		/// </summary>
		public static void IntroEnd()
		{
			OnIntroEnd();
		}

		/// <summary>
		/// Event called when the server confirms that a local player has captured a stage.
		/// </summary>
		public static event Action OnLocalPlayerCapturedStage = () => { LogEvent(); };

		/// <summary>
		/// Event called when the server confirms that a local player has captured a stage.
		/// </summary>
		public static void LocalPlayerCapturedStage()
		{
			OnLocalPlayerCapturedStage();
		}
		public static event Action<IList<PlayerScore>> OnTeamVictoryScreen = (s) => { LogEvent(); };

		public static void TeamVictoryScreen(IList<PlayerScore> score)
		{
			OnTeamVictoryScreen(score);
		}

		/// <summary>
		/// Event called when the player equips a rocket grip
		/// </summary>
		public static event Action OnEquipRocketGrip = () => { LogEvent(); };

		public static void EquipRocketGrip()
		{
			OnEquipRocketGrip();
		}

		/// <summary>
		/// Event called when the player unequips the rocket grip
		/// </summary>
		public static event Action OnUnequipRocketGrip = () => { LogEvent(); };

		public static void UnequipRocketGrip()
		{
			OnUnequipRocketGrip();
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
		/// Event called to update all UI elements when the player has been assigned a team.
		/// PARAMETER 1: A reference to the local player.
		/// </summary>
		public static event Action<CltPlayer> OnLocalPlayerAssignedTeam = p => { LogEvent(); };

		/// <summary>
		/// Event called to update all UI elements when the player has been assigned a team.
		/// </summary>
		/// <param name="localPlayer">A reference to the local player.</param>
		public static void LocalPlayerAssignedTeam(CltPlayer localPlayer)
		{
			OnLocalPlayerAssignedTeam(localPlayer);
		}

		/// <summary>
		/// Event called to push a UI hint onto the hint stack.
		/// </summary>
		/// <param name="hint">Which hint to push.</param>
		/// <param name="state">Whether to push or pop the given state.</param>
		public static void SetHintState(CrosshairHintText.Hint hint, bool state)
		{
			OnSetHintState(hint, state);
		}

		/// <summary>
		/// Event called when the player's dynamic crosshair should change state based on a game event.
		/// </summary>
		public static event Action<bool> OnSetCrosshairVisible = (b) => { LogEvent(); };
		
		/// <summary>
		/// Event called when the player's dynamic crosshair should change state based on a game event.
		/// </summary>
		public static void SetCrosshairVisible(bool visible)
		{
			OnSetCrosshairVisible(visible);
		}
		
		/// <summary>
		/// Event called to lerp the camera's field of view to a new value.
		/// PARAMETER 1: The new target field of view. A value less than 0 means the default.
		/// PARAMETER 2: The time to lerp over. A value less than or equal to 0 means instant.
		/// </summary>
		public static event Action<float, float> OnRequestNewFieldOfView = (f, t) => { LogEvent(); };

		/// <summary>
		/// Event called to lerp the camera's field of view to a new value.
		/// </summary>
		/// <param name="fov">The new target field of view. A value less than 0 means the default.</param>
		/// <param name="time">The time to lerp over. A value less than or equal to 0 means instant.</param>
		public static void RequestNewFieldOfView(float fov, float time)
		{
			OnRequestNewFieldOfView(fov, time);
		}
	}

	/// <summary>
	/// Event called to notify different parts of the game that an event happened to ANY player,
	/// not necessarily the local one.
	/// </summary>
	public static class LocalGeneric
	{
		/// <summary>
		/// Event called when ANY player has captured a stage area.
		/// </summary>
		public static event Action OnPlayerCapturedStage = () => { LogEvent(); };
		
		/// <summary>
		/// Event called when ANY player has captured a stage area.
		/// </summary>
		public static void PlayerCapturedStage()
		{
			OnPlayerCapturedStage();
		}

		/// <summary>
		/// Event called when ANY player has died.
		/// </summary>
		public static event Action<CltPlayer> OnPlayerDied = (p) => { LogEvent(); };
		
		/// <summary>
		/// Event called when ANY player has died.
		/// </summary>
		public static void PlayerDied(CltPlayer player)
		{
			OnPlayerDied(player);
		}

		/// <summary>
		/// Event called when ANY player has equipped a legendary part.
		/// </summary>
		public static event Action<CltPlayer> OnPlayerEquippedLegendaryPart = (p) => { LogEvent(); };
		
		/// <summary>
		/// Event called when ANY player has equipped a legendary part.
		/// </summary>
		public static void PlayerEquippedLegendaryPart(CltPlayer player)
		{
			OnPlayerEquippedLegendaryPart(player);
		}

		/// <summary>
		/// Event called when ANY player's score has changed, not just the local player.
		/// </summary>
		/// PARAMETER 1: Which player changed.
		/// PARAMETER 2: The amount of score change.
		/// PARAMETER 3: The amount of change to kill count.
		/// PARAMETER 4: The amount of change to death count.
		public static event Action<CltPlayer, int, int, int> OnPlayerScoreChanged = (p, x, y, z) => { LogEvent(); };

		/// <summary>
		/// Event called when ANY player's score has changed, not just the local player.
		/// </summary>
		/// <param name="player">Which player changed.</param>
		/// <param name="scoreChange">The amount of score change.</param>
		/// <param name="killChange">The amount of change to kill count.</param>
		/// <param name="deathChange">The amount of change to death count.</param>
		public static void PlayerScoreChanged(CltPlayer player, int scoreChange, int killChange, int deathChange)
		{
			OnPlayerScoreChanged(player, scoreChange, killChange, deathChange);
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
		/// PARAMETER 2: A reference to the new player.
		/// </summary>
		public static event Action<int, CltPlayer> OnPlayerJoined = (i, p) => { LogEvent(); };

		/// <summary>
		/// Event called when a player has joined.
		/// </summary>
		/// <param name="newCount">The new total count of players.</param>
		/// <param name="newPlayer">A reference to the new player.</param>
		public static void PlayerJoined(int newCount, CltPlayer newPlayer)
		{
			OnPlayerJoined(newCount, newPlayer);
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
		/// PARAMETER 3: True if the final shot was a headshot.
		/// </summary>
		public static event Action<CltPlayer, IDamageSource, bool> OnPlayerHealthHitsZero = (p, r, h) => { LogEvent(); };

		/// <summary>
		/// Event called when a player's health has hit zero, but they have not officially "died".
		/// </summary>
		/// <param name="player">The player whose health is at zero.</param>
		/// <param name="reason">The IDamageSource that caused the player's health to reach this point.</param>
		/// <param name="wasHeadshot">True if the final shot was a headshot.</param>
		public static void PlayerHealthHitZero(CltPlayer player, IDamageSource reason, bool wasHeadshot)
		{
			OnPlayerHealthHitsZero(player, reason, wasHeadshot);
		}

		/// <summary>
		/// Event called when a player's death is confirmed by the server.
		/// PARAMETER 1: The player that died.
		/// PARAMETER 2: The information about the kill that took place.
		/// </summary>
		public static event Action<CltPlayer, PlayerKill> OnPlayerDied = (dp, ki) => { LogEvent(); };

		/// <summary>
		/// Event called when a player's death is confirmed by the server.
		/// </summary>
		/// <param name="deadPlayer">The player that died.</param>
		/// <param name="killInfo">The information about the kill that took place.</param>
		public static void PlayerDied(CltPlayer deadPlayer, PlayerKill killInfo)
		{
			OnPlayerDied(deadPlayer, killInfo);
		}

		/// <summary>
		/// Event called by the server to tell all clients to start their intro sequence
		/// </summary>
		public static event Action OnStartIntroSequence = () => { LogEvent(); };

		/// <summary>
		/// Event called by the server to tell all clients to start their intro sequence
		/// </summary>
		public static void StartIntroSequence()
		{
			OnStartIntroSequence();
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
		public static event Action<StageCaptureArea, IList<CltPlayer>> OnPlayerCapturedStage = (s, p) => { LogEvent(); };

		/// <summary>
		/// Event called when the server has determined that a player captured a stage.
		/// </summary>
		/// <param name="area">The stage that was captured.</param>
		/// <param name="player">The player who captured the stage.</param>
		public static void PlayerCapturedStage(StageCaptureArea area, IList<CltPlayer> player)
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

		public static event Action<PlayerHealthPack> OnHealthPickedUp = a => { LogEvent(); };

		/// <summary>
		/// Event called when a player picks up a healthpack
		/// </summary>
		/// <param name="pack"></param>
		public static void HealthPickedUp(PlayerHealthPack pack)
		{
			OnHealthPickedUp(pack);
		}

		public static event Action<WeaponPartPad> OnPartPickedUp = a => { LogEvent(); };

		/// <summary>
		/// Event called when a player picks up a part from a pad
		/// </summary>
		/// <param name="pad"></param>
		public static void PartPickedUp(WeaponPartPad pad)
		{
			OnPartPickedUp(pad);
		}

		/// <summary>
		/// Event called on the server when a player has attached a new part.
		/// PARAMETER 1: The weapon that this part was attached to.
		/// PARAMETER 2: The part instance that was attached.
		/// PARAMETER 3: The bearer that attached the new part.
		/// </summary>
		public static event Action<BaseWeaponScript, WeaponPartScript> OnPlayerAttachedPart = (w, p) => { LogEvent(); };

		/// <summary>
		/// Event called on the server when a player has attached a new part.
		/// </summary>
		/// <param name="baseWeaponScript">The weapon that this part was attached to.</param>
		/// <param name="weaponPartScript">The part instance that was attached.</param>
		/// <param name="bearer">The bearer that attached the new part.</param>
		public static void PlayerAttachedPart(BaseWeaponScript baseWeaponScript, WeaponPartScript weaponPartScript)
		{
			OnPlayerAttachedPart(baseWeaponScript, weaponPartScript);
		}

		/// <summary>
		/// Event called when a player has cheated by using the debug menu.
		/// </summary>
		public static event Action<CltPlayer> OnPlayerCheated = p => { LogEvent(); };

		/// <summary>
		/// Event called when a player has cheated by using the debug menu.
		/// </summary>
		public static void PlayerCheated(CltPlayer player)
		{
			OnPlayerCheated(player);
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
		FiringSquad.Debug.Logger.Error("Firing events is not supported in a non-development build.", FiringSquad.Debug.Logger.System.Event);
#endif
	}
}
