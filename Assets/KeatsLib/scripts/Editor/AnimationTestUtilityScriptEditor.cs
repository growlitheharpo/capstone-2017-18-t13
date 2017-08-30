using UnityEditor;
using UnityEngine;

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

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("mCurrentAnimationTrigger"), new GUIContent("Animation Trigger"));
		if (GUILayout.Button("Play Animation"))
		{
			if (!EditorApplication.isPlaying)
				mShowWarning = true;
			else
			{
				string anim = serializedObject.FindProperty("mCurrentAnimationTrigger").stringValue;
				GameObject obj = ((AnimationTestUtilityScript)target).gameObject;

				AnimationUtility.PlayAnimation(obj, anim);
			}
		}
		EditorGUILayout.EndHorizontal();
	}
}
