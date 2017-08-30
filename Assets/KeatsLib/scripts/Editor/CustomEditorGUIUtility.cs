using System;
using KeatsLib.Unity;
using UnityEditor;
using UnityEngine;

// ReSharper disable once InconsistentNaming
public static class CustomEditorGUIUtility
{
	public const float LINE_HEIGHT_MODIFIER = 1.05f;
	private static float kOneLineHeight;

	public static void SetLineHeight(float h)
	{
		kOneLineHeight = h;
	}

	public static float GetLineHeight()
	{
		return kOneLineHeight;
	}
	
	public static void DrawScriptField(SerializedObject serializedObject)
	{
		DrawDisabled(() => EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"), true));
	}
	
	public static void DrawList(
		SerializedProperty prop, string label, string deleteLabel = "X",
		string addLabel = "+", float deleteWidth = 0.2f, GUIContent itemLabel = null)
	{
		EditorGUILayout.LabelField(label);
		
		EditorGUI.indentLevel++;

		for (int i = 0; i < prop.arraySize; i++)
		{
			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.PropertyField(prop.GetArrayElementAtIndex(i), itemLabel, true);
			
			if (GUILayout.Button(deleteLabel, GUILayout.MaxWidth(25.0f)))
			{
				prop.DeleteArrayElementAtIndex(i);
				return;
			}

			EditorGUILayout.EndHorizontal();
		}

		if (GUILayout.Button(addLabel))
			prop.InsertArrayElementAtIndex(prop.arraySize);

		EditorGUI.indentLevel--;
	}

	public static void DrawList(
		Rect pos, SerializedProperty prop, string label, string deleteLabel = "X",
		string addLabel = "+", GUIContent itemLabel = null)
	{

		const float delW = 25.0f;
		float propW = pos.width - delW;

		float startY = label != "" ? DrawLabel(pos, label) : pos.y;

		Rect propRect = new Rect(pos.x, startY, propW, kOneLineHeight);
		Rect delRect = propRect.ShiftAlongX(delW, kOneLineHeight);
		
		EditorGUI.indentLevel++;
		propRect = itemLabel == null
			? DrawDefaultList(prop, propRect, delRect, deleteLabel)
			: DrawListWithLabel(prop, propRect, delRect, deleteLabel, itemLabel);

		if (propRect == Rect.zero)
		{
			EditorGUI.indentLevel--;
			return;
		}

		float indentSize = EditorGUI.indentLevel * 15; //let's say that's roughly right for now.
		Rect addRect = new Rect(pos.x + indentSize, propRect.y, pos.width - indentSize, kOneLineHeight);

		if (GUI.Button(addRect, addLabel))
			prop.InsertArrayElementAtIndex(prop.arraySize);

		EditorGUI.indentLevel--;
	}
	
	private static float DrawLabel(Rect pos, string label)
	{
		Rect labelRect = new Rect(pos.x, pos.y, pos.width, kOneLineHeight);
		EditorGUI.LabelField(labelRect, label);

		return labelRect.y + labelRect.height;
	}

	private static Rect DrawDefaultList(SerializedProperty prop, Rect propRect, Rect delRect, string deleteLabel)
	{
		for (int i = 0; i < prop.arraySize; i++)
		{
			EditorGUI.PropertyField(propRect, prop.GetArrayElementAtIndex(i), GUIContent.none, true);

			if (GUI.Button(delRect, new GUIContent(deleteLabel)))
			{
				prop.DeleteArrayElementAtIndex(i);
				return Rect.zero;
			}

			propRect.y += propRect.height;
			delRect.y += delRect.height;
		}

		return propRect;
	}

	private static Rect DrawListWithLabel(SerializedProperty prop, Rect propRect, Rect delRect, string deleteLabel, GUIContent itemLabel)
	{
		Rect labelRect = propRect;
		labelRect.width *= 0.2f;

		propRect.width -= labelRect.width;
		propRect.x += labelRect.width;

		GUIStyle style = GUI.skin.label;
		style.fixedWidth = labelRect.width;

		for (int i = 0; i < prop.arraySize; i++)
		{
			EditorGUI.LabelField(labelRect, new GUIContent(itemLabel.text + (i + 1)), style);
			EditorGUI.PropertyField(propRect, prop.GetArrayElementAtIndex(i), GUIContent.none, true);

			if (GUI.Button(delRect, new GUIContent(deleteLabel)))
			{
				prop.DeleteArrayElementAtIndex(i);
				return Rect.zero;
			}

			labelRect.y += labelRect.height;
			propRect.y += propRect.height;
			delRect.y += delRect.height;
		}

		return propRect;
	}

	public static void HorizontalLayout(Action guiFunc)
	{
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();

		guiFunc.Invoke();

		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}

	public static void DrawDisabled(Action guiFunc)
	{
		bool state = GUI.enabled;
		GUI.enabled = false;

		guiFunc.Invoke();

		GUI.enabled = state;
	}

	public static void VerticalSpacer(float size)
	{
		GUILayout.BeginVertical();
		GUILayout.Space(size);
		GUILayout.EndVertical();
	}
}
