using System.Collections;
using UnityEngine;
using UIText = UnityEngine.UI.Text;

namespace Prototype2
{
	public class SampleTargetScript : MonoBehaviour, IDamageReceiver
	{
		[SerializeField] private ParticleSystem mDeathParticles;
		[SerializeField] private GameObject mHitIndicator;
		[SerializeField] private GameObject mMesh;
		[SerializeField] private UIText mText;
		[SerializeField] private float mStartHealth;

		private BoundProperty<float> mHealth;

		private void Awake()
		{
			mHealth = new BoundProperty<float>(mStartHealth, (gameObject.name + "-health").GetHashCode());
		}

		public void ApplyDamage(float amount, Vector3 point)
		{
			StopAllCoroutines();
			mHealth.value = Mathf.Clamp(mHealth.value - amount, 0.0f, float.MaxValue);

			if (mHealth.value <= 0.0f)
			{
				Die();
				return;
			}

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

		private void Die()
		{
			mMesh.SetActive(false);
			mDeathParticles.Play();
			StartCoroutine(WaitForDeath());
		}

		private IEnumerator WaitForDeath()
		{
			yield return null; // wait 1 frame
			yield return new WaitForParticles(mDeathParticles);

			Destroy(gameObject);
		}
	}
}
