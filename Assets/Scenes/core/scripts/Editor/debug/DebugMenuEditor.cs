using FiringSquad.Debug;
using UnityEngine;

namespace UnityEditor
{
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