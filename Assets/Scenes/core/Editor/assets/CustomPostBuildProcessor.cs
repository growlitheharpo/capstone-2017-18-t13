using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor.Callbacks;
using Debug = UnityEngine.Debug;

namespace UnityEditor
{
	public class CustomPostBuildProcessor
	{
		[MenuItem("Pipeline/Create Build")]
		public static void DoBuild()
		{
			Debug.Log("Getting the latest version of the build SVN repo.");
			string currentProjectPath = Path.GetFullPath(Application.dataPath + "\\..");
			UpdateCloudRepo(currentProjectPath);
		}

		private static void ContinueAfterRepoUpdate()
		{
			Debug.Log("Success! SVN was updated.");
			Debug.Log("Finding latest version number.");

			string currentProjectPath = Path.GetFullPath(Application.dataPath + "\\..");
			GetLatestVersionNumber(currentProjectPath);
		}

		private static void ContinueWithVersionNumber(string number)
		{
			Debug.Log("Success! Using version " + number);
		}

		/*[PostProcessBuild(1)]
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
			//string dataFolder = new FileInfo(pathToBuild).Directory.FullName + "\\" + dataFolderName;

			//First, copy the .exe
			File.Copy(pathToBuild, targetOutput + buildName + ".exe", true);

			//Then, copy everything in the data folder.
			if (Directory.Exists(targetOutput + dataFolderName))
				Directory.Delete(targetOutput + dataFolderName, true);
		}*/

		private static void UpdateCloudRepo(string projectPath)
		{
			var window = EditorWindow.GetWindow<SvnWrapper>(true, "SVN", true);
			window.Initialize(projectPath, "checkout https://pineapple.champlain.edu/svn/capstone-2017-18-t13.svn@HEAD CloudBuild");
			window.OnProcessComplete += ContinueAfterRepoUpdate;
			window.OnProcessFail += LogProcessFailure;
		}

		private static void LogProcessFailure()
		{
			Debug.LogError("Unable to complete the build. See output window for details.");
		}

		private static void GetLatestVersionNumber(string currentProjectPath)
		{
			string currentBranch = GitProcess.Launch(currentProjectPath, "rev-parse --abbrev-ref HEAD");
			string branchVersionPointer = currentBranch.Replace('/', '_') + ".txt";

			string tagPath = currentProjectPath + "\\CloudBuild\\tags\\";

			string currentValue = "0.00a";
			if (File.Exists(tagPath + branchVersionPointer))
				currentValue = File.ReadAllText(tagPath + branchVersionPointer);
			if (File.Exists(tagPath + "master.txt"))
				currentValue = File.ReadAllText(tagPath + "master.txt");

			SimpleTextEntryWindow window = SimpleTextEntryWindow.Initialize(
				"Choose Version", currentValue,
				"Please enter the new version for this build. The form is auto-filled with the current version. " +
				"Increment either the version number or version letter as appropriate.",
				val => GUILayout.TextArea(val));
			window.OnSubmit += ContinueWithVersionNumber;
		}
		
		private class SvnWrapper : EditorWindow
		{
			public event Action OnProcessComplete = () => { }, OnProcessFail = () => { };
			private Process svnProcess;
			private string output;
			private string errorOutput = "";
			private string input;
			private enum Status { Running, Success, Fail, Stalled };
			private Status mStatus;

			public void Initialize(string path, string args)
			{
				svnProcess = new Process();
				svnProcess.StartInfo.CreateNoWindow = true;
				svnProcess.StartInfo.UseShellExecute = false;
				svnProcess.StartInfo.RedirectStandardOutput = true;
				svnProcess.StartInfo.RedirectStandardInput = true;
				svnProcess.StartInfo.RedirectStandardError = true;
				svnProcess.StartInfo.FileName = "svn";
				svnProcess.StartInfo.WorkingDirectory = path;
				svnProcess.StartInfo.Arguments = args;
				svnProcess.EnableRaisingEvents = true;
				svnProcess.Exited += SvnProcess_Exited;

				svnProcess.Start();
				mStatus = Status.Running;
			}

			private void SvnProcess_Exited(object sender, EventArgs e)
			{
				int exitCode = svnProcess.ExitCode;
				
				var buffer = new char[2048];
				svnProcess.StandardError.Read(buffer, 0, 2048);
				errorOutput += new string(buffer).TrimEnd('\0');

				svnProcess.WaitForExit();
				svnProcess.Dispose();

				if (exitCode == 0)
					mStatus = Status.Success;
				else
					mStatus = Status.Fail;
			}

			private void OnGUI()
			{
				GUIStyle style = new GUIStyle(GUI.skin.label)
				{
					alignment = TextAnchor.UpperLeft,
				};
				var buffer = new char[2048];

				if (mStatus == Status.Success)
				{
					Close();
					OnProcessComplete();
				}
				else if (mStatus == Status.Fail)
				{
					mStatus = Status.Stalled;
					GUILayout.Box(errorOutput, style, GUILayout.MaxWidth(500.0f));
					OnProcessFail();
				}
				else if (mStatus == Status.Stalled)
				{
					GUILayout.Box(errorOutput, style, GUILayout.MaxWidth(500.0f));
					return;
				}
				else
				{
					if (svnProcess != null && svnProcess.StandardOutput != null)
					{
						svnProcess.StandardOutput.Read(buffer, 0, 2048);
						output += new string(buffer).TrimEnd('\0');
					}

					GUILayout.Box(output, style, GUILayout.MaxWidth(500.0f));
					input = GUILayout.TextField(input);

					if (GUILayout.Button("Enter"))
						svnProcess.StandardInput.WriteLine(input);
				}
			}

			private void OnDestroy()
			{
				if (mStatus == Status.Running && svnProcess.Responding)
				{
					svnProcess.Kill();
					svnProcess.Dispose();
				}
			}
		}
	}
}