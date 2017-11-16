using JetBrains.Annotations;

namespace FiringSquad.Core.UI
{
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
	}
}
