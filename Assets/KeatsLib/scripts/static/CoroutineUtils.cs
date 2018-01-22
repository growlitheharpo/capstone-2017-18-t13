using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace KeatsLib.Unity
{
	/// <summary>
	/// A collection of useful coroutines.
	/// </summary>
	public static class Coroutines
	{
		/// <summary>
		/// The delegate type for Actions to call after a Coroutine has completed.
		/// </summary>
		public delegate void CallOnComplete();

		public delegate float PercentageEvaluator(float current, float total);

		public static readonly PercentageEvaluator LINEAR_EVALUATOR =
			(current, total) => current / total;

		public static readonly PercentageEvaluator MATHF_SMOOTHSTEP =
			(current, total) => Mathf.SmoothStep(0.0f, 1.0f, current / total);

		public static readonly PercentageEvaluator CHEAP_SMOOTH_EVALUATOR =
			(current, total) => -6.0f * (Mathf.Pow(current / total, 3.0f) / 3.0f - Mathf.Pow(current / total, 2.0f) / 2.0f);

		public static readonly PercentageEvaluator EXPENSIVE_SMOOTH_EVALUATOR =
			(current, total) => 0.5f * (Mathf.Pow(2.0f * (current / total), 1.0f / 3.0f) + 1.0f);

		/// <summary>
		/// Generic coroutine to lerp the position of an object in 3D.
		/// </summary>
		/// <param name="tRef">The transform to move.</param>
		/// <param name="target">The new target position.</param>
		/// <param name="time">Time to lerp over.</param>
		/// <param name="space">Whether to apply this transformation to the local or world position.</param>
		/// <param name="tScale">The method used to evaluate T as the lerp occurs. Defaults to linear.</param>
		/// <param name="callback">Function to call when this lerp has completed.</param>
		public static IEnumerator LerpPosition(
			Transform tRef, Vector3 target, float time, Space space = Space.Self, PercentageEvaluator tScale = null,
			CallOnComplete callback = null)
		{
			if (tScale == null)
				tScale = LINEAR_EVALUATOR;

			if (space == Space.Self)
				yield return LerpLocalPos(tRef, target, time, tScale);
			else
				yield return LerpWorldPos(tRef, target, time, tScale);

			if (callback != null)
				callback();
		}

		/// <summary>
		/// Private function for lerping a local position.
		/// </summary>
		private static IEnumerator LerpLocalPos(Transform obj, Vector3 p, float t, PercentageEvaluator tScale)
		{
			float currentTime = 0.0f;
			Vector3 startPos = obj.localPosition;

			while (currentTime < t)
			{
				obj.transform.localPosition = Vector3.Lerp(startPos, p, tScale(currentTime, t));

				currentTime += Time.deltaTime;
				yield return null;
			}

			obj.transform.localPosition = p;
		}

		/// <summary>
		/// Private function for lerping a world position.
		/// </summary>
		private static IEnumerator LerpWorldPos(Transform obj, Vector3 p, float t, PercentageEvaluator tScale)
		{
			float currentTime = 0.0f;
			Vector3 startPos = obj.position;

			while (currentTime < t)
			{
				obj.transform.position = Vector3.Lerp(startPos, p, tScale(currentTime, t));

				currentTime += Time.deltaTime;
				yield return null;
			}

			obj.transform.position = p;
		}

		/// <summary>
		/// Generic coroutine to lerp the rotation of an object in 3D.
		/// </summary>
		/// <param name="tRef">The transform to move.</param>
		/// <param name="target">The new target rotation.</param>
		/// <param name="time">Time to lerp over.</param>
		/// <param name="space">Whether to apply this transformation to the local or world rotation.</param>
		/// <param name="tScale">The method used to evaluate T as the lerp occurs. Defaults to linear.</param>
		/// <param name="callback">Function to call when this lerp has completed.</param>
		public static IEnumerator LerpRotation(
			Transform tRef, Quaternion target, float time, Space space = Space.Self, PercentageEvaluator tScale = null,
			CallOnComplete callback = null)
		{
			if (tScale == null)
				tScale = LINEAR_EVALUATOR;

			if (space == Space.Self)
				yield return LerpLocalRot(tRef, target, time, tScale);
			else
				yield return LerpWorldRot(tRef, target, time, tScale);

			if (callback != null)
				callback();
		}

		/// <summary>
		/// Private function for lerping a local rotation.
		/// </summary>
		private static IEnumerator LerpLocalRot(Transform t, Quaternion rot, float time, PercentageEvaluator tScale)
		{
			float currentTime = 0.0f;
			Quaternion startRot = t.localRotation;

			while (currentTime < time)
			{
				t.localRotation = Quaternion.Slerp(startRot, rot, tScale(currentTime, time));

				currentTime += Time.deltaTime;
				yield return new WaitForEndOfFrame();
			}

			t.localRotation = rot;
		}

		/// <summary>
		/// Private function for lerping a world rotation.
		/// </summary>
		private static IEnumerator LerpWorldRot(Transform t, Quaternion rot, float time, PercentageEvaluator tScale)
		{
			float currentTime = 0.0f;
			Quaternion startRot = t.rotation;

			while (currentTime < time)
			{
				t.rotation = Quaternion.Slerp(startRot, rot, tScale(currentTime, time));

				currentTime += Time.deltaTime;
				yield return new WaitForEndOfFrame();
			}

			t.rotation = rot;
		}

		/// <summary>
		/// Generic coroutine to scale the rotation of an object in 3D.
		/// </summary>
		/// <param name="tRef">The transform to move.</param>
		/// <param name="target">The new target position.</param>
		/// <param name="time">Time to lerp over.</param>
		/// <param name="tScale">The method used to evaluate T as the lerp occurs. Defaults to linear.</param>
		/// <param name="callback">Function to call when this lerp has completed.</param>
		public static IEnumerator LerpScale(
			Transform tRef, Vector3 target, float time, PercentageEvaluator tScale = null, CallOnComplete callback = null)
		{
			if (tScale == null)
				tScale = LINEAR_EVALUATOR;

			yield return LerpLocalScale(tRef, target, time, tScale);

			if (callback != null)
				callback();
		}

		/// <summary>
		/// Private fucntion for lerping a local rotation.
		/// </summary>
		private static IEnumerator LerpLocalScale(Transform obj, Vector3 scale, float t, PercentageEvaluator tScale)
		{
			float currentTime = 0.0f;
			Vector3 startPos = obj.localScale;

			while (currentTime < t)
			{
				obj.transform.localScale = Vector3.Lerp(startPos, scale, tScale(currentTime, t));

				currentTime += Time.deltaTime;
				yield return null;
			}

			obj.transform.localScale = scale;
		}

		/// <summary>
		/// Lerps the intensity of a light over the amount of time provided.
		/// </summary>
		/// <param name="light">The light to lerp.</param>
		/// <param name="newIntensity">The new target intensity value.</param>
		/// <param name="time">The amount of time to lerp over.</param>
		/// <param name="tScale">The method used to evaluate T as the lerp occurs. Defaults to linear.</param>
		/// <param name="callback">Function to call when this lerp has completed.</param>
		public static IEnumerator LerpLightIntensity(
			Light light, float newIntensity, float time, PercentageEvaluator tScale = null, CallOnComplete callback = null)
		{
			if (tScale == null)
				tScale = LINEAR_EVALUATOR;

			float elapsedTime = 0.0f, startingVal = light.intensity;

			while (elapsedTime < time)
			{
				light.intensity = Mathf.Lerp(startingVal, newIntensity, tScale(elapsedTime, time));

				elapsedTime += Time.deltaTime;
				yield return null;
			}

			light.intensity = newIntensity;

			if (callback != null)
				callback();
		}

		/// <summary>
		/// Lerp the color of a UI element over time.
		/// </summary>
		/// <param name="targetGraphic">The target UI element to effect.</param>
		/// <param name="color">The new color to lerp to.</param>
		/// <param name="time">The amount of time to lerp over.</param>
		/// <param name="tScale">The method used to evaluate T as the lerp occurs. Defaults to linear.</param>
		/// <param name="callback">Function to call when this lerp has completed.</param>
		/// <returns></returns>
		public static IEnumerator LerpUIColor(
			Graphic targetGraphic, Color color, float time, PercentageEvaluator tScale = null,
			CallOnComplete callback = null)
		{
			if (tScale == null)
				tScale = LINEAR_EVALUATOR;

			float elapsedTime = 0.0f;
			Color startingVal = targetGraphic.color;

			while (elapsedTime < time)
			{
				yield return null;
				targetGraphic.color = Color.Lerp(startingVal, color, tScale(elapsedTime, time));
				elapsedTime += Time.deltaTime;
			}

			targetGraphic.color = color;

			if (callback != null)
				callback();
		}

		/// <summary>
		/// Wait a certain number of frames, than activate the given callback.
		/// </summary>
		/// <param name="numberFrames">Number of frames to wait for.</param>
		/// <param name="callback">The action to invoke after the number of frames have passed.</param>
		public static IEnumerator InvokeAfterFrames(uint numberFrames, Action callback)
		{
			for (int i = 0; i < numberFrames; i++)
				yield return null;

			callback.Invoke();
		}

		/// <summary>
		/// Wait a certain number of seconds, then activate the given callback.
		/// </summary>
		/// <param name="seconds">Length of time in seconds to wait for.</param>
		/// <param name="callback">The action to invoke after the time has passed.</param>
		public static IEnumerator InvokeAfterSeconds(float seconds, Action callback)
		{
			yield return new WaitForSeconds(seconds);
			callback.Invoke();
		}

		/// <summary>
		/// Call a given function every tick and pass in the current time since started, until it returns false.
		/// </summary>
		/// <param name="callback">
		/// The function to invoke.
		/// The first parameter is the amount of time (in seconds) since the coroutine began.
		/// The function returns true if it should continue, and returns false if it should end.
		/// </param>
		public static IEnumerator InvokeEveryTick(Func<float, bool> callback)
		{
			float currentTime = 0.0f;
			while (callback(currentTime))
			{
				currentTime += Time.deltaTime;
				yield return null;
			}
		}

		/// <summary>
		/// Destroys a particle system after it has finished playing.
		/// TODO: If we update to Unity 2017.2, we won't need this!!!
		/// </summary>
		public static IEnumerator WaitAndDestroyParticleSystem(ParticleSystem ps, bool destroyGameObject = true)
		{
			yield return null;
			yield return null;
			yield return new WaitForParticles(ps);
			yield return null;

			Object.Destroy(destroyGameObject ? (Object)ps.gameObject : ps);
		}
	}
}
