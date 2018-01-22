using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// UI class that manages hint text beneath the crosshair.
	/// Uses EventManager.LocalGUI.SetHintState to handle the hint stack.
	/// </summary>
	public class CrosshairHintText : MonoBehaviour
	{
		/// <summary>
		/// The different hints available to push.
		/// </summary>
		public enum Hint
		{
			MagnetArmGrab,
			ItemEquipOrDrop,
		}

		/// <summary>
		/// The map of enums to actual hitn text.
		/// </summary>
		private readonly Dictionary<Hint, string> mHintTextMap = new Dictionary<Hint, string>
		{
			{ Hint.MagnetArmGrab, "Hold F to pull" },
			{ Hint.ItemEquipOrDrop, "Press E to equip or F to drop" },
		};

		private UnityEngine.UI.Text mUIText;
		private Stack<Hint> mActiveHints;

		/// <summary>
		/// Unity's Awake function.
		/// </summary>
		private void Awake()
		{
			mUIText = GetComponent<UnityEngine.UI.Text>();
			EventManager.LocalGUI.OnSetHintState += OnSetHintState;
			mActiveHints = new Stack<Hint>();

			UpdateText();
		}

		/// <summary>
		/// Cleanup listeners and event handlers.
		/// </summary>
		private void OnDestroy()
		{
			EventManager.LocalGUI.OnSetHintState -= OnSetHintState;
		}

		/// <summary>
		/// EVENT HANDLER: LocalGUI.OnSetHintState
		/// </summary>
		private void OnSetHintState(Hint hint, bool state)
		{
			if (state && !mActiveHints.Contains(hint))
				mActiveHints.Push(hint);
			if (!state)
				mActiveHints = new Stack<Hint>(mActiveHints.Where(x => x != hint).ToArray());

			UpdateText();
		}

		/// <summary>
		/// Update the text to be the most recent hint text pushed.
		/// </summary>
		private void UpdateText()
		{
			mUIText.text = mActiveHints.Count <= 0 ? "" : mHintTextMap[mActiveHints.Peek()];
		}
	}
}
