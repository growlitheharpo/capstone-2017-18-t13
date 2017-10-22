﻿using FiringSquad.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay
{
	/// <summary>
	/// Base Character interface.
	/// </summary>
	public interface ICharacter
	{
		NetworkInstanceId netId { get; }
		GameObject gameObject { get; }
		Transform eye { get; }

		AudioProfile audioProfile { get; }

		bool isCurrentPlayer { get; }
	}
}
