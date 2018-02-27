using UnityEngine;

namespace FiringSquad.Core
{
	/// <summary>
	/// A base class for managers and other singletons. Ensures only a single instance exists at startup.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		// ReSharper disable once StaticMemberInGenericType (this is the desired behavior)
		private static readonly object LOCK = new object();

		/// <summary>
		/// Determines whether this MonoSingleton should persist through scene loads. Can be
		/// overridden in a child class to return false, in which case the singleton will not persist.
		/// </summary>
		protected virtual bool dontDestroyOnLoad { get { return true; } }

		public static T instance { get; private set; }

		/// <summary>
		/// Unity's Awake function.
		/// </summary>
		protected virtual void Awake()
		{
			lock (LOCK)
			{
				// Find all objects of our type.
				var instances = FindObjectsOfType<T>();

				if (instances.Length > 1)
				{
					//If more than one exists, and we don't have an instance saved, destroy all but one and set that as our instance.
					if (instance == null)
					{
						for (int i = 1; i < instances.Length - 1; i++)
							Destroy(instances[i].gameObject);

						SetupInstance(instances[0]);
					}
					else //More than one exists and we have an instance saved. Destroy all others.
					{
						foreach (T t in instances)
						{
							if (t != instance)
								Destroy(t.gameObject);
						}
					}
				}
				else if (instances.Length == 1) // only one instance exists
					SetupInstance(instances[0]);
				else // there are no instances
					instance = null;
			}
		}

		/// <summary>
		/// Sets up the given instance to be the singleton.
		/// </summary>
		/// <param name="inst"></param>
		private void SetupInstance(T inst)
		{
			instance = inst;
			if (dontDestroyOnLoad)
			{
				instance.transform.SetParent(null);
				DontDestroyOnLoad(instance.gameObject);
			}
		}
	}
}
