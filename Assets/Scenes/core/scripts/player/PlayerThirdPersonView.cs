using System.Collections;
using FiringSquad.Data;
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
		private enum SpriteValue
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
		private SpriteValue mTargetValue, mDefaultValueLayer;

		private const float CRITICAL_HEALTH_THRESHOLD = 20.0f;

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

			mTargetValue = mDefaultValueLayer = SpriteValue.Neutral;

		}

		/// <summary>
		/// Unity's Update function
		/// </summary>
		private void Update()
		{
			// Just setting an int is cheap, so we can do it every frame.
			if (mTargetMaterial != null)
				mTargetMaterial.SetInt("_CurrentSprite", (int)mTargetValue);
		}

		/// <summary>
		/// Reflect that an enemy got a legendary part
		/// </summary>
		public void ReflectEnemyGotLegendaryPart()
		{
			StopAllCoroutines();
			StartCoroutine(ChangeFaceTemporarily(SpriteValue.OtherPlayerGotLegendary, 2.5f));
		}

		/// <summary>
		/// Reflect that this player picked up a legendary part
		/// </summary>
		public void ReflectGotLegendaryPart()
		{
			StopAllCoroutines();
			StartCoroutine(ChangeFaceTemporarily(SpriteValue.GotLegendary, 2.5f));
		}

		/// <summary>
		/// Reflect that the player has lost a teammate
		/// </summary>
		public void ReflectTeammateDied()
		{
			StopAllCoroutines();
			StartCoroutine(ChangeFaceTemporarily(SpriteValue.TeammateKilled, 2.5f));
		}

		/// <summary>
		/// Reflect that the player has taken damage with the "ouch" face.
		/// </summary>
		public void ReflectTookDamage(float newHealth)
		{
			StopAllCoroutines();

			if (newHealth < CRITICAL_HEALTH_THRESHOLD)
				mTargetValue = mDefaultValueLayer = SpriteValue.CriticalHealth;
			else
				StartCoroutine(ChangeFaceTemporarily(SpriteValue.Wounded, 0.75f));
		}

		/// <summary>
		/// Reflect that the player just got a kill with the appropriate face.
		/// </summary>
		/// <param name="killInfo">The data provided by the server about the kill.</param>
		public void ReflectGotKill(PlayerKill killInfo)
		{
			StopAllCoroutines();

			// Check if it was a kill streak
			if ((killInfo.mFlags & KillFlags.Killstreak) != KillFlags.None)
				StartCoroutine(ChangeFaceTemporarily(SpriteValue.KillingSpree, 2.5f));
			else if ((killInfo.mFlags & KillFlags.Multikill) != KillFlags.None)
				StartCoroutine(ChangeFaceTemporarily(SpriteValue.Dominated, 2.5f));
			else
				StartCoroutine(ChangeFaceTemporarily(SpriteValue.GotKill, 2.5f));
		}

		/// <summary>
		/// Update the player's health amount, which changes their default face.
		/// </summary>
		/// <param name="newHealth"></param>
		public void UpdateHealthAmount(float newHealth)
		{
			SpriteValue newVal = newHealth < CRITICAL_HEALTH_THRESHOLD ? SpriteValue.CriticalHealth : SpriteValue.Neutral;

			if (mTargetValue == mDefaultValueLayer)
				mTargetValue = mDefaultValueLayer = newVal;
			else
				mDefaultValueLayer = newVal;
		}

		/// <summary>
		/// Update the face to a value and then change back to Neutral
		/// </summary>
		private IEnumerator ChangeFaceTemporarily(SpriteValue newFace, float f)
		{
			mTargetValue = newFace;
			yield return new WaitForSeconds(f);
			mTargetValue = mDefaultValueLayer;
		}
	}
}
