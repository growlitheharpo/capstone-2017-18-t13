using System;
using System.Collections.Generic;
using System.Linq;

namespace KeatsLib.Unity
{
	/// <summary>
	/// Global input manager using the command pattern.
	/// </summary>
	public class Input : MonoSingleton<Input>, IInput
	{
		/// <summary>
		/// Base input map to see if some form of Unity input collection has been activated.
		/// </summary>
		private abstract class BaseInputMap
		{
			private readonly InputLevel mInputLevel;

			protected BaseInputMap(InputLevel l)
			{
				mInputLevel = l;
			}

			/// <summary>
			/// Returns true if the current enabled inputs match our input level
			/// </summary>
			public bool Enabled()
			{
				return instance.IsInputEnabled(mInputLevel);
			}

			/// <summary>
			/// Returns true if this input is activated based on its internal rules.
			/// </summary>
			public abstract bool Activated();
		}

		/// <summary>
		/// Useable input map to bind Unity input collection.
		/// </summary>
		/// <typeparam name="T">String or KeyCode</typeparam>
		private class InputMap<T> : BaseInputMap
		{
			private readonly Func<T, bool> mFunction;
			private readonly T mValue;

			public InputMap(Func<T, bool> check, T value, InputLevel level) : base(level)
			{
				mFunction = check;
				mValue = value;
			}

			public override bool Activated()
			{
				return mFunction(mValue);
			}
		}
		
		/// <summary>
		/// Useable input map to bind Unity input collection.
		/// </summary>
		private class AxisMap : BaseInputMap
		{
			private readonly Func<string, float> mFunction;
			private readonly string mAxisName;

			public AxisMap(Func<string, float> check, string value, InputLevel level) : base(level)
			{
				mFunction = check;
				mAxisName = value;
			}

			public override bool Activated()
			{
				return Math.Abs(mFunction(mAxisName)) > 0.01f;
			}

			public float CurrentValue()
			{
				return mFunction(mAxisName);
			}
		}

		[Flags]
		public enum InputLevel
		{
			None = 0,
			Gameplay = 1,
			InGameMenu = 32,
			PauseMenu = 64,
			DevConsole = 128,
			All = int.MaxValue
		}

		private readonly Dictionary<Action, List<BaseInputMap>> mCommands = new Dictionary<Action, List<BaseInputMap>>();
		private readonly Dictionary<Action<float>, List<AxisMap>> mAxes = new Dictionary<Action<float>, List<AxisMap>>();

		private InputLevel mEnabledInputs = InputLevel.None;

		/// <summary>
		/// Register an input to a command.
		/// </summary>
		/// <param name="method">Which Unity input method to use (GetKey, GetKeyDown, GetKeyUp, etc.)</param>
		/// <param name="key">Which KeyCode or button/axis name to check.</param>
		/// <param name="command">The command to fire when this input is activated.</param>
		/// <param name="level">The InputLevel for this input. All flags must be matched for input to fire.</param>
		/// <param name="allowOtherKeys">Allows other keys to map to this same input.</param>
		public IInput RegisterInput<T>(Func<T, bool> method, T key, Action command, InputLevel level, bool allowOtherKeys = true)
		{
			Logger.Info("Registering " + command.Method.Name + " as a new input on key " + key + ".", Logger.System.Input);
			var newInput = new InputMap<T>(method, key, level);
			ClearInputFromOtherCommands(newInput);

			if (!allowOtherKeys)
				ClearOtherInputsForCommand(command, mCommands);

			if (!mCommands.ContainsKey(command))
				mCommands[command] = new List<BaseInputMap>();

			mCommands[command].Add(newInput);
			return this;
		}

		public IInput UnregisterInput(Action command)
		{
			Logger.Info("Unegistering " + command.Method.Name + " from all inputs.", Logger.System.Input);
			mCommands.Remove(command);
			return this;
		}

		public IInput RegisterAxis(Func<string, float> method, string axis, Action<float> command, InputLevel level, bool allowOtherAxes = true)
		{
			Logger.Info("Registering " + command.Method.Name + " as a new input on axis " + axis + ".", Logger.System.Input);
			AxisMap newInput = new AxisMap(method, axis, level);
			ClearInputFromOtherCommands(newInput);

			if (!allowOtherAxes)
				ClearOtherInputsForCommand(command, mAxes);

			if (!mAxes.ContainsKey(command))
				mAxes[command] = new List<AxisMap>();

			mAxes[command].Add(newInput);
			return this;
		}

		public IInput UnregisterAxis(Action<float> command)
		{
			Logger.Info("Unegistering " + command.Method.Name + " from all inputs.", Logger.System.Input);
			mAxes.Remove(command);
			return this;
		}

		/// <summary>
		/// Remove all other inputs for this command.
		/// </summary>
		private static void ClearOtherInputsForCommand<T1, T2>(T1 c, IDictionary<T1, List<T2>> commands)
		{
			if (commands.ContainsKey(c))
				commands[c].Clear();
		}

		/// <summary>
		/// Remove this input from all other commands.
		/// </summary>
		private void ClearInputFromOtherCommands(BaseInputMap i)
		{
			foreach (var pair in mCommands)
				pair.Value.Remove(i);
		}

		/// <summary>
		/// Unity Update loop.
		/// </summary>
		private void Update()
		{
			foreach (var i in mCommands)
			{
				if (i.Value.Any(x => x.Enabled() && x.Activated()))
					i.Key.Invoke();
			}

			foreach (var i in mAxes)
			{
				float value = i.Value
					.Where(input => input.Enabled())
					.Sum(input => input.CurrentValue());
				i.Key.Invoke(value);
			}
		}

		/// <summary>
		/// Directly set the current allowed input level.
		/// </summary>
		public void SetInputLevel(InputLevel level)
		{
			mEnabledInputs = level;
		}

		/// <summary>
		/// Set the enabled/disabled state for a specific input level.
		/// </summary>
		/// <param name="flag">The level whose state should be changed.</param>
		/// <param name="state">The state to set the level to.</param>
		public void SetInputLevelState(InputLevel flag, bool state)
		{
			if (state)
				EnableInputLevel(flag);
			else
				DisableInputLevel(flag);
		}

		/// <summary>
		/// Sets an input level as disabled.
		/// </summary>
		public void DisableInputLevel(InputLevel level)
		{
			mEnabledInputs &= ~level;
		}

		/// <summary>
		/// Sets an input level as enabled.
		/// </summary>
		public void EnableInputLevel(InputLevel level)
		{
			mEnabledInputs |= level;
		}

		/// <summary>
		/// Check if an input level is enabled or disabled.
		/// </summary>
		public bool IsInputEnabled(InputLevel level)
		{
			return (mEnabledInputs & level) == level;
		}
	}
}
