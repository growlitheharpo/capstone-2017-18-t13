using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	/// <summary>
	/// Local script that handles the third person view of each character.
	/// </summary>
	public class PlayerThirdPersonView : MonoBehaviour
	{
		/// <summary>
		/// The "CurrentSprite" indexes for each sprite.
		/// </summary>
		private enum FaceValues
		{
			Neutral = 0,
			TeammateKilled = 1,
			Dominated = 2,
			GotLegendary = 3,
			OtherPlayerGotLegendary = 4,
			KillingSpree = 5,
			GotKill = 6,
			Wounded = 7,
			CriticalHealth = 8,
		}

		/// Inspector variables
		[SerializeField] private Renderer mTargetRenderer;

		/// Private variables
		private Material mTargetMaterial;

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		private void Awake()
		{
			// Find the correct material on our target.
			Shader targetShader = Shader.Find("Unlit/Unlit Spritesheet");
			foreach (Material m in mTargetRenderer.materials)
			{
				if (m.shader != targetShader && !m.HasProperty("_CurrentSprite"))
					continue;

				mTargetMaterial = m;
				break;
			}
		}
	}
}
