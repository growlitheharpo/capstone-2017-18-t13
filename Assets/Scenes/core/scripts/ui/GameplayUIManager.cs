using System;
using System.Collections.Generic;
using System.Linq;

namespace FiringSquad.Core.UI
{
	public class GameplayUIManager : MonoSingleton<GameplayUIManager>, IGameplayUIManager
	{
		public static readonly int CLIP_CURRENT = "player_clip_current".GetHashCode();
		public static readonly int CLIP_TOTAL = "player_clip_total".GetHashCode();
		public static readonly int PLAYER_HEALTH = "player_health_current".GetHashCode();

		public static readonly int PLAYER_RESPAWN_TIME = "player_current_respawn_timer".GetHashCode();
		public static readonly int PLAYER_KILLS = "player_current_kills".GetHashCode();
		public static readonly int PLAYER_DEATHS = "player_current_deaths".GetHashCode();
		public static readonly int ARENA_ROUND_TIME = "arena_current_time".GetHashCode();

		private Dictionary<int, WeakReference> mPropertyMap;

		protected override void Awake()
		{
			base.Awake();
			mPropertyMap = new Dictionary<int, WeakReference>();
		}

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

		public void BindProperty(int hash, BoundProperty prop)
		{
			mPropertyMap[hash] = new WeakReference(prop);
		}

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
