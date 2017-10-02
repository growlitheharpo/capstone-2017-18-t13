using FiringSquad.Gameplay;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	public class WeaponPartScriptBarrel : WeaponPartScript
	{
		public override BaseWeaponScript.Attachment attachPoint { get { return BaseWeaponScript.Attachment.Barrel; } }

		[SerializeField] private Transform mTip;
		public Transform barrelTip { get { return mTip; } }

		[SerializeField] private int mProjectileCount = 1;
		public int projectileCount { get { return mProjectileCount; } }

		[SerializeField] private bool mOverrideRecoilCurve;

		[SerializeField] private AnimationCurve mRecoilCurve;
		public AnimationCurve recoilCurve { get { return mOverrideRecoilCurve ? mRecoilCurve : null; } }
	}
}

#if UNITY_EDITOR

namespace UnityEditor
{
	[CustomEditor(typeof(WeaponPartScriptBarrel))]
	public class WeaponPartScriptBarrelEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			SerializedObject so = serializedObject;

			so.Update();

			EditorGUILayout.PropertyField(so.FindProperty("mData"), true);
			EditorGUILayout.PropertyField(so.FindProperty("mDescription"), true);
			EditorGUILayout.PropertyField(so.FindProperty("mDurability"), true);
			EditorGUILayout.PropertyField(so.FindProperty("mTip"), true);
			EditorGUILayout.PropertyField(so.FindProperty("mProjectileCount"), true);
			EditorGUILayout.PropertyField(so.FindProperty("mOverrideRecoilCurve"), true);

			if (so.FindProperty("mOverrideRecoilCurve").boolValue)
				EditorGUILayout.PropertyField(so.FindProperty("mRecoilCurve"), true);

			so.ApplyModifiedProperties();
		}
	}
}

#endif