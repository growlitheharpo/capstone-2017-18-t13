using FiringSquad.Gameplay.UI;
using UnityEngine;

namespace UnityEditor
{
	/// <summary>
	/// Custom editor for the color updater.
	/// Adds a "Find All Children" button.
	/// </summary>
	[CustomEditor(typeof(UIColorUpdater))]
	public class UIColorUpdaterEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			DrawDefaultInspector();

			DrawFindObjectsButtons();

			serializedObject.ApplyModifiedProperties();
		}

		private void DrawFindObjectsButtons()
		{
			CustomEditorGUIUtility.HorizontalLayout(() =>
			{
				if (GUILayout.Button(""))
				{

				}
			});
		}
	}
}
