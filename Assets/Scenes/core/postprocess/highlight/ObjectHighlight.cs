using System.Collections.Generic;
using FiringSquad.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityStandardAssets.ImageEffects;

namespace FiringSquad.Gameplay
{
	/*
	* Many, many, many thanks to the author of this blog: http://xroft666.blogspot.com/2015/07/glow-highlighting-in-unity.html
	*/
	/// <summary>
	/// Manages highlights of objects. List of objects is global and can be added to or removed from by anyone.
	/// </summary>
	public class ObjectHighlight : MonoSingleton<ObjectHighlight>
	{
		private enum ShaderPasses
		{
			GlowPass = 0,
			OcclusionPass = 1,
			DepthFilterPass = 2,
		}

		[SerializeField] private Color mColor;

		private List<Renderer> mOcclusionRenderers;
		private List<Renderer> mOutlineRenderers;

		private CommandBuffer mCommand;
		private BlurOptimized mBlur;
		private Material mMaterial;

		/// <inheritdoc />
		protected override void Awake()
		{
			base.Awake();
			mCommand = new CommandBuffer();
			mMaterial = new Material(Shader.Find("Hidden/Custom/Highlight"));

			mOutlineRenderers = new List<Renderer>();
			mOcclusionRenderers = new List<Renderer>();

			mBlur = GetComponent<BlurOptimized>() ?? FindObjectOfType<BlurOptimized>() ?? AddBlurComponent();
		}

		/// <summary>
		/// Add a Blur component to the camera if one does not already exist.
		/// </summary>
		private BlurOptimized AddBlurComponent()
		{
			mBlur = gameObject.AddComponent<BlurOptimized>();
			mBlur.blurShader = Shader.Find("Hidden/FastBlur");
			mBlur.enabled = false;

			return mBlur;
		}

		/// <summary>
		/// Unity's OnRenderImage function.
		/// Do some graphics operations related to our outline shader and add it.
		/// </summary>
		private void OnRenderImage(RenderTexture src, RenderTexture dest)
		{
			// Grab temporary render textures for the effect
			RenderTexture highlightTex = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.R8),
						blurredTex = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.R8),
						occludedTex = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.R8);

			mCommand.Clear();
			ClearTexture(highlightTex);

			// Render the highlights, blur it in another buffer, then add the occluders into the first.
			RenderObjects(mOutlineRenderers, highlightTex);
			mBlur.OnRenderImage(highlightTex, blurredTex);
			RenderObjects(mOcclusionRenderers, highlightTex);

			// Blit the occluderTex onto the blurredTex (subtract occlusion from the blurred outlines, essentially)
			mMaterial.SetTexture("_OccludeMap", highlightTex);
			Graphics.Blit(blurredTex, occludedTex, mMaterial, (int)ShaderPasses.OcclusionPass);

			// Finally, blit the outline onto the main texture
			mMaterial.SetTexture("_OccludeMap", occludedTex);
			mMaterial.SetColor("_Color", mColor);
			Graphics.Blit(src, dest, mMaterial, (int)ShaderPasses.GlowPass);

			// Release all our temporary rendertextures
			RenderTexture.ReleaseTemporary(occludedTex);
			RenderTexture.ReleaseTemporary(blurredTex);
			RenderTexture.ReleaseTemporary(highlightTex);
		}

		/// <summary>
		/// Clear a provided texture to Color.clear.
		/// </summary>
		private static void ClearTexture(RenderTexture tex)
		{
			RenderTexture.active = tex;
			GL.Clear(true, true, Color.clear);
			RenderTexture.active = null;
		}

		/// <summary>
		/// Render a list of objects onto the provided texture using the DepthFilterPass.
		/// </summary>
		private void RenderObjects(IList<Renderer> renderers, RenderTexture tex)
		{
			if (renderers == null || renderers.Count <= 0)
				return;

			RenderTargetIdentifier id = new RenderTargetIdentifier(tex);
			mCommand.SetRenderTarget(id);

			// ReSharper disable once ForCanBeConvertedToForeach
			for (int i = 0; i < renderers.Count; ++i)
			{
				if (renderers[i] == null)
				{
					renderers.RemoveAt(i);
					--i;
				}
				else if (renderers[i].enabled && renderers[i].gameObject.activeInHierarchy)
					mCommand.DrawRenderer(renderers[i], mMaterial, 0, (int)ShaderPasses.DepthFilterPass);
			}

			RenderTexture.active = tex;
			Graphics.ExecuteCommandBuffer(mCommand);
			RenderTexture.active = null;
		}

		/// <summary>
		/// Adds a renderer to the highlight list.
		/// </summary>
		/// <param name="r">A reference to the renderer.</param>
		public void AddRendererToHighlightList(Renderer r)
		{
			if (r != null)
				mOutlineRenderers.Add(r);
		}

		/// <summary>
		/// Remove a renderer from the highlight list.
		/// </summary>
		/// <returns>True if the item was found and removed, false otherwise.</returns>
		public bool RemoveRendererFromHighlightList(Renderer r)
		{
			return mOutlineRenderers.Remove(r);
		}

		/// <summary>
		/// Adds a renderer to the list of renderers that highlights cannot appear through.
		/// </summary>
		/// <param name="r">The renderer to add.</param>
		public void AddOccluder(Renderer r)
		{
			if (r != null)
				mOcclusionRenderers.Add(r);
		}

		/// <summary>
		/// Remove a renderer from the list of renderers that highlights cannot appear through.
		/// </summary>
		/// <returns>True if the item was found and removed, false otherwise.</returns>
		public bool RemoveOccluder(Renderer r)
		{
			return mOcclusionRenderers.Remove(r);
		}
	}
}
