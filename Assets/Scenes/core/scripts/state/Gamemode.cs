using System;
using System.Linq;
using UnityEngine;

public class Gamemode : MonoBehaviour
{
	[Serializable]
	public class MyGunSettings
	{
		[SerializeField] private bool mEnableAIDrops;
		public bool enableAIDrops { get { return mEnableAIDrops; } }

		[SerializeField] private bool mEnableDurability;
		public bool enableDurability { get { return mEnableDurability; } }
	}

	[Serializable]
	public class QuickdrawSettings
	{
		[SerializeField] private bool mEnableAIDrops = true;
		public bool enableAIDrops { get { return mEnableAIDrops; } }

		[SerializeField] private bool mEnableDurability = true;
		public bool enableDurability { get { return mEnableDurability; } }
	}

	[Serializable]
	public class ArenaSettings
	{
		[SerializeField] private float mRoundTime;
		public float roundTime { get { return mRoundTime; } }
		
		[SerializeField] private bool mEnableDurability;
		public bool enableDurability { get { return mEnableDurability; } }

		[SerializeField] private GameObject mSpawnPointHolder;
		public Transform[] spawnPoints { get { return mSpawnPointHolder.transform.Cast<Transform>().ToArray(); } }

		[SerializeField] private GameObject mDeathParticles;
		public GameObject deathParticles { get { return mDeathParticles; } }

		[SerializeField] private GameObject mHitParticles;
		public GameObject hitParticles { get { return mHitParticles; } }
	}

	public enum Mode
	{
		MyGun,
		QuickDraw,
		Arena,
	}

	[SerializeField] private Mode mMode;
	public Mode mode { get { return mMode; } }

	[SerializeField] private MyGunSettings mMyGunSettings;
	public MyGunSettings mygunSettings { get { return mMyGunSettings; } }

	[SerializeField] private QuickdrawSettings mQuickdrawSettings;
	public QuickdrawSettings quickdrawSettings { get { return mQuickdrawSettings; } }

	[SerializeField] private ArenaSettings mArenaSettings;
	public ArenaSettings arenaSettings { get { return mArenaSettings; } }
}

#if UNITY_EDITOR
namespace UnityEditor
{
	[CustomEditor(typeof(Gamemode))]
	public class GamemodeEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("mMode"));

			Gamemode t = (target as Gamemode);
			if (t == null)
				return;

			SerializedProperty prop;

			switch (t.mode)
			{
				case Gamemode.Mode.MyGun:
					prop = serializedObject.FindProperty("mMyGunSettings");
					prop.isExpanded = true;
					EditorGUILayout.PropertyField(prop, true);
					break;
				case Gamemode.Mode.QuickDraw:
					prop = serializedObject.FindProperty("mQuickdrawSettings");
					prop.isExpanded = true;
					EditorGUILayout.PropertyField(prop, true);
					break;
				case Gamemode.Mode.Arena:
					prop = serializedObject.FindProperty("mArenaSettings");
					prop.isExpanded = true;
					EditorGUILayout.PropertyField(prop, true);
					break;
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif