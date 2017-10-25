using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FiringSquad.Gameplay.UI
{
	public class CrosshairHintText : MonoBehaviour
	{
		private readonly Dictionary<Hint, string> mHintTextMap = new Dictionary<Hint, string>
		{
			{ Hint.MagnetArmGrab, "Hold F to pull" },
			{ Hint.ItemEquipOrDrop, "Press E to equip or F to drop" },
		};

		public enum Hint
		{
			MagnetArmGrab,
			ItemEquipOrDrop,
		}

		private UnityEngine.UI.Text mUIText;
		private Stack<Hint> mActiveHints;

		private void Awake()
		{
			mUIText = GetComponent<UnityEngine.UI.Text>();
			EventManager.LocalGUI.OnSetHintState += OnSetHintState;
			mActiveHints = new Stack<Hint>();

			UpdateText();
		}

		private void OnDestroy()
		{
			EventManager.LocalGUI.OnSetHintState -= OnSetHintState;
		}

		private void OnSetHintState(Hint hint, bool state)
		{
			if (state)
				mActiveHints.Push(hint);
			if (!state)
				mActiveHints = new Stack<Hint>(mActiveHints.Where(x => x != hint).ToArray());

			UpdateText();
		}

		private void UpdateText()
		{
			mUIText.text = mActiveHints.Count <= 0 ? "" : mHintTextMap[mActiveHints.Peek()];
		}
	}
}
