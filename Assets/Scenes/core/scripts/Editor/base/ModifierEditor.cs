﻿using FiringSquad.Data;
using KeatsLib.Unity;
using UnityEngine;

namespace UnityEditor
{
	/// <summary>
	/// Custom property drawer for a modifier. Draws a dropdown for mode and float for amount.
	/// </summary>
	/// <inheritdoc />
	[CustomPropertyDrawer(typeof(Modifier.Float))]
	public class ModifierFloatDrawer : PropertyDrawer
	{
		/// <inheritdoc />
		/// <summary>
		/// Returns 2.25 the one-line height for spacing.
		/// </summary>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return base.GetPropertyHeight(property, label) * 2.25f;
		}

		/// <summary>
		/// Draws the actual modifier.
		/// </summary>
		/// <inheritdoc />
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			Rect labelRect = new Rect(position.x, position.y, position.width, base.GetPropertyHeight(property, label) * 1.125f);
			Rect enumRect = labelRect.ShiftAlongY(labelRect.width / 2.0f);
			Rect valRect = enumRect.ShiftAlongX(enumRect.width);

			SerializedProperty enumProp = property.FindPropertyRelative("mType");
			SerializedProperty valProp = property.FindPropertyRelative("mAmount");

			EditorGUI.LabelField(labelRect, label);
			EditorGUI.PropertyField(enumRect, enumProp, GUIContent.none, true);
			EditorGUI.PropertyField(valRect, valProp, GUIContent.none, true);
		}
	}

	/// <summary>
	/// Custom property drawer for a modifier. Draws a dropdown for mode and float for amount.
	/// </summary>
	/// <inheritdoc />
	[CustomPropertyDrawer(typeof(Modifier.Int))]
	public class ModifierIntDrawer : PropertyDrawer
	{
		/// <inheritdoc />
		/// <summary>
		/// Returns 2.25 the one-line height for spacing.
		/// </summary>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return base.GetPropertyHeight(property, label) * 2.25f;
		}

		/// <summary>
		/// Draws the actual modifier.
		/// </summary>
		/// <inheritdoc />
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			Rect labelRect = new Rect(position.x, position.y, position.width, base.GetPropertyHeight(property, label) * 1.125f);
			Rect enumRect = labelRect.ShiftAlongY(labelRect.width / 2.0f);
			Rect valRect = enumRect.ShiftAlongX(enumRect.width);

			SerializedProperty enumProp = property.FindPropertyRelative("mType");
			SerializedProperty valProp = property.FindPropertyRelative("mAmount");

			EditorGUI.LabelField(labelRect, label);
			EditorGUI.PropertyField(enumRect, enumProp, GUIContent.none, true);
			EditorGUI.PropertyField(valRect, valProp, GUIContent.none, true);
		}
	}
}
