using JetBrains.Annotations;
using UnityEngine;

namespace FiringSquad.Core.UI
{
	/// <summary>
	/// The different types of screen panels that will pause input.
	/// </summary>
	public enum ScreenPanelTypes
	{
		Pause,
		PlayerNameEntry,
		GameOver,
		Console,
		DebugMenu,
		HandleConnection,
		TeamVictory,
		Scorecard,
	}

	/// <summary>
	/// The public interface for the gameplay UI manager.
	/// Utilized for binding properties to UI elements.
	/// </summary>
	public interface IUIManager : IGlobalService
	{
		/// <summary>
		/// Get the property associated with a particular hash.
		/// </summary>
		/// <typeparam name="T">The type of property.</typeparam>
		/// <param name="hash">The hash to bind to.</param>
		/// <returns>The bound property in the system, or null if one could not be found.</returns>
		[CanBeNull] BoundProperty<T> GetProperty<T>(int hash);

		/// <summary>
		/// Bind a BoundProperty instance to a particular hash.
		/// </summary>
		/// <param name="hash">The hash to search for.</param>
		/// <param name="prop">The property instance to bind.</param>
		void BindProperty(int hash, BoundProperty prop);

		/// <summary>
		/// Instantly remove a property from the system.
		/// </summary>
		/// <param name="prop">The property that is being removed.</param>
		void UnbindProperty(BoundProperty prop);

		/// <summary>
		/// Push a new panel type onto the screen UI stack.
		/// </summary>
		/// <param name="type">Which panel to activate.</param>
		IScreenPanel PushNewPanel(ScreenPanelTypes type);

		/// <summary>
		/// Remove a panel type form the screen UI stack.
		/// </summary>
		/// <param name="type">Which panel to deactivate.</param>
		IUIManager PopPanel(ScreenPanelTypes type);

		/// <summary>
		/// Toggle whether a panel is active or not.
		/// If it is active, pops it. If it is inactive, pushes it to the top.
		/// </summary>
		/// <param name="type">Which panel to toggle.</param>
		IScreenPanel TogglePanel(ScreenPanelTypes type);

		/// <summary>
		/// Register a panel to be used by the UI system.
		/// Will immediately disable the panel.
		/// </summary>
		/// <param name="panelObject">The GameObject for this panel type.</param>
		/// <param name="type">Which type of panel this is.</param>
		/// <param name="disablePanel">Whether or not to disable the panel immediately.</param>
		IUIManager RegisterPanel(IScreenPanel panelObject, ScreenPanelTypes type, bool disablePanel = true);

		/// <summary>
		/// Register a panel to be used by the UI system.
		/// </summary>
		/// <param name="panelObject">The GameObject to be removed.</param>
		IUIManager UnregisterPanel(IScreenPanel panelObject);
	}
}
