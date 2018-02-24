using System.Collections;
using FiringSquad.Gameplay.UI;
using KeatsLib.Collections;
using UnityEngine;

namespace FiringSquad.Gameplay.Weapons
{
	/// <summary>
	/// Utiltiy component that binds the emissive color of the target mesh renderer to a provided float.
	/// </summary>
	public class BoundFloatEmissiveField : BoundUIElement<float>
	{
		/// Inspector variables
		[SerializeField] private string mEmissiveProperty = "_EmissionColor";
		[SerializeField] private Color mStartColor = Color.white;
		[SerializeField] private Color mEndColor = Color.white;
		[SerializeField] private float mBlinkingThreshold = 0.15f;
		[SerializeField] private float mBlinkingRate = 0.15f;

		/// Private variables
		private Material[] mMaterialInstances;
		private Coroutine mBlinkRoutine;

		/// <inheritdoc />
		protected override void Start()
		{
			mMaterialInstances = GetComponent<MeshRenderer>().materials;
			base.Start();
		}

		/// <inheritdoc />
		protected override void HandlePropertyChanged()
		{
			if (property.value >= mBlinkingThreshold)
			{
				if (mBlinkRoutine != null)
					StopCoroutine(mBlinkRoutine);

				float percent = property.value.Rescale(mBlinkingThreshold, 1.0f);
				Color val = Color.Lerp(mEndColor, mStartColor, percent);

				foreach (Material m in mMaterialInstances)
					m.SetColor(mEmissiveProperty, val);
			}
			else if (mBlinkRoutine == null)
				mBlinkRoutine = StartCoroutine(BlinkValue());
		}

		private IEnumerator BlinkValue()
		{
			Color black = Color.black;
			float currentTime = 0.0f;
			while (true)
			{
				float currentPercent = Mathf.PingPong(currentTime, mBlinkingRate) / mBlinkingRate;
				Color val = Color.Lerp(mEndColor, black, currentPercent);

				foreach (Material m in mMaterialInstances)
					m.SetColor(mEmissiveProperty, val);

				currentTime += Time.deltaTime;
				yield return null;
			}
		}
	}
}
