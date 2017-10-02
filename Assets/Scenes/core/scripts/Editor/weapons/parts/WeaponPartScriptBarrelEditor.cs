using FiringSquad.Gameplay;

namespace UnityEditor
{
	[CustomEditor(typeof(WeaponPartScriptBarrel))]
	public class WeaponPartScriptBarrelEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			SerializedObject so = serializedObject;
			so.Update();

			DrawDefaultInspector();

			if (so.FindProperty("mOverrideRecoilCurve").boolValue)
				EditorGUILayout.PropertyField(so.FindProperty("mRecoilCurve"), true);

			so.ApplyModifiedProperties();
		}
	}
}
