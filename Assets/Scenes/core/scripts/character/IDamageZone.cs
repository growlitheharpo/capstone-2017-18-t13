﻿namespace FiringSquad.Gameplay
{
	/// <summary>
	/// Interface for all objects that can receieve damage from an IWeapon and then forward it
	/// to a damage receiever.
	/// </summary>
	public interface IDamageZone
	{
		/// <summary>
		/// The damage modification this zone provides, as a percentage.
		/// </summary>
		float damageModification { get; }

		/// <summary>
		/// The receiver that this damage zone is attached to.
		/// </summary>
		IDamageReceiver receiver { get; }
	}
}
