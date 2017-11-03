using FiringSquad.Gameplay;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Data
{
	public class PlayerScore
	{
		[SerializeField] private NetworkInstanceId mPlayerId;
		[SerializeField] private byte mKills;
		[SerializeField] private byte mDeaths;

		public NetworkInstanceId playerId { get; set; }
		public byte kills { get { return mKills; } set { mKills = value; } }
		public byte deaths { get { return mDeaths; } set { mDeaths = value; } }

		public CltPlayer player
		{
			get
			{
				return ClientScene.FindLocalObject(mPlayerId).GetComponent<CltPlayer>();
			}
			set { mPlayerId = value.netId; }
		}

		public PlayerScore() { }

		public PlayerScore(NetworkInstanceId p)
		{
			mPlayerId = p;
		}

		public PlayerScore(CltPlayer p)
		{
			player = p;
		}

		public PlayerScore(NetworkInstanceId p, byte kill, byte death)
		{
			mPlayerId = p;
			mKills = kill;
			mDeaths = death;
		}

		public PlayerScore(CltPlayer p, byte kill, byte death)
		{
			player = p;
			mKills = kill;
			mDeaths = death;
		}

		public void Serialize(NetworkWriter writer)
		{
			writer.Write(mPlayerId);
			writer.Write(mKills);
			writer.Write(mDeaths);
		}

		public static PlayerScore Deserialize(NetworkReader reader)
		{
			PlayerScore result = new PlayerScore
			{
				mPlayerId = reader.ReadNetworkId(),
				mKills = reader.ReadByte(),
				mDeaths = reader.ReadByte()
			};

			return result;
		}

		public static byte[] SerializeArray(PlayerScore[] array)
		{
			NetworkWriter w = new NetworkWriter();
			w.Write(array.Length);

			foreach (PlayerScore score in array)
				score.Serialize(w);

			return w.ToArray();
		}

		public static PlayerScore[] DeserializeArray(byte[] array)
		{
			NetworkReader r = new NetworkReader(array);
			int size = r.ReadInt32();

			var result = new PlayerScore[size];
			for (int i = 0; i < size; i++)
				result[i] = Deserialize(r);

			return result;
		}
	}
}
