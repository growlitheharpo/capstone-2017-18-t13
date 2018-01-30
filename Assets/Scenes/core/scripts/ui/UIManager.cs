using System;
using System.Collections.Generic;
using System.Linq;
using FiringSquad.Core.Input;
using KeatsLib.Collections;
using UnityEngine;

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
		private Dictionary<ScreenPanelTypes, IScreenPanel> mPanelTypeToObjectMap;
		private Dictionary<IScreenPanel, ScreenPanelTypes> mPanelObjectToTypeMap;
		private UniqueStack<GameObject> mActivePanels;

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			mPropertyMap = new Dictionary<int, WeakReference>();
			mPanelObjectToTypeMap = new Dictionary<IScreenPanel, ScreenPanelTypes>();
			mPanelTypeToObjectMap = new Dictionary<ScreenPanelTypes, IScreenPanel>();
			mActivePanels = new UniqueStack<GameObject>();
		}

		#region Bound Properties

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

		#endregion

		#region Panel Stack Management

		/// <inheritdoc />
		public IScreenPanel PushNewPanel(ScreenPanelTypes type)
		{
			if (!mPanelTypeToObjectMap.ContainsKey(type))
				return null;

			IScreenPanel panel = mPanelTypeToObjectMap[type];
			GameObject go = panel.gameObject;

			go.SetActive(true);
			panel.OnEnablePanel();

			ServiceLocator.Get<IInput>()
				.DisableInputLevel(InputLevel.Gameplay)
				.DisableInputLevel(InputLevel.HideCursor);
			
			mActivePanels.Push(go);
			go.transform.SetAsLastSibling();

			return panel;
		}

		/// <inheritdoc />
		public IUIManager PopPanel(ScreenPanelTypes type)
		{
			if (!mPanelTypeToObjectMap.ContainsKey(type))
				return this;

			IScreenPanel panel = mPanelTypeToObjectMap[type];
			GameObject go = panel.gameObject;

			panel.OnDisablePanel();
			go.SetActive(false);
			mActivePanels.Remove(go);

			if (mActivePanels.Count == 0)
			{
				ServiceLocator.Get<IInput>()
					.EnableInputLevel(InputLevel.Gameplay)
					.EnableInputLevel(InputLevel.HideCursor);
			}

			return this;
		}

		public IScreenPanel TogglePanel(ScreenPanelTypes type)
		{
			if (!mPanelTypeToObjectMap.ContainsKey(type))
				return null;

			IScreenPanel panel = mPanelTypeToObjectMap[type];
			if (!mActivePanels.Contains(panel.gameObject))
				return PushNewPanel(type);

			PopPanel(type);
			return panel;
		}

		/// <inheritdoc />
		public IUIManager RegisterPanel(IScreenPanel panelObject, ScreenPanelTypes type)
		{
			if (mPanelTypeToObjectMap.ContainsKey(type) || mPanelObjectToTypeMap.ContainsKey(panelObject))
				throw new ArgumentException("Registering a panel for more than one type, or more than one panel for a type!");

			mPanelObjectToTypeMap[panelObject] = type;
			mPanelTypeToObjectMap[type] = panelObject;

			panelObject.gameObject.SetActive(false);

			return this;
		}

		/// <inheritdoc />
		public IUIManager UnregisterPanel(IScreenPanel panelObject)
		{
			if (!mPanelObjectToTypeMap.ContainsKey(panelObject))
				return this;

			ScreenPanelTypes type = mPanelObjectToTypeMap[panelObject];

			mPanelObjectToTypeMap.Remove(panelObject);
			mPanelTypeToObjectMap.Remove(type);

			GameObject go = panelObject.gameObject;
			mActivePanels.Remove(go);

			return this;
		}

		#endregion
	}
}
