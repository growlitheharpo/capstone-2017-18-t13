using System.Linq;
using FiringSquad.Core;
using FiringSquad.Core.Input;
using FiringSquad.Core.Weapons;
using FiringSquad.Gameplay;
using FiringSquad.Gameplay.Weapons;
using UnityEngine;
using Input = UnityEngine.Input;

namespace FiringSquad.Debug
{
	/// <summary>
	/// The debug menu that displays all weapon parts.
	/// </summary>
	public class DebugMenu : MonoBehaviour
	{
		/// Inspector variables
		[SerializeField] private WeaponPartScript[] mMechanisms;
		[SerializeField] private WeaponPartScript[] mBarrels;
		[SerializeField] private WeaponPartScript[] mScopes;
		[SerializeField] private WeaponPartScript[] mGrips;

		/// Private variables
		private bool mActive;

		/// <summary>
		/// Update our WeaponList from the service.
		/// </summary>
		public void RefreshWeaponList()
		{
			var parts = ServiceLocator.Get<IWeaponPartManager>().GetAllPrefabScripts(true).Values;

			mMechanisms = parts.Where(x => x.attachPoint == BaseWeaponScript.Attachment.Mechanism).ToArray();
			mBarrels = parts.Where(x => x.attachPoint == BaseWeaponScript.Attachment.Barrel).ToArray();
			mScopes = parts.Where(x => x.attachPoint == BaseWeaponScript.Attachment.Scope).ToArray();
			mGrips = parts.Where(x => x.attachPoint == BaseWeaponScript.Attachment.Grip).ToArray();
		}

		/// <summary>
		/// Unity's Start function.
		/// </summary>
		private void Start()
		{
			RefreshWeaponList();
			ServiceLocator.Get<IInput>()
				.RegisterInput(Input.GetKeyDown, KeyCode.Tab, ToggleUI, InputLevel.None);
		}

		/// <summary>
		/// Cleanup all listeners and event handlers.
		/// </summary>
		private void OnDestroy()
		{
			ServiceLocator.Get<IInput>()
				.UnregisterInput(ToggleUI);
		}

		/// <summary>
		/// Toggle whether or not the menu is visible.
		/// </summary>
		private void ToggleUI()
		{
			mActive = !mActive;
			ServiceLocator.Get<IInput>()
				.SetInputLevelState(InputLevel.Gameplay, !mActive)
				.SetInputLevelState(InputLevel.HideCursor, !mActive);
		}

		/// <summary>
		/// Unity's ONGUI function. Draw the parts.
		/// </summary>
		private void OnGUI()
		{
			if (!mActive)
				return;

			float columnWidth = Screen.width / 4.0f;

			Rect mechRect = new Rect(0.0f, 0.0f, columnWidth, Screen.height);
			Rect barrelRect = new Rect(mechRect.x + mechRect.width, 0.0f, columnWidth, Screen.height);
			Rect scopeRect = new Rect(barrelRect.x + barrelRect.width, 0.0f, columnWidth, Screen.height);
			Rect gripRect = new Rect(scopeRect.x + scopeRect.width, 0.0f, columnWidth, Screen.height);

			DrawPartList(mechRect, mMechanisms);
			DrawPartList(barrelRect, mBarrels);
			DrawPartList(scopeRect, mScopes);
			DrawPartList(gripRect, mGrips);
		}

		/// <summary>
		/// Draw a list of parts.
		/// </summary>
		/// <param name="area">The overall rect of this list.</param>
		/// <param name="parts">The collection of weapon parts in this list.</param>
		private static void DrawPartList(Rect area, WeaponPartScript[] parts)
		{
			GUILayout.BeginArea(area);
			foreach (WeaponPartScript part in parts)
			{
				string label = part.prettyName;
				if (part.description != "")
					label += "\n\n" + part.description;

				if (GUILayout.Button(label, GUILayout.MaxHeight(100.0f)))
				{
					CltPlayer player = FindObjectsOfType<CltPlayer>()
						.FirstOrDefault(x => x.isCurrentPlayer);
					if (player != null)
						player.CmdDebugEquipWeaponPart(part.name);
				}
			}
			GUILayout.EndArea();
		}
	}
}
