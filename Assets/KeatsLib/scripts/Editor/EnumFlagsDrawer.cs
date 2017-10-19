// http://answers.unity3d.com/answers/514102/view.html

using KeatsLib.Unity;
using UnityEngine;

namespace UnityEditor
{
	/// <summary>
	/// Used to draw flags in the inspector.
	/// </summary>
	[CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
	public class EnumFlagsAttributeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			property.intValue = EditorGUI.MaskField(position, label, property.intValue, property.enumDisplayNames);
		}
	}
}
