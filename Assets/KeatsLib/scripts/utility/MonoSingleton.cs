using UnityEngine;

/// <summary>
/// A base class for managers and other singletons. Ensures only a single instance exists at startup.
/// </summary>
/// <typeparam name="T"></typeparam>
public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
	// ReSharper disable once StaticMemberInGenericType (this is the desired behavior)
	private static readonly object LOCK = new object();

	public static T instance { get; private set; }

	protected virtual void Awake()
	{
		lock (LOCK)
		{
			var instances = FindObjectsOfType<T>();
			if (instances.Length > 1)
			{
				if (instance == null)
				{
					for (int i = 1; i < instances.Length - 1; i++)
						Destroy(instances[i].gameObject);
					
					SetupInstance(instances[0]);
				}
				else
				{
					foreach (T t in instances)
					{
						if (t != instance)
							Destroy(t.gameObject);
					}
				}
			}
			else if (instances.Length == 1)
				SetupInstance(instances[0]);
			else
				instance = null;
		}
	}

	private void SetupInstance(T inst)
	{
		instance = inst;
		instance.transform.SetParent(null);
		DontDestroyOnLoad(instance.gameObject);
	}
}
