﻿using UnityEngine;

namespace FiringSquad.Gameplay
{
	/// <summary>
	/// Interface for all objects that can receive damage from IWeapons.
	/// </summary>
	public interface IDamageReceiver
	{
		/// <summary>
		/// The GameObject that this DamageReceiver is attached to.
		/// </summary>
		GameObject gameObject { get; }

		/// <summary>
		/// Apply damage to this receiver.
		/// </summary>
		/// <param name="amount">The amount of damage to apply.</param>
		/// <param name="point">The world position where this damage occurred.</param>
		/// <param name="normal">The normal/tangent of the hit.</param>
		/// <param name="cause">The source of this damage.</param>
		/// <param name="wasHeadshot">True if this shot was on headshot damage zone.</param>
		void ApplyDamage(float amount, Vector3 point, Vector3 normal, IDamageSource cause, bool wasHeadshot);

		/// <summary>
		/// Heal a certain amount of health to this receiver.
		/// </summary>
		/// <param name="amount">The amount of health to heal.</param>
		void HealDamage(float amount);

		/// <summary>
		/// The current health value of this damage receiver.
		/// </summary>
		float currentHealth { get; }
	}
}
