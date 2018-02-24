using System.Collections;
using System.Collections.Generic;
using FiringSquad.Data;
using UnityEngine;
using UIGraphic = UnityEngine.UI.Graphic;
using UIShadow = UnityEngine.UI.Shadow;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// Utility script to update an arbitary collection of UI elements when
	/// the player's team color changes.
	/// </summary>
	public class UIColorUpdater : MonoBehaviour
	{
		/// Inspector variables
		[SerializeField] private Color mPrimaryOrangeColor;
		[SerializeField] private Color mShadowOrangeColor;
		[SerializeField] private Color mPrimaryBlueColor;
		[SerializeField] private Color mShadowBlueColor;

		[SerializeField] private UIGraphic[] mGraphics;
		[SerializeField] private UIShadow[] mShadows;

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		private void Awake()
		{
			EventManager.LocalGUI.OnLocalPlayerAssignedTeam += OnLocalPlayerAssignedTeam;
		}

		/// <summary>
		/// Event Handler: EventManager.LocalGUI.OnLocalPlayerAssignedTeam
		/// </summary>
		private void OnLocalPlayerAssignedTeam(CltPlayer localPlayer)
		{
			Color primary = localPlayer.playerTeam == GameData.PlayerTeam.Orange ? mPrimaryOrangeColor : mPrimaryBlueColor;
			Color shadow = localPlayer.playerTeam == GameData.PlayerTeam.Orange ? mShadowOrangeColor : mShadowBlueColor;

			foreach (UIGraphic g in mGraphics)
				g.color = new Color(primary.r, primary.g, primary.b, g.color.a);

			foreach (UIShadow s in mShadows)
				s.effectColor = new Color(shadow.r, shadow.g, shadow.b, s.effectColor.a);
		}

#if UNITY_EDITOR

		/// <summary>
		/// EDITOR ONLY
		/// Set the list of UI Graphics elements that this updater affects.
		/// </summary>
		public void EditorSetGraphicsArray(UIGraphic[] newArray)
		{
			mGraphics = newArray;
		}
		
		/// <summary>
		/// EDITOR ONLY
		/// Set the list of UI Shadow elements that this updater affects.
		/// </summary>
		public void EditorSetShadowsArray(UIShadow[] newArray)
		{
			mShadows = newArray;
		}

#endif
	}
}
