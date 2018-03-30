using UnityEngine;

namespace FiringSquad.Core.UI
{
	/// <summary>
	/// An interface without any functions to assure type-safety in the UIManager.
	/// </summary>
	public interface IScreenPanel
	{
		GameObject gameObject { get; }

		/// <summary>
		/// True if this panel type should turn off input when enabled.
		/// </summary>
		bool disablesInput { get; }

		/// <summary>
		/// Automatically called by the UIManager when a panel is pushed onto the stack.
		/// </summary>
		void OnEnablePanel();

		/// <summary>
		/// Automatically called by the UIManager when a panel is popped from the stack.
		/// </summary>
		void OnDisablePanel();
	}
}
