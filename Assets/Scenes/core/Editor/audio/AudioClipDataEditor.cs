using FiringSquad.Data;
using KeatsLib.Unity;
using UnityEngine;

namespace UnityEditor
{
	/// <summary>
	/// Custom editor for the AudioClipData.
	/// Tries to make it look more similar to Unity's default inspector
	/// for an AudioSource
	/// </summary>
	[CustomEditor(typeof(AudioClipData))]
	public class AudioClipDataEditor : Editor
	{
		private readonly GUIContent mSound = new GUIContent("Audio Clip");
		private readonly GUIContent mGroup = new GUIContent("Output Group");

		private readonly GUIContent mFadeInTime = new GUIContent("Default Fade-In Time");
		private readonly GUIContent mFadeOutTime = new GUIContent("Default Fade-Out Time");
		private readonly GUIContent mPlayAtSource = new GUIContent("Play At Source");

		private readonly GUIContent mBypassEffects = new GUIContent("Bypass Effects");
		private readonly GUIContent mLooping = new GUIContent("Loop");

		private readonly GUIContent mPriority = new GUIContent("Priority");
		private readonly GUIContent mVolume = new GUIContent("Volume");
		private readonly GUIContent mPitch = new GUIContent("Pitch");
		private readonly GUIContent mStereoPan = new GUIContent("Stereo Pan");
		private readonly GUIContent mSpatialBlend = new GUIContent("Spatial Blend");

		private readonly GUIContent mRolloffMode = new GUIContent("Volume Rolloff");
		private readonly GUIContent mMinDistance = new GUIContent("Min Distance");
		private readonly GUIContent mMaxDistance = new GUIContent("Max Distance");

		private bool mFoldout;

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			SerializedObject so = serializedObject;

			CustomEditorGUIUtility.DrawScriptField(so);

			EditorGUILayout.PropertyField(so.FindProperty("mSound"), mSound);
			EditorGUILayout.PropertyField(so.FindProperty("mGroup"), mGroup);

			CustomEditorGUIUtility.VerticalSpacer(25.0f);

			EditorGUILayout.PropertyField(so.FindProperty("mFadeInTime"), mFadeInTime);
			EditorGUILayout.PropertyField(so.FindProperty("mFadeOutTime"), mFadeOutTime);
			EditorGUILayout.PropertyField(so.FindProperty("mPlayAtSource"), mPlayAtSource);

			CustomEditorGUIUtility.VerticalSpacer(10.0f);

			EditorGUILayout.PropertyField(so.FindProperty("mBypassEffects"), mBypassEffects);
			EditorGUILayout.PropertyField(so.FindProperty("mLooping"), mLooping);

			CustomEditorGUIUtility.VerticalSpacer(10.0f);

			DrawCustomSliderInt(so.FindProperty("mPriority"), 1, 256, "Low", "High", mPriority);
			DrawCustomSliderFloat(so.FindProperty("mVolume"), 0.0f, 1.0f, "", "", mVolume);
			DrawCustomSliderFloat(so.FindProperty("mPitch"), -3.0f, 3.0f, "", "", mPitch);
			DrawCustomSliderFloat(so.FindProperty("mStereoPan"), -1.0f, 1.0f, "Left", "Right", mStereoPan);
			DrawCustomSliderFloat(so.FindProperty("mSpatialBlend"), 0.0f, 1.0f, "2D", "3D", mSpatialBlend);

			CustomEditorGUIUtility.VerticalSpacer(10.0f);

			mFoldout = EditorGUILayout.Foldout(mFoldout, new GUIContent("3D Sound Settings"), true);
			if (mFoldout)
			{
				EditorGUILayout.PropertyField(so.FindProperty("mRolloffMode"), mRolloffMode);
				EditorGUILayout.PropertyField(so.FindProperty("mMinDistance"), mMinDistance);
				EditorGUILayout.PropertyField(so.FindProperty("mMaxDistance"), mMaxDistance);
			}

			serializedObject.ApplyModifiedProperties();
		}

		private void DrawCustomSliderInt(
			SerializedProperty prop, int min, int max, string minLabel, string maxLabel, GUIContent proplabel)
		{
			Rect r = EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(30.0f));
			CustomEditorGUIUtility.VerticalSpacer(30);

			const float labelPercent = 0.35f;
			Rect labelRect = new Rect(r.x, r.y, r.width * labelPercent, 17.0f);
			Rect propRect = labelRect.ShiftAlongX(r.width * (1.0f - labelPercent));

			EditorGUI.LabelField(labelRect, proplabel);
			EditorGUI.IntSlider(propRect, prop, min, max, GUIContent.none);

			Rect leftRect = new Rect(propRect.x, propRect.y + propRect.height * 0.75f, propRect.width * 0.62f, propRect.height);
			Rect rightRect = leftRect.ShiftAlongX(propRect.width * 0.5f);

			GUIStyle style1 = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperLeft };
			GUIStyle style2 = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperRight };
			EditorGUI.LabelField(leftRect, new GUIContent(minLabel), style1);
			EditorGUI.LabelField(rightRect, new GUIContent(maxLabel), style2);

			EditorGUILayout.EndHorizontal();
		}

		private void DrawCustomSliderFloat(
			SerializedProperty prop, float min, float max, string minLabel, string maxLabel, GUIContent proplabel)
		{
			Rect r = EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(30.0f));
			CustomEditorGUIUtility.VerticalSpacer(30);

			const float labelPercent = 0.35f;
			Rect labelRect = new Rect(r.x, r.y, r.width * labelPercent, 17.0f);
			Rect propRect = labelRect.ShiftAlongX(r.width * (1.0f - labelPercent));

			EditorGUI.LabelField(labelRect, proplabel);
			EditorGUI.Slider(propRect, prop, min, max, GUIContent.none);
			
			Rect leftRect = new Rect(propRect.x, propRect.y + propRect.height * 0.75f, propRect.width * 0.62f, propRect.height);
			Rect rightRect = leftRect.ShiftAlongX(propRect.width * 0.5f);

			GUIStyle style1 = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperLeft };
			GUIStyle style2 = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperRight };
			EditorGUI.LabelField(leftRect, new GUIContent(minLabel), style1);
			EditorGUI.LabelField(rightRect, new GUIContent(maxLabel), style2);

			EditorGUILayout.EndHorizontal();
		}
	}
}
