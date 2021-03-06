﻿using UnityEngine;

namespace UnityEditor
{
	/// <summary>
	/// Custom inspector for crates.
	/// Auto-balances weights and draws a prettier list.
	/// </summary>
	[CustomEditor(typeof(FiringSquad.Gameplay.WeaponPartPad))][CanEditMultipleObjects]
	public class WeaponPartPadEditor : Editor
	{
		private UnityEditorInternal.ReorderableList mList;
		private SerializedProperty mPartsProperty;

		/// <summary>
		/// Called when the list is enabled. Connects a ReorderableList to the property
		/// </summary>
		private void OnEnable()
		{
			mPartsProperty = serializedObject.FindProperty("mParts");
			mList =
				new UnityEditorInternal.ReorderableList(serializedObject, mPartsProperty, true, true, true, true)
				{
					drawHeaderCallback = rect =>
					{
						EditorGUI.LabelField(rect, "Parts and Weights");
					},
					drawElementCallback = (rect, index, isActive, isFocused) =>
					{
						SerializedProperty element = mList.serializedProperty.GetArrayElementAtIndex(index);
						EditorGUI.PropertyField(new Rect(rect.x, rect.y, 150.0f, EditorGUIUtility.singleLineHeight),
							element.FindPropertyRelative("mPrefab"), GUIContent.none, true);

						EditorGUI.PropertyField(new Rect(rect.x + 150.0f, rect.y, rect.width - 150.0f, EditorGUIUtility.singleLineHeight),
							element.FindPropertyRelative("mWeight"), GUIContent.none, true);
					}
				};
		}

		/// <inheritdoc />
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			DrawDefaultInspector();

			mList.DoLayoutList();
			BalanceWeights();

			if (GUILayout.Button("Snap To Floor"))
				SnapToFloor(target as Component);

			serializedObject.ApplyModifiedProperties();
		}

		/// <summary>
		/// Balance all of our sliders.
		/// </summary>
		private void BalanceWeights()
		{
			float sum = 0.0f;
			for (int i = 0; i < mPartsProperty.arraySize; i++)
				sum += mPartsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("mWeight").floatValue;

			if (sum <= 0.0f)
				return;

			for (int i = 0; i < mPartsProperty.arraySize; i++)
				mPartsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("mWeight").floatValue /= sum;
		}

		/// <summary>
		/// Snap the crates to the floor.
		/// </summary>
		/// <param name="component"></param>
		private static void SnapToFloor(Component component)
		{
			Transform t = component.transform;
			Ray r = new Ray(t.position, Vector3.down);

			RaycastHit hitInfo;
			if (!Physics.Raycast(r, out hitInfo))
				return;

			Vector3 point = hitInfo.point;
			t.position = point;
		}
	}
}
