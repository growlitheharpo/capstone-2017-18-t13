using System.IO;
using KeatsLib.Unity;
using UnityEngine;

namespace UnityEditor
{
	/// <summary>
	/// Custom editor for the AudioProfile class.
	/// Binds the ID to the file name.
	/// </summary>
	[CustomEditor(typeof(AudioProfile))]
	public class AudioProfileEditor : Editor
	{
		private AudioProfile mTarget;

		public override void OnInspectorGUI()
		{
			mTarget = target as AudioProfile;
			serializedObject.Update();

			ManuallyUpdateId();
			
			CustomEditorGUIUtility.DrawScriptField(serializedObject);

			DrawProfile(serializedObject);

			serializedObject.ApplyModifiedProperties();
			mTarget.SynchronizeCollections();
		}

		private void ManuallyUpdateId()
		{
			string path = AssetDatabase.GetAssetPath(mTarget);
			string file = Path.GetFileNameWithoutExtension(path);
			mTarget.id = file;
		}

		private static void DrawProfile(SerializedObject target)
		{
			CustomEditorGUIUtility.DrawDisabled(() =>
				EditorGUILayout.PropertyField(target.FindProperty("mId"), new GUIContent("ID", "Edit file name to change the ID."))
			);
			EditorGUILayout.PropertyField(target.FindProperty("mProfile"), new GUIContent("Profile Type"));
			CustomEditorGUIUtility.DrawList(target.FindProperty("mClipsArray"), "Clips Array", addLabel: "Add New Event Clip List");
			EditorGUILayout.PropertyField(target.FindProperty("mParent"), new GUIContent("Parent Profile"));
		}
	}

	/// <summary>
	/// Custom property drawer for the event-to-clip list
	/// Cleaner and more Dictionary-like.
	/// </summary>
	[CustomPropertyDrawer(typeof(AudioProfile.EventToClipList))]
	public class AudioProfileEventMatcher : PropertyDrawer
	{
		private float mOneLineHeight;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			mOneLineHeight = base.GetPropertyHeight(property, label);

			int count = Mathf.Clamp(property.FindPropertyRelative("mClips").arraySize + 1, 2, int.MaxValue);
			return count * mOneLineHeight;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			CustomEditorGUIUtility.SetLineHeight(mOneLineHeight);
			SerializedProperty eventProp = property.FindPropertyRelative("mEvent");
			SerializedProperty arrayProp = property.FindPropertyRelative("mClips");

			Rect eventRect = new Rect(position.x, position.y, 150.0f, mOneLineHeight);
			Rect arrayRect = eventRect.ShiftAlongX(position.width - 150.0f, position.height);

			EditorGUI.PropertyField(eventRect, eventProp, GUIContent.none);
			CustomEditorGUIUtility.DrawList(arrayRect, arrayProp, "");
		}
	}
}
