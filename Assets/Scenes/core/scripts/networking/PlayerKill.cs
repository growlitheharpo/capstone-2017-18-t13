using System;
using FiringSquad.Gameplay;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Data
{
	[Flags]
	public enum KillFlags
	{
		None = 0x0,
		Headshot = 1 << 0,
		Multikill = 1 << 2,
		Revenge = 1 << 3,
		Killstreak = 1 << 4,
		Kingslayer = 1 << 5,
	}

	/// <summary>
	/// Utility struct for storing and serializing a player's score over the network.
	/// </summary>
	public struct PlayerKill
	{
		public Vector3 mDeathPosition;
		public Vector3 mNewSpawnPosition;
		public Quaternion mNewSpawnRotation;

		public NetworkInstanceId mKillerId;
		public KillFlags mFlags;

		/// <summary>
		/// The actual ICharacter of the killer pointed to by our killer ID.
		/// </summary>
		[CanBeNull]
		public ICharacter killer
		{
			get
			{
				if (mKillerId == NetworkInstanceId.Invalid)
					return null;

				GameObject go = NetworkManager.singleton.client != null
					? ClientScene.FindLocalObject(mKillerId)
					: NetworkServer.FindLocalObject(mKillerId);

				return go != null ? go.GetComponent<ICharacter>() : null;
			}
			set
			{
				mKillerId = value != null ? value.netId : NetworkInstanceId.Invalid;
			}
		}

		/// <summary>
		/// The respawn position for the dead player.
		/// </summary>
		public Transform spawnPosition
		{
			set
			{
				mNewSpawnPosition = value.position;
				mNewSpawnRotation = value.rotation;
			}
		}
	}
}
