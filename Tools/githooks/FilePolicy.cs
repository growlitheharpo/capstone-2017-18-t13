using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace precommit
{
	internal static class FilePolicy
	{
		private enum GitStatus
		{
			All,
			Staged,
			Added
		}

		private enum Folder
		{
			All,
			Assets,
			AssetsScenes
		}

		private const string START = "^";
		private const string END = "$";

		private const string LC_WORD = "([a-z]+)";
		private const string UC_WORD = "([A-Z][a-z]+)";
		private const string OPT_NUM_TWO = "(\\d{0}|\\d{2})";
		private const string OPT_DIMENSION = "((\\d{1,3}(\\.\\d)?)(x(\\d{1,3}(\\.\\d)?)){1,2})";
		private const string ANIM = "(@[a-z]+)";
		private const string OPT_ANIM = ANIM + "?";
		private const string AUD_TYPE = "((sfx)|(mus))_";
		private const string TEX_TYPE = "((color_)|(rmao_)|(norm_)|(emiss_)|(aorm_))";

		private const string REGULAR_NAME = LC_WORD + UC_WORD + "*" + "(" + OPT_NUM_TWO + ")|(" + OPT_DIMENSION + ")";
		private const string SUFFIX = "(_" + REGULAR_NAME + ")?";

		private static readonly Dictionary<string, Regex> FILETYPE_CONVENTIONS = new Dictionary<string, Regex>
		{
			{ ".prefab",	new Regex(START + "(p_)" + REGULAR_NAME + SUFFIX + "(.prefab)(.meta)?" + END) },
			{ ".mat",		new Regex(START + "(mat_)" + REGULAR_NAME + SUFFIX + "(.mat)(.meta)?" + END) },
			{ ".png",		new Regex(START + "(tex_)" + TEX_TYPE + "?" + REGULAR_NAME + SUFFIX + "(.png)(.meta)?" + END) },
			{ ".jpg",		new Regex(START + "(tex_)" + TEX_TYPE + "?" + REGULAR_NAME + SUFFIX + "(.jpg)(.meta)?" + END) },
			{ ".jpeg",		new Regex(START + "(tex_)" + TEX_TYPE + "?" + REGULAR_NAME + SUFFIX + "(.jpeg)(.meta)?" + END) },
			{ ".tga",		new Regex(START + "(tex_)" + TEX_TYPE + REGULAR_NAME + SUFFIX + "(.tga)(.meta)?" + END) },
			{ ".wav",		new Regex(START + "(aud_)" + AUD_TYPE + REGULAR_NAME + SUFFIX + "(.wav)(.meta)?" + END) },
			{ ".mp3",		new Regex(START + "(aud_)" + AUD_TYPE + REGULAR_NAME + SUFFIX + "(.mp3)(.meta)?" + END) },
			{ ".fbx",		new Regex(START + "(mesh_)" + REGULAR_NAME + SUFFIX + OPT_ANIM + "(.fbx)(.meta)?" + END) }
		};

		private static readonly Dictionary<string, string> FILETYPE_SUGGESTIONS = new Dictionary<string, string>
		{
			{ ".prefab",	"p_prefabName[00][_piece00].prefab" },
			{ ".wav",		"aud_(sfx|mus)_associatedName[00][_piece00].wav" },
			{ ".mp3",		"aud_(sfx|mus)_associatedName[00][_piece00].mp3" },
			{ ".mat",		"mat_associatedObjName[00][_piece00].mat" },
			{ ".png",		"tex_[map_]associatedObjName[00][_piece00].png" },
			{ ".jpg",       "tex_[map_]associatedObjName[00][_piece00].jpg" },
			{ ".jpeg",      "tex_[map_]associatedObjName[00][_piece00].jpg" },
			{ ".tga",       "tex_(map)_associatedObjName[00][_piece00].tga" },
			{ ".fbx",		"mesh_descriptiveName[00][_piece00][@animation].fbx" }
		};

		private static IEnumerable<string> StripFileStatus(IEnumerable<string> files, GitStatus s, Folder f, bool stripRenames = true)
		{
			return files
				.Where(x =>
				{
					switch (s)
					{
						case GitStatus.Staged:
							return x[0] != ' ';
						case GitStatus.Added:
							return x[0] == 'A';
						default:
							return true;
					}
				})
				.Select(x => x.Remove(0, 3))
				.Where(x =>
				{
					switch (f)
					{
						case Folder.Assets:
							return x.StartsWith("Assets/");
						case Folder.AssetsScenes:
							return x.StartsWith("Assets/Scenes/");
						default:
							return true;
					}
				})
				.Select(x => stripRenames && x.Contains(" -> ") ? x.Substring(x.IndexOf(" -> ", StringComparison.Ordinal) + 4) : x);
		}

		public static async Task<PrehookResult> RejectProjectVersionTxt(string[] files)
		{
			using (var task = Task.Run(() =>
			{
				string fileExists = files.FirstOrDefault(x => x.EndsWith("ProjectVersion.txt"));
				if (!string.IsNullOrEmpty(fileExists) && fileExists[0] != ' ')
				{
					return new PrehookResult(false, "It is forbidden to modify ProjectVersion.txt!\n" +
													"Please unstage/uncheck this file before committing.");
				}

				return new PrehookResult(true);
			}))
			{
				await task;
				return task.Result;
			}
		}

		public static async Task<PrehookResult> EnforceAssetFileNamingPatterns(string[] files)
		{
			using (var task = Task.Run(() =>
			{
				var failures = new List<string>();
				var stagedFiles = StripFileStatus(files, GitStatus.Staged, Folder.Assets);

				foreach (string file in stagedFiles)
				{
					if (file.Contains(' '))
					{
						failures.Add("\t" + file + "\n\t Please do not have any spaces in file paths or names.");
						continue;
					}

					string filetype = GetFiletype(file);
					string fileName = file.Split('/').Last();

					if (!FILETYPE_CONVENTIONS.ContainsKey(filetype))
						continue;
					if (!FILETYPE_CONVENTIONS[filetype].IsMatch(fileName))
						failures.Add("\t" + file + "\n\t Please use this format: " + FILETYPE_SUGGESTIONS[filetype]);
				}

				return failures.Count > 0
					? new PrehookResult(false, "The following files conflict with our naming conventions:\n" + string.Join("\n\n", failures))
					: new PrehookResult(true);
			}))
			{
				await task;
				return task.Result;
			}
		}

		public static async Task<PrehookResult> EnsureNewFilesHaveMeta(string[] files)
		{
			string rootFolder = await GitAsyncUtil.Git("rev-parse --show-toplevel");
			using (var task = Task.Run(() =>
			{
				IEnumerable<string> addedAssetFiles = StripFileStatus(files, GitStatus.Added, Folder.Assets).ToArray();

				var newAssets = addedAssetFiles
					.Where(x => !x.EndsWith(".meta"))
					.OrderBy(x => x.ToLower())
					.ToArray();

				var newMetas = addedAssetFiles
					.Where(x => x.EndsWith(".meta"))
					.Where(x => !Directory.Exists(rootFolder + "/" + x.Substring(0, x.Length - ".meta".Length)))
					.OrderBy(x => x.ToLower())
					.ToArray();

				var failures = new List<string>();
				failures.AddRange(newAssets.Where(t => !newMetas.Contains(t + ".meta")));
				failures.AddRange(newMetas.Where(t => !newAssets.Contains(t.Substring(0, t.Length - ".meta".Length))));

				if (failures.Count > 0)
				{
					return new PrehookResult(
						false,
						"The following files are either .meta files with no matching\n asset, or assets with no matching meta file:\n"
						+ string.Join("\n\n", failures.Select((t, i) => "\t" + (i + 1) + ". " + t)));
				}

				return new PrehookResult(true);
			}))
			{
				await task;
				return task.Result;
			}
		}

		public static async Task<PrehookResult> RejectDefaultMaterialFiles(string[] files)
		{
			using (var task = Task.Run(() =>
			{
				var addedAssetFiles = StripFileStatus(files, GitStatus.Added, Folder.Assets);

				var failures = addedAssetFiles
					.Where(x => x.ToLower().Contains("lambert"))
					.Select((x, i) => "\t" + (i + 1) + ". " + x)
					.ToArray();

				if (failures.Length > 0)
				{
					return new PrehookResult(false,
						"It is forbidden to commit default Unity materials:\n"
						+ string.Join("\n\n", failures));
				}

				return new PrehookResult(true);
			}))
			{
				await task;
				return task.Result;
			}
		}

		public static async Task<PrehookResult> RejectCapitalizedFolders(string[] files)
		{
			using (var task = Task.Run(() =>
			{
				var stagedFiles = StripFileStatus(files, GitStatus.Staged, Folder.AssetsScenes)
					.Select(x => x.Remove(0, "Assets/Scenes/".Length))
					.Select(x => string.Join("/", x.Split('/').Reverse().Skip(1).Reverse()));

				var failures = new List<string>();
				foreach (string path in stagedFiles)
				{
					if (path.ToLower() == path)
						continue;

					string lowercaseEditor = string.Join("/", path.Split('/')
						.Select(x => x == "Editor" ? "editor" : x));

					if (path.ToLower() == lowercaseEditor)
						continue;

					failures.Add("\t" + (failures.Count + 1) + ". " + path);
				}

				return failures.Count > 0
					? new PrehookResult(false, "It is forbidden to use capital letters in subfolder names:\n" + string.Join("\n", failures))
					: new PrehookResult(true);
			}))
			{
				await task;
				return task.Result;
			}
		}

		private static string GetFiletype(string file, bool getMetaRoot = true)
		{
			var pieces = file.Split('.');

			if (getMetaRoot && pieces[pieces.Length - 1] == "meta")
				return "." + pieces[pieces.Length - 2];

			return "." + pieces[pieces.Length - 1];
		}
	}
}
