using UnityEngine;

namespace UnityEditor
{
	[CustomEditor(typeof(FiringSquad.Gameplay.WeaponPartCrate))]
	public class WeaponPartCrateEditor : Editor
	{
		private UnityEditorInternal.ReorderableList mList;
		private SerializedProperty mPartsProperty;

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

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			DrawDefaultInspector();

			mList.DoLayoutList();
			BalanceWeights();

			if (GUILayout.Button("Snap To Floor"))
				SnapToFloor((target) as Component);

			serializedObject.ApplyModifiedProperties();
		}

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

		private void SnapToFloor(Component component)
		{
			Transform t = component.transform;
			Ray r = new Ray(t.position, Vector3.down);

			RaycastHit hitInfo;
			if (!Physics.Raycast(r, out hitInfo))
				return;

			Collider col = component.GetComponent<Collider>();
			Vector3 point = hitInfo.point;
			point.y += col.bounds.extents.y;
			t.position = point;
		}
	}
}
