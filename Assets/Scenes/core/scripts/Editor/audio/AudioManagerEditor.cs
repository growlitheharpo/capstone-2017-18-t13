using System;
using System.Collections.Generic;
using System.Linq;
using FiringSquad.Core.Audio;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor
{
	[CustomEditor(typeof(AudioManager))]
	public class AudioManagerEditor : Editor
	{
		private ReorderableList mList;
		private SerializedProperty mEventsProperty;

		/// <summary>
		/// Called when the list is enabled. Connects the ReorderableList to the property.
		/// </summary>
		private void OnEnable()
		{
			mEventsProperty = serializedObject.FindProperty("mEventBindList");
			mList = new ReorderableList(serializedObject, mEventsProperty, false, true, true, true)
			{
				drawHeaderCallback = rect =>
				{
					EditorGUI.LabelField(rect, "Audio Event Map");
				},
				elementHeightCallback = GetElementHeight,
				drawElementCallback = DrawElement
			};
		}

		/// <summary>
		/// Unity's OnInspectorGUI callback
		/// </summary>
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			DrawDefaultInspector();
			DrawWarningBox();
			mList.DoLayoutList();
			serializedObject.ApplyModifiedProperties();
		}

		/// <summary>
		/// This function is really, really ugly.
		/// The point of it is to draw a warning box whenever there are AudioEvent enum values
		/// that do not have an FMOD bind.
		/// </summary>
		private void DrawWarningBox()
		{
			SerializedProperty prop = serializedObject.FindProperty("mEventBindList");
			var propEvents = new List<AudioEvent>();
			for (int i = 0; i < prop.arraySize; i++)
			{
				SerializedProperty element = prop.GetArrayElementAtIndex(i).FindPropertyRelative("mEnumVal");
				string s = CustomEditorGUIUtility.GetEnumPropertyString(element);
				propEvents.Add((AudioEvent)Enum.Parse(typeof(AudioEvent), s));
			}

			var allVals = Enum.GetValues(typeof(AudioEvent)).Cast<AudioEvent>();
			var missingItems = allVals.Where(x => !propEvents.Contains(x)).ToArray();
			if (missingItems.Length == 0)
				return;

			string missingItemsString = string.Join("\n", missingItems.Select(x => x.ToString()).ToArray());
			EditorGUILayout.HelpBox("The following audio events are missing an FMOD bind: \n" + missingItemsString, MessageType.Warning);
		}

		/// <summary>
		/// Get the height of a single element in the event list.
		/// </summary>
		/// <param name="index">The index of the item.</param>
		private float GetElementHeight(int index)
		{
			SerializedProperty element = mList.serializedProperty.GetArrayElementAtIndex(index);

			float oneLine = EditorGUIUtility.singleLineHeight;

			if (!element.isExpanded)
				return oneLine * 1.5f;

			return oneLine * 3.0f + EditorGUI.GetPropertyHeight(element.FindPropertyRelative("mFmodVal"));
		}

		/// <summary>
		/// Draw an element in our event list.
		/// </summary>
		/// <param name="rect">Rect we can draw in.</param>
		/// <param name="index">Index of the item we can draw.</param>
		/// <param name="isActive">Whether or not the item is active.</param>
		/// <param name="isFocused">Whether or not this item is focused.</param>
		private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
		{
			rect = new Rect(rect.x + 5.0f, rect.y, rect.width, rect.height);
			float oneline = EditorGUIUtility.singleLineHeight;

			SerializedProperty element = mList.serializedProperty.GetArrayElementAtIndex(index);
			SerializedProperty enumval = element.FindPropertyRelative("mEnumVal");
			SerializedProperty fmod = element.FindPropertyRelative("mFmodVal");

			//Rect r1 = new Rect(rect.x, rect.y + 5.0f + oneline, rect.width, oneline);
			//Rect r2 = new Rect(r1.x, r1.y + r1.height + 10.0f, rect.width, rect.height - r1.height);
			Rect r1 = new Rect(rect.x, rect.y, rect.width, oneline);
			Rect r2 = new Rect(rect.x, r1.y + r1.height + 5.0f, rect.width, oneline);
			Rect r3 = new Rect(rect.x, r2.y + r2.height + 5.0f, rect.width, rect.height - (r2.y + r2.height + 5.0f));

			string enumName = CustomEditorGUIUtility.GetEnumPropertyString(enumval);
			element.isExpanded = EditorGUI.Foldout(r1, element.isExpanded, new GUIContent(enumName), true);

			if (!element.isExpanded)
				return;


			EditorGUI.PropertyField(r2, enumval, GUIContent.none);
			EditorGUI.PropertyField(r3, fmod, GUIContent.none);
		}
	}
}
