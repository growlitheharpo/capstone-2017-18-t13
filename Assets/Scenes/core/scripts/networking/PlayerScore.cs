using FiringSquad.Gameplay;
using FiringSquad.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Data
{
	public struct PlayerScore : INetworkable<PlayerScore>
	{
		[SerializeField] private NetworkInstanceId mPlayerId;
		[SerializeField] private int mKills;
		[SerializeField] private int mDeaths;

		public NetworkInstanceId playerId { get { return mPlayerId; } }
		public int kills { get { return mKills; } }
		public int deaths { get { return mDeaths; } }

		public CltPlayer player
		{
			get
			{
				return ClientScene.FindLocalObject(mPlayerId).GetComponent<CltPlayer>();
			}
			set { mPlayerId = value.netId; }
		}

		public PlayerScore(NetworkInstanceId p)
		{
			mPlayerId = p;
			mKills = 0;
			mDeaths = 0;
		}

		public PlayerScore(CltPlayer p)
		{
			mPlayerId = p.netId;
			mKills = 0;
			mDeaths = 0;
		}

		public PlayerScore(NetworkInstanceId p, int kill, int death)
		{
			mPlayerId = p;
			mKills = kill;
			mDeaths = death;
		}

		public PlayerScore(CltPlayer p, int kill, int death)
		{
			mPlayerId = p.netId;
			mKills = kill;
			mDeaths = death;
		}

		public void Serialize(NetworkWriter writer)
		{
			writer.Write(mPlayerId);
			writer.Write((byte)mKills);
			writer.Write((byte)mDeaths);
		}

		public void Deserialize(NetworkReader reader, out object result)
		{
			result = new PlayerScore
			{
				mPlayerId = reader.ReadNetworkId(),
				mKills = reader.ReadByte(),
				mDeaths = reader.ReadByte()
			};
		}

		public PlayerScore Deserialize(NetworkReader reader)
		{
			object result;
			Deserialize(reader, out result);
			return (PlayerScore)result;
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
				result[i] = result[i].Deserialize(r);

			return result;
		}
	}
}
