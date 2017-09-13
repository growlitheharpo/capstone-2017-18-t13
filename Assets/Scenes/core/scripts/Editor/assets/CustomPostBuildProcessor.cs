using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UnityEditor
{
	/// <summary>
	/// Create a build and auto-upload it to SVN.
	/// Basically a giant state machine that tries to wait for each step.
	/// </summary>
	public class CustomPostBuildProcessor
	{
		private static string kVersionNumber, kProjectPath, kCurrentGitBranch, kTagFilePath;
		private static Action kCurrentState;
		private static bool kRunAfterBuild, kCommitBuild;

		private static void Waiting() { }

		[MenuItem("Pipeline/Create Build")]
		public static void DoBuild()
		{
			kRunAfterBuild = false;
			kCommitBuild = true;
			kProjectPath = Path.GetFullPath(Application.dataPath + "\\..");
			EditorApplication.update += Update;
			kCurrentState = UpdateRepoState;
		}
		
		[MenuItem("Pipeline/Create Build and Run")]
		public static void DoBuildAndRun()
		{
			DoBuild();
			kRunAfterBuild = true;
		}
		
		[MenuItem("Pipeline/Build Without Auto-Upload")]
		public static void DoBuildNoUpload()
		{
			DoBuild();
			kRunAfterBuild = true;
			kCommitBuild = false;
		}
		
		private static void Update()
		{
			kCurrentState.Invoke();
		}

		#region States

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
			MakeBuild();

			kCurrentState = CreateOrUpdateTagState;
		}

		private static void CreateOrUpdateTagState()
		{
			Debug.Log("Build complete! Saving tag file.");
			WriteTag();

			kCurrentState = AddAllFilesToCloudRepoState;
		}

		private static void AddAllFilesToCloudRepoState()
		{
			Debug.Log("Tag written. Adding all files to SVN.");
			AddFilesToRepo();

			kCurrentState = Waiting;
		}
		
		private static void CommitNewBuildState()
		{
			if (kCommitBuild)
			{
				Debug.Log("Files added. Committing to SVN.");
				CommitToCloudRepo();

				kCurrentState = Waiting;
			}
			else
			{
				Debug.Log("Files added. Stopping before upload as requested.");
				Complete();
			}
		}
		
		#endregion

		#region Termination Functions

		private static void LogProcessFailure()
		{
			Debug.LogError("Unable to complete the build. See output window for details.");
			Complete();
		}
		
		public static void Complete()
		{
			EditorApplication.update -= Update;
		}

		#endregion

		#region Worker Functions

		private static void UpdateCloudRepo(string projectPath)
		{
			SvnWrapper window = EditorWindow.GetWindow<SvnWrapper>(true, "SVN", true);
			window.Initialize(projectPath, "checkout https://pineapple.champlain.edu/svn/capstone-2017-18-t13.svn@HEAD CloudBuild");
			window.OnProcessFail += LogProcessFailure;
			window.OnProcessComplete += () => {
				kCurrentState = GetLatestVersionState;
			};
		}

		private static void GetLatestVersionNumber(string currentProjectPath)
		{
			kCurrentGitBranch = GitProcess.Launch(currentProjectPath, "rev-parse --abbrev-ref HEAD");
			string branchVersionPointer = kCurrentGitBranch.Replace('/', '_') + ".txt";

			string tagPath = currentProjectPath + "\\CloudBuild\\tags\\";

			kTagFilePath = tagPath + branchVersionPointer;

			string currentValue = "0.00a";
			if (File.Exists(kTagFilePath))
				currentValue = File.ReadAllLines(kTagFilePath)[0];
			else if (File.Exists(tagPath + "master.txt"))
				currentValue = File.ReadAllLines(tagPath + "master.txt")[0];

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

		private static void MakeBuild()
		{
			string path = kProjectPath + "\\CloudBuild\\builds\\" + kCurrentGitBranch.Replace('/', '\\') + "\\";

			string stringDate = DateTime.Now.ToString("yyMMdd", System.Globalization.CultureInfo.InvariantCulture);
			string buildName = "build-" + stringDate + "-" + kVersionNumber;

			var scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);
			BuildPlayerOptions options = new BuildPlayerOptions
			{
				options = kRunAfterBuild ? BuildOptions.AutoRunPlayer : BuildOptions.None,
				scenes = scenes,
				target = BuildTarget.StandaloneWindows,
				locationPathName = path + buildName + ".exe",
			};

			BuildPipeline.BuildPlayer(options);
		}

		private static void WriteTag()
		{
			if (!File.Exists(kTagFilePath))
			{
				using(StreamWriter file = File.CreateText(kTagFilePath))
					file.WriteLine(kVersionNumber);
			}
			else
			{
				using (FileStream file = File.OpenWrite(kTagFilePath))
				using (StreamWriter writer = new StreamWriter(file))
					writer.WriteLine(kVersionNumber);
			}
		}

		private static void AddFilesToRepo()
		{
			SvnWrapper window = EditorWindow.GetWindow<SvnWrapper>(true, "SVN", true);
			window.Initialize(kProjectPath + "\\CloudBuild", "add * --force");
			window.OnProcessFail = LogProcessFailure;
			window.OnProcessComplete = () => {
				kCurrentState = CommitNewBuildState;
			};
		}

		private static void CommitToCloudRepo()
		{
			SvnWrapper window = EditorWindow.GetWindow<SvnWrapper>(true, "SVN", true);
			string commitMessage = string.Format("BUILD {0}-{1}", kCurrentGitBranch, kVersionNumber);
			window.Initialize(kProjectPath + "\\CloudBuild", "commit -m \"" + commitMessage + "\"");
			window.OnProcessFail = LogProcessFailure;
			window.OnProcessComplete = Complete;
		}

		#endregion
		
		private class SvnWrapper : EditorWindow
		{
			public Action OnProcessComplete = () => { }, OnProcessFail = () => { };
			private Process mSvnProcess;
			private string mOutput, mInput, mErrorOutput = "";
			private enum Status { Running, Success, Fail, Stalled };
			private Status mStatus;

			public void Initialize(string path, string args)
			{
				try
				{
					mSvnProcess = new Process
					{
						StartInfo =
						{
							CreateNoWindow = true,
							UseShellExecute = false,
							RedirectStandardOutput = true,
							RedirectStandardInput = true,
							RedirectStandardError = true,
							FileName = "svn",
							WorkingDirectory = path,
							Arguments = args
						},
						EnableRaisingEvents = true
					};
					mSvnProcess.Exited += SvnProcess_Exited;

					mSvnProcess.Start();
					mStatus = Status.Running;
				}
				catch (Exception e)
				{
					mErrorOutput += e.Message;
					mStatus = Status.Fail;
				}
			}

			private void SvnProcess_Exited(object sender, EventArgs e)
			{
				int exitCode = mSvnProcess.ExitCode;
				
				var buffer = new char[2048];
				mSvnProcess.StandardError.Read(buffer, 0, 2048);
				mErrorOutput += new string(buffer).TrimEnd('\0');

				mSvnProcess.WaitForExit();
				mSvnProcess.Dispose();

				mStatus = exitCode == 0 ? Status.Success : Status.Fail;
			}

			private void OnGUI()
			{
				GUIStyle style = new GUIStyle(GUI.skin.label)
				{
					alignment = TextAnchor.UpperLeft,
				};
				var buffer = new char[2048];

				switch (mStatus) 
				{
					case Status.Success:
						OnProcessComplete();
						Close();
						break;
					case Status.Fail:
						mStatus = Status.Stalled;
						GUILayout.Box(mErrorOutput, style);
						OnProcessFail();
						break;
					case Status.Stalled:
						GUILayout.Box(mErrorOutput, style);
						break;
					default:
						if (mSvnProcess != null)
						{
							mSvnProcess.StandardOutput.Read(buffer, 0, 2048);
							mOutput += new string(buffer).TrimEnd('\0');
						}

						GUILayout.Box(mOutput, style);
						mInput = GUILayout.TextField(mInput);

						if (GUILayout.Button("Enter") && mSvnProcess != null)
							mSvnProcess.StandardInput.WriteLine(mInput);
						break;
				}
			}

			private void OnDestroy()
			{
				if (mSvnProcess == null)
					return;

				try
				{
					mSvnProcess.Kill();
				}
				catch (Exception)
				{
					// ignored
				}
				mSvnProcess.Dispose();
			}
		}
	}
}