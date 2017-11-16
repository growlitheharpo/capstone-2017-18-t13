using FiringSquad.Debug;
using UnityEngine;

namespace UnityEditor
{
	/// <summary>
	/// Draw a custom button for the debug menu.
	/// </summary>
	[CustomEditor(typeof(DebugMenu))]
	public class DebugMenuEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			if (GUILayout.Button("Refresh List"))
				((DebugMenu)target).RefreshWeaponList();

			DrawDefaultInspector();
		}
	}
}