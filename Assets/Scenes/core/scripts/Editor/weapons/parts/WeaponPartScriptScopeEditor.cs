
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor
{
	/// <summary>
	/// Custom inspector for the weapon scope script.
	/// </summary>
	[CustomEditor(typeof(FiringSquad.Gameplay.Weapons.WeaponPartScriptScope))]
	public class WeaponPartScriptScopeEditor : Editor
	{
		//private Editor mCachedScriptableObjectEditor;
		//private Object mCachedObject;
		private class EditorObjectBind
		{
			public Editor editor;
			public Object obj;
		}

		private Dictionary<SerializedProperty, EditorObjectBind> mPropertyEditors;

		private void OnEnable()
		{
			mPropertyEditors = new Dictionary<SerializedProperty, EditorObjectBind>();
		}

		/// <inheritdoc />
		public override void OnInspectorGUI()
		{
			SerializedObject so = serializedObject;
			so.Update();

			DrawDefaultInspector();
			DrawAimDownSightsEffect();

			so.ApplyModifiedProperties();
		}

		/// <summary>
		/// Draw the ScriptableObjects of the aim down sights effects inside the scope editor.
		/// </summary>
		private void DrawAimDownSightsEffect()
		{
			SerializedProperty prop = serializedObject.FindProperty("mAimDownSightsEffects");
			prop.isExpanded = EditorGUILayout.Foldout(prop.isExpanded, prop.displayName, true);

			if (!prop.isExpanded)
				return;

			EditorGUI.indentLevel++;

			int newSize = EditorGUILayout.DelayedIntField(new GUIContent("Count"), prop.arraySize);
			if (newSize != prop.arraySize)
			{
				Undo.RecordObject(target, "Change Count");
				prop.arraySize = newSize;
			}

			for (int i = 0; i < prop.arraySize; ++i)
			{
				SerializedProperty item = prop.GetArrayElementAtIndex(i);
				EditorGUILayout.PropertyField(item, true);

				if (item.objectReferenceValue == null)
					continue;

				EditorGUI.indentLevel++;
				EditorGUI.indentLevel++;

				// Creating an editor is expensive, so we only do it when necessary
				bool needRefresh = !mPropertyEditors.ContainsKey(item)
									|| mPropertyEditors[item].obj == null
									|| mPropertyEditors[item].obj != item.objectReferenceValue
									|| mPropertyEditors[item].editor == null;
				if (needRefresh)
				{
					mPropertyEditors[item] = new EditorObjectBind();
					CreateCachedEditor(item.objectReferenceValue, null, ref mPropertyEditors[item].editor);
					mPropertyEditors[item].obj = item.objectReferenceValue;
				}

				mPropertyEditors[item].editor.OnInspectorGUI();
				EditorGUI.indentLevel--;
				EditorGUI.indentLevel--;
			}

			--EditorGUI.indentLevel;
		}
	}
}
