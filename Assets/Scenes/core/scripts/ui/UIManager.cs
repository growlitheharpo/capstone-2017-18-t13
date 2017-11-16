using System;
using System.Collections.Generic;
using System.Linq;

namespace FiringSquad.Core.UI
{
	/// <inheritdoc cref="IUIManager" />
	public class UIManager : MonoSingleton<UIManager>, IUIManager
	{
		public static readonly int CLIP_CURRENT = "player_clip_current".GetHashCode();
		public static readonly int CLIP_TOTAL = "player_clip_total".GetHashCode();
		public static readonly int PLAYER_HEALTH = "player_health_current".GetHashCode();

		public static readonly int PLAYER_RESPAWN_TIME = "player_current_respawn_timer".GetHashCode();
		public static readonly int PLAYER_KILLS = "player_current_kills".GetHashCode();
		public static readonly int PLAYER_DEATHS = "player_current_deaths".GetHashCode();
		public static readonly int ARENA_ROUND_TIME = "arena_current_time".GetHashCode();

		/// Private variables
		private Dictionary<int, WeakReference> mPropertyMap;

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			mPropertyMap = new Dictionary<int, WeakReference>();
		}

		/// <inheritdoc />
		public BoundProperty<T> GetProperty<T>(int hash)
		{
			if (!mPropertyMap.ContainsKey(hash))
				return null;

			WeakReference reference = mPropertyMap[hash];
			if (reference != null && reference.IsAlive)
				return reference.Target as BoundProperty<T>;

			mPropertyMap.Remove(hash);
			return null;
		}

		/// <inheritdoc />
		public void BindProperty(int hash, BoundProperty prop)
		{
			mPropertyMap[hash] = new WeakReference(prop);
		}

		/// <inheritdoc />
		public void UnbindProperty(BoundProperty obj)
		{
			var keys = mPropertyMap
				.Where(x => ReferenceEquals(x.Value.Target, obj))
				.Select(x => x.Key)
				.ToArray();

			foreach (int key in keys)
				mPropertyMap.Remove(key);
		}
	}
}
