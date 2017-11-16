using System;
using UnityEngine;

namespace UnityEditor
{
	/// <summary>
	/// Minor utility to find an asset in the project based on it's unique asset ID.
	/// </summary>
	public class DebugAssetFinder : EditorWindow
	{
		/// <summary>
		/// Create a window for finding an asset.
		/// </summary>
		[MenuItem("Debug/Asset Finder")]
		public static void CreateAssetFinder()
		{
			GetWindow<DebugAssetFinder>(true, "Asset Finder", true);
		}

		private string mID;
		private GameObject mObject;

		/// <summary>
		/// Handle the window being enabled.
		/// </summary>
		private void OnEnable()
		{
			mObject = null;
			mID = "";
		}

		/// <summary>
		/// Draw the inspector window.
		/// </summary>
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
