using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor
{
	public class CustomColormaskMaterialEditor : ShaderGUI
	{
		private static class Labels
		{
			public static readonly GUIContent ALBEDO_TEXT = new GUIContent("Albedo", "Albedo (RGB) and Transparency (A)");
			public static readonly GUIContent COLOR_MASK_MAP_TEXT = new GUIContent("Color Mask Map", "Mask (Greyscale)");
			public static readonly GUIContent METALLIC_MAP_TEXT = new GUIContent("Metallic, Smoothness", "Metallic (R) and Smoothness (A)");
			public static readonly GUIContent NORMAL_MAP_TEXT = new GUIContent("Normal Map", "Normal Map");
			public static readonly GUIContent EMISSION_MASK_TEXT = new GUIContent("Emissive Mask", "Emission (RGB)");
		}

		private MaterialProperty mAlbedoMapProp;
		private MaterialProperty mAlbedoColorProp;
		private MaterialProperty mColorMaskMapProp;
		private MaterialProperty mColorMaskColorProp;
		private MaterialProperty mMetallicSmoothnessProp;
		private MaterialProperty mSmoothnessScaleProp;
		private MaterialProperty mNormalMapProp;
		private MaterialProperty mEmissiveMapProp;
		private MaterialProperty mEmissiveColorProp;

		private readonly ColorPickerHDRConfig mColorPickerHdrConfig = new ColorPickerHDRConfig(0f, 99f, 1 / 99f, 3f);
		private MaterialEditor mMaterialEditor;

		private void FindProperties(MaterialProperty[] properties)
		{
			mAlbedoMapProp = FindProperty("_MainTex", properties);
			mAlbedoColorProp = FindProperty("_Color", properties);
			mColorMaskMapProp = FindProperty("_ColorMask", properties);
			mColorMaskColorProp = FindProperty("_ColorMaskColor", properties);
			mMetallicSmoothnessProp = FindProperty("_MetallicGlossMap", properties);
			mSmoothnessScaleProp = FindProperty("_GlossMapScale", properties);
			mNormalMapProp = FindProperty("_BumpMap", properties);
			mEmissiveMapProp = FindProperty("_EmissionMap", properties);
			mEmissiveColorProp = FindProperty("_EmissionColor", properties);
		}

		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
		{
			FindProperties(properties);
			mMaterialEditor = materialEditor;

			DrawProperties();
		}

		private void DrawProperties()
		{
			EditorGUIUtility.labelWidth = 0.0f;

			GUILayout.Label("Texture Maps", EditorStyles.boldLabel);

			GUILayout.Label("Albedo/Color");
			mMaterialEditor.TexturePropertySingleLine(Labels.ALBEDO_TEXT, mAlbedoMapProp, mAlbedoColorProp);
			mMaterialEditor.TexturePropertySingleLine(Labels.COLOR_MASK_MAP_TEXT, mColorMaskMapProp, mColorMaskColorProp);

			GUILayout.Space(30.0f);
			mMaterialEditor.TexturePropertySingleLine(Labels.METALLIC_MAP_TEXT, mMetallicSmoothnessProp);
			mMaterialEditor.ShaderProperty(mSmoothnessScaleProp, "Smoothness Scale", 3);
			mMaterialEditor.TexturePropertySingleLine(Labels.NORMAL_MAP_TEXT, mNormalMapProp);

			GUILayout.Space(30.0f);
			DoEmissionArea();
		}

		private void DoEmissionArea()
		{
			if (!mMaterialEditor.EmissionEnabledProperty())
				return;

			bool hadTexture = mEmissiveMapProp.textureValue != null;
			mMaterialEditor.TexturePropertyWithHDRColor(Labels.EMISSION_MASK_TEXT, mEmissiveMapProp, mEmissiveColorProp, mColorPickerHdrConfig, false);

			float brightness = mEmissiveColorProp.colorValue.maxColorComponent;
			if (mEmissiveMapProp.textureValue != null && !hadTexture && brightness <= 0f)
				mEmissiveColorProp.colorValue = Color.white;
		}
	}
}
