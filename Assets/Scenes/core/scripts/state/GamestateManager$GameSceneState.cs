using System;
using KeatsLib.State;
using UnityEngine;
using Input = KeatsLib.Unity.Input;

public partial class GamestateManager
{
	/// <summary>
	/// State used when the game is in the game scene state.
	/// </summary>
	private class GameSceneState : BaseStateMachine, IGameState
	{
		public bool safeToTransition { get { return true; } }

		public bool IsFeatureEnabled(Feature feat)
		{
			Type currentType = currentState.GetType();

			switch (feat)
			{
				case Feature.WeaponDrops:
					if (currentType == typeof(ArenaGamemodeState))
						return false;
					if (currentType == typeof(MyGunGamemodeState))
						return ((MyGunGamemodeState)currentState).settings.enableAIDrops;
					if (currentType == typeof(QuickdrawGamemodeState))
						return ((QuickdrawGamemodeState)currentState).settings.enableAIDrops;
					break;
				case Feature.WeaponDurability:
					if (currentType == typeof(ArenaGamemodeState))
						return ((ArenaGamemodeState)currentState).settings.enableDurability;
					if (currentType == typeof(MyGunGamemodeState))
						return ((MyGunGamemodeState)currentState).settings.enableDurability;
					if (currentType == typeof(QuickdrawGamemodeState))
						return ((QuickdrawGamemodeState)currentState).settings.enableDurability;
					break;
			}

			return false;
		}

		/// <inheritdoc />
			public void OnEnter()
		{
			EventManager.OnInputLevelChanged += HandleInputChange;
			bool state = ServiceLocator.Get<IInput>().IsInputEnabled(Input.InputLevel.Gameplay);

			TransitionStates(new FindGameModeState(this));

			SetCursorState(state);
		}

		private void HandleInputChange(Input.InputLevel input, bool state)
		{
			if (input != Input.InputLevel.Gameplay)
				return;

			SetCursorState(state);
		}

		private void SetCursorState(bool hidden)
		{
			Cursor.lockState = hidden ? CursorLockMode.Locked : CursorLockMode.None;
			Cursor.visible = !hidden;
		}

		public new void Update()
		{
			base.Update();
		}

		public void OnExit()
		{
			TransitionStates(new NullState(this));

			EventManager.OnInputLevelChanged -= HandleInputChange;
			SetCursorState(false);
		}

		public IState GetTransition()
		{
			return this; //we never explicitly leave
		}

		private class NullState : BaseState<GameSceneState>
		{
			public NullState(GameSceneState m) : base(m) { }

			public override IState GetTransition()
			{
				return this;
			}
		}

		private class FindGameModeState : BaseState<GameSceneState>
		{
			public FindGameModeState(GameSceneState m) : base(m) { }

			public override IState GetTransition()
			{
				Gamemode g = FindObjectOfType<Gamemode>();
				if (g == null)
					return this;

				switch (g.mode)
				{
					case Gamemode.Mode.MyGun:
						return new MyGunGamemodeState(mMachine);
					case Gamemode.Mode.QuickDraw:
						return new QuickdrawGamemodeState(mMachine);
					case Gamemode.Mode.Arena:
						return new ArenaGamemodeState(mMachine);
					default:
						return this;
				}
			}
		}

		private class MyGunGamemodeState : BaseState<GameSceneState>
		{
			private Gamemode.MyGunSettings mSettings;
			public Gamemode.MyGunSettings settings { get { return mSettings; } }

			public MyGunGamemodeState(GameSceneState m) : base(m)
			{
				mSettings = FindObjectOfType<Gamemode>().mygunSettings;
			}

			public override IState GetTransition()
			{
				return this;
			}
		}

		private class QuickdrawGamemodeState : BaseState<GameSceneState>
		{
			private Gamemode.QuickdrawSettings mSettings;
			public Gamemode.QuickdrawSettings settings { get { return mSettings; } }

			public QuickdrawGamemodeState(GameSceneState m) : base(m)
			{
				mSettings = FindObjectOfType<Gamemode>().quickdrawSettings;
			}

			public override IState GetTransition()
			{
				return this;
			}
		}

		private class ArenaGamemodeState : BaseState<GameSceneState>
		{
			private Gamemode.ArenaSettings mSettings;
			public Gamemode.ArenaSettings settings { get { return mSettings; } }

			public ArenaGamemodeState(GameSceneState m) : base(m)
			{
				mSettings = FindObjectOfType<Gamemode>().arenaSettings;
			}

			public override IState GetTransition()
			{
				return this;
			}
		}
	}
}
