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
		private static string kVersionNumber, kProjectPath, kCurrentGitBranch;
		private static Action kCurrentState;
		private static bool mRunAfterBuild;

		private static void Waiting() { }

		[MenuItem("Pipeline/Create Build")]
		public static void DoBuild()
		{
			mRunAfterBuild = false;
			kProjectPath = Path.GetFullPath(Application.dataPath + "\\..");
			EditorApplication.update += Update;
			kCurrentState = UpdateRepoState;
		}
		
		[MenuItem("Pipeline/Create Build and Run")]
		public static void DoBuildAndRun()
		{
			DoBuild();
			mRunAfterBuild = true;
		}
		
		public static void Complete()
		{
			EditorApplication.update -= Update;
		}

		private static void Update()
		{
			kCurrentState.Invoke();
		}

		private static void UpdateRepoState()
		{
			Debug.Log("Getting the latest version of the build SVN repo.");
			UpdateCloudRepo(kProjectPath);
			kCurrentState = Waiting;
		}

		private static void GetLatestVersionState()
		{
			Debug.Log("Finding latest version number.");
			GetLatestVersionNumber(kProjectPath);
			kCurrentState = Waiting;
		}

		private static void MakeBuildState()
		{
			Debug.Log("Starting the build process...");
			
			//Make the actual build
			var path = kProjectPath + "\\CloudBuild\\builds\\" + kCurrentGitBranch.Replace('/', '\\') + "\\";

			var stringDate = DateTime.Now.ToString("yyMMdd", System.Globalization.CultureInfo.InvariantCulture);
			var buildName = "build-" + stringDate + "-" + kVersionNumber;

			var scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);
			var options = new BuildPlayerOptions
			{
				options = mRunAfterBuild ? BuildOptions.AutoRunPlayer : BuildOptions.None,
				scenes = scenes,
				target = BuildTarget.StandaloneWindows,
				locationPathName = path + buildName + ".exe",
			};

			BuildPipeline.BuildPlayer(options);

			kCurrentState = CreateOrUpdateTag;
		}

		private static void CreateOrUpdateTag()
		{
			Debug.Log("Build complete! Saving tag file.");
			kCurrentState = Waiting;
			Complete();
		}
		
		private static void CommitNewBuildState()
		{
			//Send the files to the SVN server
			kCurrentState = Waiting;
		}

		private static void UpdateCloudRepo(string projectPath)
		{
			var window = EditorWindow.GetWindow<SvnWrapper>(true, "SVN", true);
			window.Initialize(projectPath, "checkout https://pineapple.champlain.edu/svn/capstone-2017-18-t13.svn@HEAD CloudBuild");
			window.OnProcessFail += LogProcessFailure;
			window.OnProcessComplete += () => {
				kCurrentState = GetLatestVersionState;
			};
		}

		private static void LogProcessFailure()
		{
			Debug.LogError("Unable to complete the build. See output window for details.");
			Complete();
		}

		private static void GetLatestVersionNumber(string currentProjectPath)
		{
			kCurrentGitBranch = GitProcess.Launch(currentProjectPath, "rev-parse --abbrev-ref HEAD");
			string branchVersionPointer = kCurrentGitBranch.Replace('/', '_') + ".txt";

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
			window.OnSubmit += result => {
				kVersionNumber = result;
				kCurrentState = MakeBuildState;
			};
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
				if (svnProcess != null)
				{
					try
					{
						svnProcess.Kill();
					}
					catch (Exception) { }
					svnProcess.Dispose();
				}
			}
		}
	}
}