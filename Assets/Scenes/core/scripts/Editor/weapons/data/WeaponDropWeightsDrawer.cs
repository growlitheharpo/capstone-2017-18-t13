using FiringSquad.Data;
using KeatsLib.Unity;
using UnityEngine;

namespace UnityEditor
{
	[CustomPropertyDrawer(typeof(WeaponDropWeights))]
	public class WeaponDropWeightsDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (property.isExpanded)
				return base.GetPropertyHeight(property, label) * 5;
			return base.GetPropertyHeight(property, label);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			Rect pos = new Rect(position.x, position.y, position.width, base.GetPropertyHeight(property, label));
			property.isExpanded = EditorGUI.Foldout(pos, property.isExpanded, label, true);

			if (property.isExpanded)
			{
				Rect rMech = pos.ShiftAlongY(pos.width),
					rBarrel = rMech.ShiftAlongY(pos.width),
					rScope = rBarrel.ShiftAlongY(pos.width),
					rGrip = rScope.ShiftAlongY(pos.width);

				SerializedProperty pMech = property.FindPropertyRelative("mMechanismWeight"),
									pBarrel = property.FindPropertyRelative("mBarrelWeight"),
									pScope = property.FindPropertyRelative("mScopeWeight"),
									pGrip = property.FindPropertyRelative("mGripWeight");

				EditorGUI.PropertyField(rMech, pMech);
				EditorGUI.PropertyField(rBarrel, pBarrel);
				EditorGUI.PropertyField(rScope, pScope);
				EditorGUI.PropertyField(rGrip, pGrip);

				float sum = pMech.floatValue + pBarrel.floatValue + pScope.floatValue + pGrip.floatValue;

				if (sum == 0.0f)
					pMech.floatValue = 1.0f;
				else
				{
					pMech.floatValue /= sum;
					pBarrel.floatValue /= sum;
					pScope.floatValue /= sum;
					pGrip.floatValue /= sum;
				}
			}
		}
	}
}
