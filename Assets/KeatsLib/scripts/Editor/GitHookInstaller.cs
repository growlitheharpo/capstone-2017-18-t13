using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UnityEditor
{
	/// <summary>
	/// Pipeline class used to install our custom git hooks
	/// the first time Unity is launched.
	/// </summary>
	[InitializeOnLoad]
	public class GitHookInstaller : EditorWindow
	{
		private delegate void GUIState();

		/// <summary>
		/// static data holder.
		/// </summary>
		public static class FiringSquad
		{
			public static readonly string[] POSSIBLE_NAMES =
			{
				"Charles Carucci",
				"James Keats",
				"Max Sanel",
				"Tyler Bolster",
				"Justin Mulkin",
				"Natalie Frost",
				"Timothy Eccleston",
				"Michael Manfredi",
			};

			public static readonly string[] POSSIBLE_EMAILS =
			{
				"charles.carucci@mymail.champlain.edu",
				"james.keats@mymail.champlain.edu",
				"max.sanel@mymail.champlain.edu",
				"tyler.bolster@mymail.champlain.edu",
				"justin.mulkin@mymail.champlain.edu",
				"natalie.frost@mymail.champlain.edu",
				"timothy.eccleston@mymail.champlain.edu",
				"michael.manfredi@mymail.champlain.edu",
			};
		}

		private static readonly bool DO_ONCE;
		private static List<string> kMissingFiles;
		private GUIState mCurrentState;

		private GUIStyle mHeaderStyle;
		private int mNameEmailIndex;

		/// <summary>
		/// Static constructor. Ensures we only perform the check once.
		/// </summary>
		static GitHookInstaller()
		{
			if (DO_ONCE)
				return;

			DO_ONCE = true;
			EditorApplication.update += DoCheck;
		}

		/// <summary>
		/// Forces the git information to update now.
		/// </summary>
		[MenuItem("Pipeline/Update Git Information")]
		public static void ForceInitialize()
		{
			EditorApplication.update += DoCheck;
		}

		/// <summary>
		/// Checks to see if this local copy of the repository has the hooks installed.
		/// It also checks to make sure the user name and email are valid.
		/// </summary>
		private static void DoCheck()
		{
			string repoPath = Application.dataPath + "/..";

			EnsureGitHooksExists(repoPath + "/.git/hooks/");

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
				try
				{
					string currentName = GitProcess.Launch("config --get user.name");
					int nameIndex = Array.IndexOf(FiringSquad.POSSIBLE_NAMES, currentName);
					string currentEmail = GitProcess.Launch("config --get user.email");
					int emailIndex = Array.IndexOf(FiringSquad.POSSIBLE_EMAILS, currentEmail);

					if (nameIndex < 0 || emailIndex < 0)
					{
						GitHookInstaller window = GetWindow<GitHookInstaller>();
						window.mNameEmailIndex = nameIndex;
						window.mCurrentState = window.EnsureProperName;
					}
				}
				catch (Exception)
				{
					Debug.LogWarning("Unable to automatically set user's name and email because command-line git is not installed.");
					Debug.LogWarning("Please make sure your name and email are set properly before committing.");
				}
			}

			// Remove this function from the update list.
			EditorApplication.update -= DoCheck;
		}

		private static void EnsureGitHooksExists(string s)
		{
			if (!Directory.Exists(s))
				Directory.CreateDirectory(s);
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

		/// <summary>
		/// The initial prompt to copy the git hooks to the local folder.
		/// </summary>
		private void InitialPopup()
		{
			CustomEditorGUIUtility.HorizontalLayout(() => { GUILayout.Label("Git Setup", mHeaderStyle); });
			CustomEditorGUIUtility.HorizontalLayout(() =>
			{
				GUIStyle style = new GUIStyle(GUI.skin.box) { normal = { textColor = GUI.skin.label.normal.textColor } };

				GUILayout.Box(
					"It appears you do not have the latest version of the git utilities active " +
					"on this copy of the repository. Would you like to set them up now?", style, GUILayout.MaxWidth(350.0f));
			});

			CustomEditorGUIUtility.VerticalSpacer(50.0f);

			CustomEditorGUIUtility.HorizontalLayout(() =>
			{
				CustomEditorGUIUtility.DrawAsColor(Color.green, () =>
				{
					if (GUILayout.Button("Yes", GUILayout.MaxWidth(150.0f)))
					{
						CopyGitFiles();
						mCurrentState = EnsureProperName;
					}
				});
			});
		}

		/// <summary>
		/// The second popup to make sure the user.name and user.email are correct.
		/// </summary>
		private void EnsureProperName()
		{
			CustomEditorGUIUtility.HorizontalLayout(() => { GUILayout.Label("Git Setup", mHeaderStyle); });
			CustomEditorGUIUtility.HorizontalLayout(() =>
			{
				mNameEmailIndex = EditorGUILayout.Popup(mNameEmailIndex, FiringSquad.POSSIBLE_NAMES, GUILayout.MaxWidth(200.0f));
			});
			CustomEditorGUIUtility.HorizontalLayout(() =>
			{
				mNameEmailIndex = EditorGUILayout.Popup(mNameEmailIndex, FiringSquad.POSSIBLE_EMAILS, GUILayout.MaxWidth(200.0f));
			});

			CustomEditorGUIUtility.VerticalSpacer(50);

			CustomEditorGUIUtility.HorizontalLayout(() =>
			{
				CustomEditorGUIUtility.DrawAsColor(Color.green, () =>
				{
					if (GUILayout.Button("Confirm", GUILayout.MaxWidth(150.0f)))
					{
						GitProcess.Launch(string.Format("config --local user.name \"{0}\"", FiringSquad.POSSIBLE_NAMES[mNameEmailIndex]));
						GitProcess.Launch(string.Format("config --local user.email {0}", FiringSquad.POSSIBLE_EMAILS[mNameEmailIndex]));
						Close();
					}
				});
			});
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
	}
}
