using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor.Callbacks;
using Debug = UnityEngine.Debug;

namespace UnityEditor
{
	public class CustomPostBuildProcessor : MonoBehaviour
	{
		private class GetVersionWindow : EditorWindow
		{
			public Action<string> continueAction { get; set; }
			public string currentVersion { get; set; }

			private void OnGUI()
			{
				CustomEditorGUIUtility.HorizontalLayout(() => { GUILayout.Label("Choose Version"); });
				CustomEditorGUIUtility.HorizontalLayout(() =>
				{
					GUILayout.Box(
						"Please enter the new version for this build. The form is auto-filled with the current version. " +
						"Increment either the version number or version letter as appropriate.", GUILayout.MaxWidth(350.0f));
				});

				if (!GUILayout.Button(new GUIContent("Accept")))
					return;

				Close();
				continueAction(currentVersion);
			}

			public static GetVersionWindow Get()
			{
				return GetWindow<GetVersionWindow>(true, "Choose Version", true);
			}
		}

		[MenuItem("Pipeline/Official Build")]
		public static void DoBuild()
		{
			string currentProjectPath = Path.GetFullPath(Application.dataPath + "\\..");

			Debug.Log("Getting the latest version of the build SVN repo.");
			UpdateCloudRepo(currentProjectPath);
			Debug.Log("Success!");

			Debug.Log("Finding latest version number.");
			GetLatestVersionNumber(currentProjectPath);
		}

		private static void ContinueWithVersionNumber(string number)
		{
			Debug.Log("Success! Using version " + number);
		}

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
			string targetOutput = Path.GetFullPath(currentProjectPath + "\\CloudBuild\\builds\\");
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

		private static void UpdateCloudRepo(string projectPath)
		{
			DoSvnProcess(projectPath, "checkout https://pineapple.champlain.edu/svn/capstone-2017-18-t13.svn@HEAD CloudBuild");
		}

		private static void GetLatestVersionNumber(string currentProjectPath)
		{
			string currentBranch = DoGitProcess(currentProjectPath, "rev-parse --abbrev-ref HEAD");
			string branchVersionPointer = currentBranch.Replace('/', '_') + ".txt";

			string tagPath = currentProjectPath + "\\CloudBuild\\tags\\";

			Debug.Log("Looking for " + tagPath + branchVersionPointer);
			if (File.Exists(tagPath + branchVersionPointer))
			{
				string version = File.ReadAllText(tagPath + branchVersionPointer);
				GetVersionWindow window = EditorWindow.GetWindow<GetVersionWindow>(true, "Choose Version", true);
				window.continueAction = ContinueWithVersionNumber;
				window.currentVersion = version;
				return;
			}
			if (File.Exists(tagPath + "master.txt"))
			{
				string masterVersion = File.ReadAllText(tagPath + "master.txt");
				GetVersionWindow window = EditorWindow.GetWindow<GetVersionWindow>(true, "Choose Version", true);
				window.continueAction = ContinueWithVersionNumber;
				window.currentVersion = masterVersion;
				return;
			}

			ContinueWithVersionNumber("0.00a");
		}

		private static string DoSvnProcess(string location, string args, int bufferSize = 2048)
		{
			using (Process p = new Process())
			{
				p.StartInfo.CreateNoWindow = true;
				p.StartInfo.UseShellExecute = false;
				p.StartInfo.RedirectStandardOutput = true;
				p.StartInfo.FileName = "svn";
				p.StartInfo.WorkingDirectory = location;
				p.StartInfo.Arguments = args;
				p.Start();

				var buffer = new char[bufferSize];
				p.StandardOutput.Read(buffer, 0, bufferSize);
				p.WaitForExit();

				return new string(buffer).TrimEnd('\n', '\0');
			}
		}

		private static string DoGitProcess(string location, string args, int bufferSize = 2048)
		{
			using (Process p = new Process())
			{
				p.StartInfo.CreateNoWindow = true;
				p.StartInfo.UseShellExecute = false;
				p.StartInfo.RedirectStandardOutput = true;
				p.StartInfo.FileName = "git";
				p.StartInfo.WorkingDirectory = location;
				p.StartInfo.Arguments = args;
				p.Start();

				var buffer = new char[bufferSize];
				p.StandardOutput.Read(buffer, 0, bufferSize);
				p.WaitForExit();

				return new string(buffer).TrimEnd('\n', '\0');
			}
		}
	}
}