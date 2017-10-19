using FiringSquad.Data;
using KeatsLib.Unity;
using UnityEngine;

namespace UnityEditor
{
	[CustomPropertyDrawer(typeof(Modifier.Float))]
	public class ModifierFloatDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return base.GetPropertyHeight(property, label) * 2.25f;
		}

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

	[CustomPropertyDrawer(typeof(Modifier.Int))]
	public class ModifierIntDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return base.GetPropertyHeight(property, label) * 2.25f;
		}

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
