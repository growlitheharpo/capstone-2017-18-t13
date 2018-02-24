﻿using System;
using System.Collections.Generic;
using FiringSquad.Gameplay.Weapons;
using UnityEngine;

namespace FiringSquad.Data
{
	/// <summary>
	/// A collection of enums for the game rules.
	/// </summary>
	public class GameData
	{
		public enum MatchType
		{
			Deathmatch,
			TeamDeathmatch,
		}

		public enum PlayerTeam
		{
			Deathmatch = 0,
			Blue = 1,
			Orange = 2,
		}
	}

	/// <summary>
	/// The collection of variables used by the NetworkServerGameManager to manage the game's rules.
	/// </summary>
	[Serializable]
	public class ServerGameDefaultData
	{
		[SerializeField] private GameData.MatchType mCurrentType;
		[SerializeField] private List<WeaponPartScript> mStageCaptureParts;
		[SerializeField] private float mMinStageWaitTime;
		[SerializeField] private float mMaxStageWaitTime;
		[SerializeField] private int mRoundTime;
		[SerializeField] private int mLobbyTime;
		[SerializeField] private int mGoalPlayerCount;

		/// <summary>
		/// The list of parts that can be used for when a stage is captured.
		/// </summary>
		public List<WeaponPartScript> stageCaptureParts { get { return mStageCaptureParts; } }

		/// <summary>
		/// The minimum wait for a stage to spawn.
		/// </summary>
		public float minStageWaitTime { get { return mMinStageWaitTime; } }

		/// <summary>
		/// The maximum wait for a stage to spawn.
		/// </summary>
		public float maxStageWaitTime { get { return mMaxStageWaitTime; } }

		/// <summary>
		/// The length of a round, in seconds.
		/// </summary>
		public int roundTime { get { return mRoundTime; } }

		/// <summary>
		/// The length of the lobby, in seconds.
		/// </summary>
		public int lobbyTime { get { return mLobbyTime; } }

		/// <summary>
		/// How many players to wait for before the match starts.
		/// </summary>
		public int goalPlayerCount { get { return mGoalPlayerCount; } }

		/// <summary>
		/// Which gametype we are currently playing.
		/// </summary>
		public GameData.MatchType currentType { get { return mCurrentType; } }
	}
}