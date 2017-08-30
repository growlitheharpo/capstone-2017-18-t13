using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

[InitializeOnLoad]
public class GitHookInstaller : EditorWindow
{
	private delegate void GUIState();

	public struct FiringSquad
	{
		public static readonly string[] POSSIBLE_NAMES =
		{
			"Charles Carucci",
			"James Keats",
			"Max Sanel",
			"Tyler Bolster"
		};

		public static readonly string[] POSSIBLE_EMAILS =
		{
			"charles.carucci@mymail.champlain.edu",
			"james.keats@mymail.champlain.edu",
			"max.sanel@mymail.champlain.edu",
			"tyler.bolster@mymail.champlain.edu"
		};
	}

	private static readonly bool DO_ONCE;
	private static List<string> kMissingFiles;
	private GUIState mCurrentState;

	private GUIStyle mHeaderStyle;
	private int mNameIndex, mEmailIndex;

	static GitHookInstaller()
	{
		if (DO_ONCE)
			return;

		DO_ONCE = true;
		EditorApplication.update += DoCheck;
	}

	private void Awake()
	{
		titleContent = new GUIContent("Firing Squad: Git Setup");
		minSize = new Vector2(450.0f, 200.0f);
	}

	private void OnGUI()
	{
		if (mCurrentState == null)
			mCurrentState = InitialPopup;
		if (mHeaderStyle == null)
			mHeaderStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold };

		mCurrentState.Invoke();
	}

	private void InitialPopup()
	{
		CustomEditorGUIUtility.HorizontalLayout(() => { GUILayout.Label("Git Setup", mHeaderStyle); });
		CustomEditorGUIUtility.HorizontalLayout(() =>
		{
			GUILayout.Box(
				"It appears you do not have the latest version of the git utilities active " +
				"on this copy of the repository. Would you like to set them up now?", GUILayout.MaxWidth(350.0f));
		});

		CustomEditorGUIUtility.VerticalSpacer(50.0f);

		CustomEditorGUIUtility.HorizontalLayout(() =>
		{
			Color startColor = GUI.color;
			GUI.color = Color.green;
			if (GUILayout.Button("Yes", GUILayout.MaxWidth(150.0f)))
			{
				CopyGitFiles();
				mCurrentState = EnsureProperName;
			}
			GUI.color = startColor;
		});
	}

	private void EnsureProperName()
	{
		CustomEditorGUIUtility.HorizontalLayout(() => { GUILayout.Label("Git Setup", mHeaderStyle); });

		CustomEditorGUIUtility.HorizontalLayout(() =>
		{
			mNameIndex = EditorGUILayout.Popup(mNameIndex, FiringSquad.POSSIBLE_NAMES, GUILayout.MaxWidth(200.0f));
		});

		CustomEditorGUIUtility.HorizontalLayout(() =>
		{
			mEmailIndex = EditorGUILayout.Popup(mEmailIndex, FiringSquad.POSSIBLE_EMAILS, GUILayout.MaxWidth(200.0f));
		});

		CustomEditorGUIUtility.VerticalSpacer(50);

		CustomEditorGUIUtility.HorizontalLayout(() =>
		{
			Color startColor = GUI.color;
			GUI.color = Color.green;
			if (GUILayout.Button("Confirm", GUILayout.MaxWidth(150.0f)))
			{
				DoGitProcess(string.Format("config --local user.name \"{0}\"", FiringSquad.POSSIBLE_NAMES[mNameIndex]));
				DoGitProcess(string.Format("config --local user.email {0}", FiringSquad.POSSIBLE_EMAILS[mEmailIndex]));
				Close();
			}
			GUI.color = startColor;
		});
	}

	private static void DoCheck()
	{
		string repoPath = Application.dataPath + "/..";
		var currentGitPaths = Directory.GetFiles(repoPath + "/.git/hooks/");
		var currentExePaths = Directory.GetFiles(repoPath + "/Tools/Executables/");

		kMissingFiles = new List<string>();

		foreach (string path in currentExePaths)
		{
			string fileName = Path.GetFileNameWithoutExtension(path);
			string gitFile = currentGitPaths.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x) == fileName);

			if (gitFile == default(string) || !FilesAreIdentical(path, gitFile))
				kMissingFiles.Add(path);
		}

		if (kMissingFiles.Count > 0)
		{
			GitHookInstaller window = GetWindow<GitHookInstaller>(true, "Firing Squad Setup", true);
			window.mCurrentState = window.InitialPopup;
		}
		else
		{
			string currentName = DoGitProcess("config --get user.name");
			int nameIndex = Array.IndexOf(FiringSquad.POSSIBLE_NAMES, currentName);
			string currentEmail = DoGitProcess("config --get user.email");
			int emailIndex = Array.IndexOf(FiringSquad.POSSIBLE_EMAILS, currentEmail);

			if (nameIndex < 0 || emailIndex < 0)
			{
				GitHookInstaller window = GetWindow<GitHookInstaller>();
				window.mNameIndex = nameIndex;
				window.mEmailIndex = emailIndex;
				window.mCurrentState = window.EnsureProperName;
			}
		}

		EditorApplication.update -= DoCheck;
	}

	private static bool FilesAreIdentical(string file1, string file2)
	{
		try
		{
			using (FileStream f1 = File.OpenRead(file1))
			using (FileStream f2 = File.OpenRead(file2))
			{
				var one = new byte[sizeof(long)];
				var two = new byte[sizeof(long)];

				int count = (int)Math.Ceiling((double)f1.Length / sizeof(long));
				for (int i = 0; i < count; i++)
				{
					f1.Read(one, 0, sizeof(long));
					f2.Read(two, 0, sizeof(long));

					if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0))
						return false;
				}
			}
		}
		catch (Exception e)
		{
			Debug.LogWarning("Error occured: " + e.Message + e.Data + e.StackTrace);
		}

		return true;
	}

	private static void CopyGitFiles()
	{
		string gitPath = Application.dataPath + "/../.git/hooks/";
		foreach (string sourcePath in kMissingFiles)
		{
			string fileName = Path.GetFileName(sourcePath);
			File.Copy(sourcePath, gitPath + fileName, true);
		}
	}

	private static string DoGitProcess(string args, int bufferSize = 2048)
	{
		using (Process p = new Process())
		{
			p.StartInfo.CreateNoWindow = true;
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.FileName = "git";
			p.StartInfo.Arguments = args;
			p.Start();

			var buffer = new char[bufferSize];
			p.StandardOutput.Read(buffer, 0, bufferSize);
			p.WaitForExit();

			return new string(buffer).TrimEnd('\n', '\0');
		}
	}
}
