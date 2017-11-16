using FiringSquad.Gameplay.Weapons;

namespace UnityEditor
{
	/// <summary>
	/// Custom inspector for the weapon barrel script.
	/// </summary>
	[CustomEditor(typeof(WeaponPartScriptBarrel))]
	public class WeaponPartScriptBarrelEditor : Editor
	{
		/// <inheritdoc />
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
