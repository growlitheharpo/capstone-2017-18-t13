using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using FiringSquad.Data;
using UnityEngine.Audio;

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
		ModelImporter modelImporter = (ModelImporter) assetImporter;
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

		// Don't create extra materials for kit items
		if (folderPath.ToLower().Contains("kit"))
			return null;

		string trimmedFileName = new Regex("(?<=(mesh_))(.)+").Match(fileName).Value;
		string materialPath = folderPath + "/mat_" + trimmedFileName + ".mat";

		Shader correctShader = Shader.Find("StandardCustom");
		if (correctShader)
		{
			mat.shader = correctShader;
			FillTextureMaps(mat, folderPath, trimmedFileName);
		}

		AssetDatabase.CreateAsset(mat, materialPath);
		return mat;
	}

	private static void FillTextureMaps(Material mat, string folder, string filename)
	{
		Texture color = GetTexture(folder, filename, "color");
		Texture mrao = GetTexture(folder, filename, "mrao");
		Texture normal = GetTexture(folder, filename, "norm");

		mat.SetTexture("_MainTex", color);
		mat.SetTexture("_MetallicGlossMap", mrao);
		mat.SetTexture("_BumpMap", normal);
	}

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
		string folderPath = Path.GetDirectoryName(assetImporter.assetPath);
		string fileName = Path.GetFileNameWithoutExtension(assetImporter.assetPath) ?? "";
		string dataPath = folderPath + "/" + fileName + ".asset";

		AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>("Assets/Scenes/core/audio/MasterAudioMixer.mixer");

		AudioClipData data = AssetDatabase.LoadAssetAtPath<AudioClipData>(dataPath);
		if (data == null && !File.Exists(dataPath))
		{
			data = ScriptableObject.CreateInstance<AudioClipData>();
			AssetDatabase.CreateAsset(data, dataPath);
		}

		if (data != null)
			data.group = mixer.FindMatchingGroups("sfx")[0];

		AssetDatabase.SaveAssets();
	}
}
