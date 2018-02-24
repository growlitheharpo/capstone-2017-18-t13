using System.Linq;
using FiringSquad.Gameplay.UI;
using UnityEngine;
using UnityEngine.UI;

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
			UIColorUpdater t = target as UIColorUpdater;
			if (t == null)
				return;

			CustomEditorGUIUtility.HorizontalLayout(() =>
			{
				if (GUILayout.Button("Find All Child Images"))
				{
					Undo.RecordObject(target, "Find All Image Children");
					t.EditorSetGraphicsArray(t.GetComponentsInChildren<Image>().Select(x => x as Graphic).ToArray());
				}
				if (GUILayout.Button("Find All Text Children"))
				{
					Undo.RecordObject(target, "Find All Children");
					t.EditorSetGraphicsArray(t.GetComponentsInChildren<Text>().Select(x => x as Graphic).ToArray());
				}
				if (GUILayout.Button("Find All Shadow Children"))
				{
					Undo.RecordObject(target, "Find All Children");
					t.EditorSetShadowsArray(t.GetComponentsInChildren<Shadow>());
				}
			});

			if (GUILayout.Button("Find ALL Possible Children"))
			{
				Undo.RecordObject(target, "Find All Children");
				t.EditorSetGraphicsArray(t.GetComponentsInChildren<Graphic>());
				t.EditorSetShadowsArray(t.GetComponentsInChildren<Shadow>());
			}
		}
	}
}
