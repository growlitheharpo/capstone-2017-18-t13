using FiringSquad.Data;
using UnityEngine;
using Logger = FiringSquad.Debug.Logger;
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
		[SerializeField] private UIGraphic[] mEnemyGraphics;
		[SerializeField] private UIShadow[] mEnemyShadows;

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		private void Awake()
		{
			EventManager.LocalGUI.OnLocalPlayerAssignedTeam += OnLocalPlayerAssignedTeam;
		}

		/// <summary>
		/// Unity's OnDestroy function
		/// </summary>
		private void OnDestroy()
		{
			EventManager.LocalGUI.OnLocalPlayerAssignedTeam -= OnLocalPlayerAssignedTeam;
		}

		/// <summary>
		/// Event Handler: EventManager.LocalGUI.OnLocalPlayerAssignedTeam
		/// </summary>
		private void OnLocalPlayerAssignedTeam(CltPlayer localPlayer)
		{
			Color primary = localPlayer.playerTeam == GameData.PlayerTeam.Orange ? mPrimaryOrangeColor : mPrimaryBlueColor;
			Color shadow = localPlayer.playerTeam == GameData.PlayerTeam.Orange ? mShadowOrangeColor : mShadowBlueColor;

			foreach (UIGraphic g in mGraphics)
			{
				if (g == null)
				{
					Logger.Warn(string.Format("Warning: Null object in UI Color Updater \"{0}\"", name));
					continue;
				}

				g.color = new Color(primary.r, primary.g, primary.b, g.color.a);
			}

			foreach (UIShadow s in mShadows)
			{
				if (s == null)
				{
					Logger.Warn(string.Format("Warning: Null object in UI Color Updater \"{0}\"", name));
					continue;
				}

				s.effectColor = new Color(shadow.r, shadow.g, shadow.b, s.effectColor.a);
			}

			if (localPlayer.playerTeam == GameData.PlayerTeam.Deathmatch)
				return;

			Color enemyPrimary = localPlayer.playerTeam == GameData.PlayerTeam.Orange ? mPrimaryBlueColor : mPrimaryOrangeColor;
			Color enemyShadow = localPlayer.playerTeam == GameData.PlayerTeam.Orange ? mShadowBlueColor : mShadowOrangeColor;

			foreach (UIGraphic g in mEnemyGraphics)
				g.color = new Color(enemyPrimary.r, enemyPrimary.g, enemyPrimary.b, g.color.a);

			foreach (UIShadow s in mEnemyShadows)
				s.effectColor = new Color(enemyShadow.r, enemyShadow.g, enemyShadow.b, s.effectColor.a);
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
