using System;
using UnityEngine;

namespace UnityEditor
{
	public class DebugAssetFinder : EditorWindow
	{
		[MenuItem("Debug/Asset Finder")]
		public static void CreateAssetFinder()
		{
			GetWindow<DebugAssetFinder>(true, "Asset Finder", true);
		}

		private string mID;
		private GameObject mObject;

		private void OnEnable()
		{
			mObject = null;
			mID = "";
		}

		private void OnGUI()
		{
			mID = EditorGUILayout.TextField("Asset ID: ", mID);

			if (GUILayout.Button("Search"))
			{
				try
				{
					
				string path = AssetDatabase.GUIDToAssetPath(mID);
				if (!string.IsNullOrEmpty(path))
					mObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}

			EditorGUILayout.ObjectField("Object Reference: ", mObject, typeof(GameObject), false);
		}
	}
}
