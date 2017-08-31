using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class CustomPostBuildProcessor : MonoBehaviour
{
	[PostProcessBuild(1)]
	public static void OnPostprocessBuild(BuildTarget target, string pathToBuildProject)
	{
		try
		{
			if (IsOfficialBuild(pathToBuildProject))
				PerformCopy(pathToBuildProject);
		}
		catch (Exception e)
		{
			Debug.LogWarning("Unable to perform post-build copy to SVN for the following reason:\n" + e.Message);
			throw;
		}
	}

	/// <summary>
	/// Returns whether or not this build fits the "official" naming convention:
	/// build-17-MM-DD-[qa|demo]-v0.00[x].exe
	/// </summary>
	private static bool IsOfficialBuild(string pathToBuildProject)
	{
		string filename = Path.GetFileNameWithoutExtension(pathToBuildProject);
		return filename != null && new Regex("(build-)(17-(\\d\\d-){2})((qa-)|(demo-))(v\\d\\.\\d\\d[a-z]?)").IsMatch(filename);
	}

	private static void PerformCopy(string pathToBuild)
	{
		string currentProjectPath = Path.GetFullPath(Application.dataPath + "\\..");
		string targetOutput = Path.GetFullPath(currentProjectPath + "-buildtarget\\builds\\");
		Debug.Log(pathToBuild);
		Debug.Log(currentProjectPath);
		Debug.Log(targetOutput);

		string buildName = Path.GetFileNameWithoutExtension(pathToBuild);
		string dataFolderName = buildName + "_Data";

		// ReSharper disable once PossibleNullReferenceException
		string dataFolder = new FileInfo(pathToBuild).Directory.FullName + "\\" + dataFolderName;

		//First, copy the .exe
		File.Copy(pathToBuild, targetOutput + buildName + ".exe", true);

		//Then, copy everything in the data folder.
		if (Directory.Exists(targetOutput + dataFolderName))
			Directory.Delete(targetOutput + dataFolderName, true);
	}
}
