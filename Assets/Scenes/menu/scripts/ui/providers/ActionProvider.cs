using System;
using UnityEngine;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// A UI Helper class that a UI manager can use to register for button clicks.
	/// Button prefabs just have this attached and can be reused without code changes.
	/// </summary>
	public class ActionProvider : MonoBehaviour
	{
		/// <summary>
		/// Event fired when the button is clicked.
		/// </summary>
		public event Action OnClick = () => { };

		/// <summary>
		/// Public function exposed to the inspector to tie the button to.
		/// </summary>
		public void Click()
		{
			OnClick();
		}
	}
}
