using UnityEngine;

namespace FiringSquad.Gameplay
{
	/// <summary>
	/// Utility class to update the _ColorMaskColor value on an attached
	/// renderer.
	/// </summary>
	public class ColormaskUpdateUtility : MonoBehaviour
	{
		[SerializeField] private bool mAlsoChangeEmissive = true;

		/// Private variables
		private Material[] mMaterials;
		private Shader mCorrectShader;

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		private void Awake()
		{
			TryGrabMaterials();
			mCorrectShader = Shader.Find("Custom/StandardCustomColormask");
		}

		/// <summary>
		/// Attempt to grab a reference to the material(s) we should update
		/// </summary>
		private void TryGrabMaterials()
		{
			Renderer attachedRenderer = GetComponent<Renderer>();
			if (attachedRenderer == null)
			{
				mMaterials = null;
				return;
			}

			mMaterials = attachedRenderer.materials;
		}

		/// <summary>
		/// Immediately reflect a new color.
		/// </summary>
		/// <param name="c">The color to display.</param>
		public void UpdateDisplayedColor(Color c)
		{
			if (mMaterials == null)
				TryGrabMaterials();
			if (mMaterials == null)
				return;

			Color emissiveColor = GetEmissiveColor(c);

			foreach (Material m in mMaterials)
			{
				if (m.shader != mCorrectShader && !m.HasProperty("_ColorMaskColor"))
					continue;

				m.SetColor("_ColorMaskColor", c);

				if (mAlsoChangeEmissive)
					m.SetColor("_EmissionColor", emissiveColor);
			}
		}

		private Color GetEmissiveColor(Color color)
		{
			if (!mAlsoChangeEmissive)
				return color;

			float h, s, v;
			Color.RGBToHSV(color, out h, out s, out v);
			s = s * 0.5f;
			return Color.HSVToRGB(h, s, v);
		}
	}
}
