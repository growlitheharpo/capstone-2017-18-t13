using System;
using KeatsLib.Unity;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityEditor
{
	[CustomPropertyDrawer(typeof(ComponentTypeRestrictionAttribute))]
	public class ComponentTypeRestrictionDrawer : PropertyDrawer
	{
		private bool IsValid(SerializedProperty prop)
		{
			Type targetType = ((ComponentTypeRestrictionAttribute)attribute).mInheritsFromType;
			Object currentReference = prop.objectReferenceValue;

			return targetType.IsInstanceOfType(currentReference);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (!IsValid(property))
				return base.GetPropertyHeight(property, label) * 3.0f;
			return base.GetPropertyHeight(property, label);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			Rect pos = new Rect(position.x, position.y, position.width, base.GetPropertyHeight(property, label));
			EditorGUI.PropertyField(pos, property, label, true);

			if (IsValid(property))
				return;

			Rect pos2 = pos.ShiftAlongY(position.width, position.height * 2.0f / 3.0f);
			EditorGUI.HelpBox(pos2, "This data is the wrong type!", MessageType.Error);
		}
	}
}