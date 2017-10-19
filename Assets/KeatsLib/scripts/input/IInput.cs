using System;

namespace FiringSquad.Core.Input
{
	/// <summary>
	/// Interface for the Input Manager service.
	/// </summary>
	public interface IInput
	{
		/// <summary>
		/// Register a new input function to be fired when its conditions are met.
		/// </summary>
		/// <typeparam name="T">KeyCode or string</typeparam>
		/// <param name="method">The Unity.Input method to check the key with.</param>
		/// <param name="key">The key or button to check.</param>
		/// <param name="command">The action to fire when this input is activated.</param>
		/// <param name="level">The InputLevel that must be active for this input to fire.</param>
		/// <param name="allowOtherKeys">Whether other keys bound to this input should persist.</param>
		IInput RegisterInput<T>(Func<T, bool> method, T key, Action command, InputLevel level, bool allowOtherKeys = true);

		/// <summary>
		/// Remove a command from the registry. All inputs bound to it will be lost.
		/// </summary>
		IInput UnregisterInput(Action command);

		/// <summary>
		/// Register a new axis function to be fired each frame.
		/// </summary>
		/// <param name="method">The Unity.Input method to check with. Generally Input.GetAxis()</param>
		/// <param name="axis">The name of the axis to check.</param>
		/// <param name="command">The function to call with the axis value every frame.</param>
		/// <param name="level">The InputLevel that must be active for this axis to be checked.</param>
		/// <param name="allowOtherAxes">Whether other axis names bound to this command should persist.</param>
		IInput RegisterAxis(Func<string, float> method, string axis, Action<float> command, InputLevel level, bool allowOtherAxes = true);

		/// <summary>
		/// Remove an axis from the registry.
		/// </summary>
		IInput UnregisterAxis(Action<float> command);

		/// <summary>
		/// Directly set the current input level.
		/// </summary>
		void SetInputLevel(InputLevel level);

		/// <summary>
		/// Modify a particular input level to the given state.
		/// </summary>
		void SetInputLevelState(InputLevel level, bool state);

		/// <summary>
		/// Set a particular input level to enabled.
		/// </summary>
		void EnableInputLevel(InputLevel level);

		/// <summary>
		/// Set a particular input level to disabled.
		/// </summary>
		void DisableInputLevel(InputLevel level);

		/// <summary>
		/// Check if a particular input level or combination of levels are enabled.
		/// </summary>
		bool IsInputEnabled(InputLevel level);
	}
}
