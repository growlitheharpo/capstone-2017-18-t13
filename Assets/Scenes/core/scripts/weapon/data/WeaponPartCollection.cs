using System;
using System.Collections;
using System.Collections.Generic;
using FiringSquad.Gameplay.Weapons;
using JetBrains.Annotations;
using UnityEngine;

namespace FiringSquad.Data
{
	/// <summary>
	/// Serializable utility class that stores a collection of weapon parts.
	/// Extends IEnumerable for easier iteration.
	/// </summary>
	[Serializable]
	public class WeaponPartCollection : IEnumerable<WeaponPartScript>
	{
		/// Inspector variables
		[SerializeField] private WeaponPartScriptMechanism mMechanism;
		[SerializeField] private WeaponPartScriptBarrel mBarrel;
		[SerializeField] private WeaponPartScriptScope mScope;
		[SerializeField] private WeaponPartScriptGrip mGrip;

		[CanBeNull] public WeaponPartScriptMechanism mechanism { get { return mMechanism; } }
		[CanBeNull] public WeaponPartScriptBarrel barrel { get { return mBarrel; } }
		[CanBeNull] public WeaponPartScriptScope scope { get { return mScope; } }
		[CanBeNull] public WeaponPartScriptGrip grip { get { return mGrip; } }

		/// <summary>
		/// Returns an array of all the weapon parts in the following order: mechanism, barrel, scope, grip
		/// </summary>
		public WeaponPartScript[] allParts { get { return new WeaponPartScript[] { mechanism, barrel, scope, grip }; } }

		/// <summary>
		/// Serializable utility class that stores a collection of weapon parts.
		/// Extends IEnumerable for easier iteration.
		/// </summary>
		public WeaponPartCollection()
		{
			mScope = null;
			mBarrel = null;
			mMechanism = null;
			mGrip = null;
		}

		/// <summary>
		/// Serializable utility class that stores a collection of weapon parts.
		/// Extends IEnumerable for easier iteration.
		/// </summary>
		/// <param name="copy">The collection to copy all references from.</param>
		public WeaponPartCollection(WeaponPartCollection copy)
		{
			mScope = copy.mScope;
			mBarrel = copy.mBarrel;
			mMechanism = copy.mMechanism;
			mGrip = copy.mGrip;
		}

		/// <summary>
		/// Serializable utility class that stores a collection of weapon parts.
		/// Extends IEnumerable for easier iteration.
		/// </summary>
		/// <param name="mechanism1">The mechanism to store.</param>
		/// <param name="barrel1">The barrel to store.</param>
		/// <param name="scope1">The scope to store.</param>
		/// <param name="grip1">The grip to store.</param>
		public WeaponPartCollection(GameObject mechanism1, GameObject barrel1, GameObject scope1, GameObject grip1)
		{
			mMechanism = mechanism1.GetComponent<WeaponPartScriptMechanism>();
			mBarrel = barrel1.GetComponent<WeaponPartScriptBarrel>();
			mScope = scope1.GetComponent<WeaponPartScriptScope>();
			mGrip = grip1.GetComponent<WeaponPartScriptGrip>();
		}

		/// <summary>
		/// Serializable utility class that stores a collection of weapon parts.
		/// Extends IEnumerable for easier iteration.
		/// </summary>
		/// <param name="m">The mechanism to store.</param>
		/// <param name="b">The barrel to store.</param>
		/// <param name="s">The scope to store.</param>
		/// <param name="g">The grip to store.</param>
		public WeaponPartCollection(WeaponPartScriptMechanism m, WeaponPartScriptBarrel b, WeaponPartScriptScope s, WeaponPartScriptGrip g)
		{
			mMechanism = m;
			mBarrel = b;
			mScope = s;
			mGrip = g;
		}

		/// <summary>
		/// Allows this collection to be iterated over using a foreach loop.
		/// </summary>
		public IEnumerator<WeaponPartScript> GetEnumerator()
		{
			yield return mechanism;
			yield return barrel;
			yield return scope;
			yield return grip;
		}

		/// <summary>
		/// Allows this collection to be iterated over using a foreach loop.
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Access a particular attachment using its enum Attachment.
		/// </summary>
		/// <param name="index">Which attachment slot to access.</param>
		public WeaponPartScript this[Attachment index]
		{
			get
			{
				switch (index)
				{
					case Attachment.Scope:
						return scope;
					case Attachment.Barrel:
						return barrel;
					case Attachment.Mechanism:
						return mechanism;
					case Attachment.Grip:
						return grip;
					default:
						throw new ArgumentOutOfRangeException("index", index, null);
				}
			}
			set
			{
				switch (index)
				{
					case Attachment.Scope:
						mScope = value as WeaponPartScriptScope;
						break;
					case Attachment.Barrel:
						mBarrel = value as WeaponPartScriptBarrel;
						break;
					case Attachment.Mechanism:
						mMechanism = value as WeaponPartScriptMechanism;
						break;
					case Attachment.Grip:
						mGrip = value as WeaponPartScriptGrip;
						break;
					default:
						throw new ArgumentOutOfRangeException("index", index, null);
				}
			}
		}
	}
}
