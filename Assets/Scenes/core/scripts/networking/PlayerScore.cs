﻿using System.Collections.Generic;
using FiringSquad.Gameplay;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Data
{
	/// <summary>
	/// Utility collection for storing and serializing a player's score over the network.
	/// </summary>
	public class PlayerScore
	{
		/// Inspector variables
		[SerializeField] private NetworkInstanceId mPlayerId;
		[SerializeField] private int mKills;
		[SerializeField] private int mDeaths;
		[SerializeField] private int mScore;

		private CltPlayer mPlayer;

		/// <summary>
		/// The player ID of this player.
		/// </summary>
		public NetworkInstanceId playerId { get { return mPlayerId; } }

		/// <summary>
		/// The number of other players this player has killed.
		/// </summary>
		public int kills { get { return mKills; } set { mKills = value; } }

		/// <summary>
		/// The number of times this player died.
		/// </summary>
		public int deaths { get { return mDeaths; } set { mDeaths = value; } }

		/// <summary>
		/// The score value of this player (including special bonuses)
		/// </summary>
		public int score { get { return mScore; } set { mScore = value; } }

		/// <summary>
		/// The actual player associated with this score.
		/// </summary>
		public CltPlayer player
		{
			get
			{
				return mPlayer ?? (mPlayer = ClientScene.FindLocalObject(mPlayerId).GetComponent<CltPlayer>());
			}
			set
			{
				mPlayer = value;
				mPlayerId = value.netId;
			}
		}

		/// <summary>
		/// Utility struct for storing and serializing a player's score over the network.
		/// </summary>
		public PlayerScore(NetworkInstanceId p)
		{
			mPlayerId = p;
			mKills = 0;
			mDeaths = 0;
			mScore = 0;
		}

		/// <summary>
		/// Utility struct for storing and serializing a player's score over the network.
		/// </summary>
		public PlayerScore(CltPlayer p)
		{
			mPlayerId = p.netId;
			mPlayer = p;
			mKills = 0;
			mDeaths = 0;
			mScore = 0;
		}

		/// <summary>
		/// Utility struct for storing and serializing a player's score over the network.
		/// </summary>
		public PlayerScore(NetworkInstanceId p, int kill, int death, int score)
		{
			mPlayerId = p;
			mKills = kill;
			mDeaths = death;
			mScore = score;
		}

		/// <summary>
		/// Utility struct for storing and serializing a player's score over the network.
		/// </summary>
		public PlayerScore(CltPlayer p, int kill, int death, int score)
		{
			mPlayerId = p.netId;
			mPlayer = p;
			mKills = kill;
			mDeaths = death;
			mScore = score;
		}

		private PlayerScore(NetworkReader reader)
		{
			mPlayerId = reader.ReadNetworkId();
			mKills = reader.ReadByte();
			mDeaths = reader.ReadByte();
			mScore = reader.ReadInt16();
		}

		/// <summary>
		/// Write this object to the provided stream.
		/// </summary>
		public void Serialize(NetworkWriter writer)
		{
			writer.Write(mPlayerId);
			writer.Write((byte)mKills);
			writer.Write((byte)mDeaths);
			writer.Write((short)mScore);
		}

		/// <summary>
		/// Read an object from the provided stream and write it to the target.
		/// </summary>
		public void Deserialize(NetworkReader reader, out object result)
		{
			result = new PlayerScore(reader);
		}

		/// <summary>
		/// Read an object from the provided stream and return it.
		/// </summary>
		public PlayerScore Deserialize(NetworkReader reader)
		{
			object result;
			Deserialize(reader, out result);
			return (PlayerScore)result;
		}

		/// <summary>
		/// Serialize an array of PlayerScores using the instance method in serial.
		/// </summary>
		public static byte[] SerializeArray(PlayerScore[] array)
		{
			NetworkWriter w = new NetworkWriter();
			w.Write(array.Length);

			foreach (PlayerScore score in array)
				score.Serialize(w);

			return w.ToArray();
		}

		/// <summary>
		/// Deserialize an array of PlayerScores using the instance method in serial.
		/// </summary>
		public static List<PlayerScore> DeserializeArray(byte[] array)
		{
			NetworkReader r = new NetworkReader(array);
			int size = r.ReadInt32();

			var result = new List<PlayerScore>(size);
			for (int i = 0; i < size; i++)
				result.Add(new PlayerScore(r));

			return result;
		}
	}
}
