﻿using System;
using UnityEngine;

namespace UnityEditor
{
	public class SimpleTextEntryWindow : EditorWindow
	{
		public event Action<string> OnSubmit = s => { };
		private Func<string, string> guiUtilityFunc { get; set; }
		private string startValue { get; set; }
		private string infoLabel { get; set; }

		private void OnGUI()
		{
			CustomEditorGUIUtility.HorizontalLayout(() =>
			{
				GUILayout.Box(infoLabel, GUILayout.MaxWidth(350.0f));
			});

			startValue = guiUtilityFunc(startValue);

			if (!GUILayout.Button(new GUIContent("OK")))
				return;

			Close();
			OnSubmit(startValue);
		}

		public static SimpleTextEntryWindow Initialize(string label, string startValue, string infoLabel, Func<string, string> guiUtilityFunc)
		{
			SimpleTextEntryWindow window = GetWindow<SimpleTextEntryWindow>(true, label, true);
			window.startValue = startValue;
			window.infoLabel = infoLabel;
			window.guiUtilityFunc = guiUtilityFunc;

			return window;
		}
	}

}