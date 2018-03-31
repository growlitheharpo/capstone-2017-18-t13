using System.Collections.Generic;
using System.Linq;
using KeatsLib.Collections;
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
			LowHealth,
			LowClip,
		}

		/// <summary>
		/// The map of enums to actual hitn text.
		/// </summary>
		private readonly Dictionary<Hint, string> mHintTextMap = new Dictionary<Hint, string>
		{
			{ Hint.MagnetArmGrab, "Hold E to pull" },
			{ Hint.ItemEquipOrDrop, "Press E to equip or Q to drop" },
			{ Hint.LowHealth, "LOW HEALTH" },
			{ Hint.LowClip, "RELOAD" },
		};

		[SerializeField] private Color mRegularColor;
		[SerializeField] private Color mDangerColor;

		private UnityEngine.UI.Shadow mShadow;
		private UnityEngine.UI.Text mUIText;
		private List<Hint> mActiveHints;

		/// <summary>
		/// Unity's Awake function.
		/// </summary>
		private void Awake()
		{
			mUIText = GetComponent<UnityEngine.UI.Text>();
			EventManager.LocalGUI.OnSetHintState += OnSetHintState;
			mActiveHints = new List<Hint>();

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
			if (state)
			{
				int index = mActiveHints.IndexOf(hint);
				if (index >= 0)
					mActiveHints.SwapElement(index, mActiveHints.Count - 1);
				else
					mActiveHints.Add(hint);

				PrioritizeList();
			}
			else
				mActiveHints.RemoveAll(x => x == hint);

			UpdateText();
		}

		/// <summary>
		/// Reorder the list based on priority
		/// </summary>
		private void PrioritizeList()
		{
			mActiveHints = mActiveHints
				.OrderBy(x =>
				{
					switch (x)
					{
						case Hint.MagnetArmGrab:
							return 5;
						case Hint.ItemEquipOrDrop:
							return 5;
						case Hint.LowHealth:
							return 10;
						case Hint.LowClip:
							return 10;
						default:
							return 0;
					}
				})
				.ToList();
		}

		/// <summary>
		/// Update the text to be the most recent hint text pushed.
		/// </summary>
		private void UpdateText()
		{
			if (mActiveHints.Count > 0)
			{
				Hint hint = mActiveHints.Last();
				if (hint == Hint.LowClip || hint == Hint.LowHealth)
				{
					mUIText.color = mDangerColor;
					mShadow.enabled = false;
				}
				else
				{
					mUIText.color = mRegularColor;
					mShadow.enabled = true;
				}

				mUIText.text = mHintTextMap[hint];
			}
			else
				mUIText.text = "";
		}
	}
}
