using System;
using System.Collections.Generic;
using FiringSquad.Gameplay.Weapons;
using UnityEngine;

namespace FiringSquad.Data
{
	/// <summary>
	/// Serializable utility class that stores a collection of weapon parts.
	/// </summary>
	[Serializable]
	public class WeaponPartCollection
	{
		[SerializeField] private WeaponPartScriptScope mScope;
		[SerializeField] private WeaponPartScriptBarrel mBarrel;
		[SerializeField] private WeaponPartScriptMechanism mMechanism;
		[SerializeField] private WeaponPartScriptGrip mGrip;

		public WeaponPartScriptScope scope { get { return mScope; } }
		public WeaponPartScriptBarrel barrel { get { return mBarrel; } }
		public WeaponPartScriptMechanism mechanism { get { return mMechanism; } }
		public WeaponPartScriptGrip grip { get { return mGrip; } }

		public WeaponPartScript[] allParts { get { return new WeaponPartScript[] { scope, barrel, mechanism, grip }; } }

		public struct GameObjects
		{
			private readonly WeaponPartCollection mCol;

			public GameObjects(WeaponPartCollection c)
			{
				mCol = c;
			}

			public GameObject scope { get { return mCol.mScope.gameObject; } }
			public GameObject barrel { get { return mCol.mBarrel.gameObject; } }
			public GameObject mechanism { get { return mCol.mMechanism.gameObject; } }
			public GameObject grip { get { return mCol.mGrip.gameObject; } }

			public IEnumerator<GameObject> GetEnumerator()
			{
				yield return mCol.scope.gameObject;
				yield return mCol.barrel.gameObject;
				yield return mCol.mechanism.gameObject;
				yield return mCol.grip.gameObject;
			}

			/// <summary>
			/// Allows access to weapon parts by their attachment.
			/// </summary>
			public GameObject this[BaseWeaponScript.Attachment index]
			{
				get
				{
					switch (index)
					{
						case BaseWeaponScript.Attachment.Scope:
							return mCol.mScope.gameObject;
						case BaseWeaponScript.Attachment.Barrel:
							return mCol.mBarrel.gameObject;
						case BaseWeaponScript.Attachment.Mechanism:
							return mCol.mMechanism.gameObject;
						case BaseWeaponScript.Attachment.Grip:
							return mCol.mGrip.gameObject;
						default:
							throw new ArgumentOutOfRangeException("index", index, null);
					}
				}
			}
		}

		public GameObjects gameObjects { get { return new GameObjects(this);} }

		public WeaponPartCollection()
		{
			mScope = null;
			mBarrel = null;
			mMechanism = null;
			mGrip = null;
		}
		
		public WeaponPartCollection(WeaponPartCollection copy)
		{
			mScope = copy.mScope;
			mBarrel = copy.mBarrel;
			mMechanism = copy.mMechanism;
			mGrip = copy.mGrip;
		}

		public WeaponPartCollection(GameObject mechanism1, GameObject barrel1, GameObject scope1, GameObject grip1)
		{
			mMechanism = mechanism1.GetComponent<WeaponPartScriptMechanism>();
			mBarrel = barrel1.GetComponent<WeaponPartScriptBarrel>();
			mScope = scope1.GetComponent<WeaponPartScriptScope>();
			mGrip = grip1.GetComponent<WeaponPartScriptGrip>();
		}

		public WeaponPartCollection(WeaponPartScriptMechanism m, WeaponPartScriptBarrel b, WeaponPartScriptScope s, WeaponPartScriptGrip g)
		{
			mMechanism = m;
			mBarrel = b;
			mScope = s;
			mGrip = g;
		}

		public IEnumerator<WeaponPartScript> GetEnumerator()
		{
			yield return scope;
			yield return barrel;
			yield return mechanism;
			yield return grip;
		}

		public WeaponPartScript this[BaseWeaponScript.Attachment index]
		{
			get
			{
				switch (index)
				{
					case BaseWeaponScript.Attachment.Scope:
						return scope;
					case BaseWeaponScript.Attachment.Barrel:
						return barrel;
					case BaseWeaponScript.Attachment.Mechanism:
						return mechanism;
					case BaseWeaponScript.Attachment.Grip:
						return grip;
					default:
						throw new ArgumentOutOfRangeException("index", index, null);
				}
			}
			set
			{
				switch (index)
				{
					case BaseWeaponScript.Attachment.Scope:
						mScope = value as WeaponPartScriptScope;
						break;
					case BaseWeaponScript.Attachment.Barrel:
						mBarrel = value as WeaponPartScriptBarrel;
						break;
					case BaseWeaponScript.Attachment.Mechanism:
						mMechanism = value as WeaponPartScriptMechanism;
						break;
					case BaseWeaponScript.Attachment.Grip:
						mGrip = value as WeaponPartScriptGrip;
						break;
					default:
						throw new ArgumentOutOfRangeException("index", index, null);
				}
			}
		}
	}
}
