using UnityEngine;

namespace FiringSquad.Core.UI
{
	/// <summary>
	/// An interface without any functions to assure type-safety in the UIManager.
	/// </summary>
	public interface IScreenPanel
	{
		GameObject gameObject { get; }
	}
}
