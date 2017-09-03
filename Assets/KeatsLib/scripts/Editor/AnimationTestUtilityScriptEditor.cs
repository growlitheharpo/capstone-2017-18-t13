using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor script tied to the Animation Test Utility.
/// Only purpose is for testing animations and animator controllers in the editor.
/// </summary>
[CustomEditor(typeof(AnimationTestUtilityScript))]
public class AnimationTestUtilityScriptEditor : Editor
{
	private bool mShowWarning;

	public AnimationTestUtilityScriptEditor()
	{
		EditorApplication.playmodeStateChanged += PlaymodeChanged;
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		DrawScriptField();
		DrawPlayAnimationButton();

		serializedObject.ApplyModifiedProperties();
	}

	private void PlaymodeChanged()
	{
		mShowWarning = false;
	}

	private void DrawScriptField()
	{
		GUI.enabled = false;
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"), true);
		GUI.enabled = true;
	}

	private void DrawPlayAnimationButton()
	{
		if (mShowWarning)
			EditorGUILayout.HelpBox("This script will only work while in \"Play\" mode!", MessageType.Error);

		CustomEditorGUIUtility.HorizontalLayout(() =>
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("mCurrentAnimationTrigger"), new GUIContent("Animation Trigger"));

			if (!GUILayout.Button("Play Animation"))
				return;

			if (!EditorApplication.isPlaying)
				mShowWarning = true;
			else
			{
				string anim = serializedObject.FindProperty("mCurrentAnimationTrigger").stringValue;
				GameObject obj = ((AnimationTestUtilityScript)target).gameObject;

				AnimationUtility.PlayAnimation(obj, anim);
			}
		});
	}
}
