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
	public class DebugMenu : MonoBehaviour
	{
		[SerializeField] private WeaponPartScript[] mMechanisms;
		[SerializeField] private WeaponPartScript[] mBarrels;
		[SerializeField] private WeaponPartScript[] mScopes;
		[SerializeField] private WeaponPartScript[] mGrips;

		private bool mActive;
		public bool currentlyActive { get { return mActive; } }

		public void RefreshWeaponList()
		{
			var parts = ServiceLocator.Get<IWeaponPartManager>().GetAllPrefabScripts(true).Values;

			mMechanisms = parts.Where(x => x.attachPoint == BaseWeaponScript.Attachment.Mechanism).ToArray();
			mBarrels = parts.Where(x => x.attachPoint == BaseWeaponScript.Attachment.Barrel).ToArray();
			mScopes = parts.Where(x => x.attachPoint == BaseWeaponScript.Attachment.Scope).ToArray();
			mGrips = parts.Where(x => x.attachPoint == BaseWeaponScript.Attachment.Grip).ToArray();
		}

		private void Start()
		{
			RefreshWeaponList();
			ServiceLocator.Get<IInput>()
				.RegisterInput(Input.GetKeyDown, KeyCode.Tab, ToggleUI, InputLevel.None);
		}

		private void OnDestroy()
		{
			ServiceLocator.Get<IInput>()
				.UnregisterInput(ToggleUI);
		}

		private void ToggleUI()
		{
			mActive = !mActive;
			ServiceLocator.Get<IInput>()
				.SetInputLevelState(InputLevel.Gameplay, !mActive)
				.SetInputLevelState(InputLevel.HideCursor, !mActive);
		}

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
