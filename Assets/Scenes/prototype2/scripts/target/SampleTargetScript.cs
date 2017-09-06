using System.Collections;
using UnityEngine;
using UIText = UnityEngine.UI.Text;

namespace Prototype2
{
	public class SampleTargetScript : MonoBehaviour, IDamageReceiver
	{
		[SerializeField] private UIText mText;
		[SerializeField] private GameObject mHitIndicator;
		
		public void ApplyDamage(float amount, Vector3 point)
		{
			StopAllCoroutines();
			mText.color = new Color(0.4f, 0.4f, 0.4f, 1.0f);
			mText.text = "Damage:\n" + amount.ToString("####");

			Instantiate(mHitIndicator, point, Quaternion.identity);

			StartCoroutine(FadeText());
		}

		private IEnumerator FadeText()
		{
			Color startCol = mText.color;
			float currentTime = 0.0f;

			while (currentTime < 0.75f)
			{
				mText.color = Color.Lerp(startCol, new Color(startCol.r, startCol.g, startCol.b, 0.0f), currentTime / 0.75f);
				currentTime += Time.deltaTime;
				yield return null;
			}

			yield return null;
		}
	}
}
