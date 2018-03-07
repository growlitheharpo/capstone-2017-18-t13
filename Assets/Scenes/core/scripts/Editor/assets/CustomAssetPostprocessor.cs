using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

namespace UnityEditor
{
	/// <summary>
	/// Catch-All post processor for the importing of any asset type.
	/// </summary>
	public class CustomAssetPostprocessor : AssetPostprocessor
	{
		/// <summary>
		/// Pre-process Model
		/// Disable Unity's material importing (it doesn't work).
		/// </summary>
		private void OnPreprocessModel()
		{
			ModelImporter modelImporter = (ModelImporter)assetImporter;
			modelImporter.importMaterials = false;
		}

		/// <summary>
		/// Post process Model
		/// Creates a material with the shader that we are using instead of the default.
		/// Using our naming convention, try to find the correct textures to fill
		/// all the channels of the material, then save it.
		/// </summary>
		private Material OnAssignMaterialModel(Material mat, Renderer render)
		{
			string folderPath = Path.GetDirectoryName(assetImporter.assetPath) ?? "";
			string fileName = Path.GetFileNameWithoutExtension(assetImporter.assetPath) ?? "";

			string trimmedFileName = new Regex("(?<=(mesh_))(.)+").Match(fileName).Value;
			string materialPath = folderPath + "/mat_" + trimmedFileName + ".mat";

			// Don't create extra materials for kit items
			if (folderPath.ToLower().Contains("kit") || File.Exists(materialPath) || assetImporter.assetPath.Contains("@"))
				return null;

			Shader correctShader = Shader.Find("Standard");
			if (correctShader)
			{
				mat.shader = correctShader;
				FillTextureMaps(mat, folderPath, trimmedFileName);
			}

			AssetDatabase.CreateAsset(mat, materialPath);
			return mat;
		}

		/// <summary>
		/// Fill the texture maps for an imported material based on found files.
		/// </summary>
		/// <param name="mat">The material to update.</param>
		/// <param name="folder">The folder we're located in.</param>
		/// <param name="filename">The filename of the model we're importing.</param>
		private static void FillTextureMaps(Material mat, string folder, string filename)
		{
			Texture color = GetTexture(folder, filename, "color");
			Texture aorm = GetTexture(folder, filename, "aorm");
			Texture normal = GetTexture(folder, filename, "norm") ?? GetTexture(folder, filename, "normal");

			mat.SetTexture("_MainTex", color);
			if (mat.HasProperty("_Color"))
				mat.SetColor("_Color", Color.white);

			mat.SetTexture("_MetallicGlossMap", aorm);
			mat.SetTexture("_BumpMap", normal);
		}

		/// <summary>
		/// Find the texture file based on the file name and folder.
		/// </summary>
		/// <param name="folder">The folder where we're located.</param>
		/// <param name="file">The name of the file that we're importing.</param>
		/// <param name="type">The type of the texture map to search fori.</param>
		/// <returns></returns>
		private static Texture GetTexture(string folder, string file, string type)
		{
			string pathTarga = folder + "/tex_" + type + "_" + file + ".tga";
			string pathPng = folder + "/tex_" + type + "_" + file + ".png";

			string guid = AssetDatabase.AssetPathToGUID(pathTarga);
			if (guid == "")
				guid = AssetDatabase.AssetPathToGUID(pathPng);

			return AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath(guid));
		}

		/// <summary>
		/// Post process audio clip files.
		/// Creates an IAudioData associated with that clip automatically.
		/// </summary>
		private void OnPostprocessAudio(AudioClip clip)
		{
			Debug.LogWarning("It appears you are importing an audio file. Please place ALL audio in the FMOD project, not directly in Unity.", clip);
			AssetDatabase.SaveAssets();
		}
	}
}
