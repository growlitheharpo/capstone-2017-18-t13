using System;
using System.Collections.Generic;

public class GameplayUIManager : MonoSingleton<GameplayUIManager>, IGameplayUIManager
{
	public static readonly int CLIP_CURRENT = "player_clip_current".GetHashCode();
	public static readonly int CLIP_TOTAL = "player_clip_total".GetHashCode();
	public static readonly int PLAYER_HEALTH = "player_health_current".GetHashCode();

	public static readonly int PLAYER1_SCORE = "player_1_score".GetHashCode();
	public static readonly int PLAYER2_SCORE = "player_2_score".GetHashCode();
	public static readonly int ARENA_ROUND_TIME = "arena_current_time".GetHashCode();

	private Dictionary<int, WeakReference> mPropertyMap;

	protected override void Awake()
	{
		base.Awake();
		mPropertyMap = new Dictionary<int, WeakReference>();
	}

	private void Start()
	{
		EventManager.OnBoundPropertyCreated += BoundPropertyCreated;
	}

	private void OnDestroy()
	{
		EventManager.OnBoundPropertyCreated -= BoundPropertyCreated;
	}

	private void BoundPropertyCreated(BoundProperty boundProperty, int i)
	{
		mPropertyMap[i] = new WeakReference(boundProperty);
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
}
