
using UnityEngine;

namespace UnityEditor
{
	/// <summary>
	/// Custom inspector for the weapon scope script.
	/// </summary>
	[CustomEditor(typeof(FiringSquad.Gameplay.Weapons.WeaponPartScriptScope))]
	public class WeaponPartScriptScopeEditor : Editor
	{
		private Editor mCachedScriptableObjectEditor;
		private Object mCachedObject;

		/// <inheritdoc />
		public override void OnInspectorGUI()
		{
			SerializedObject so = serializedObject;
			so.Update();

			DrawDefaultInspector();
			DrawAimDownSightsEffect();

			so.ApplyModifiedProperties();
		}

		private void DrawAimDownSightsEffect()
		{
			SerializedProperty prop = serializedObject.FindProperty("mAimDownSightsEffect");
			EditorGUILayout.PropertyField(prop);

			if (prop.objectReferenceValue != null)
			{
				EditorGUI.indentLevel++;

				if (mCachedScriptableObjectEditor == null || prop.objectReferenceValue != mCachedObject)
				{
					CreateCachedEditor(prop.objectReferenceValue, null, ref mCachedScriptableObjectEditor);
					mCachedObject = prop.objectReferenceValue;
				}

				mCachedScriptableObjectEditor.OnInspectorGUI();
				EditorGUI.indentLevel--;
			}
		}
	}
}
