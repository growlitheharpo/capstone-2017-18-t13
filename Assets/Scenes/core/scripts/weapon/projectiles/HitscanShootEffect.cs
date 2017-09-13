using System.Collections;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	public class HitscanShootEffect : MonoBehaviour
	{
		private LineRenderer mRenderer;

		private void Awake()
		{
			mRenderer = GetComponent<LineRenderer>();
		}

		public IEnumerator Flash(Vector3 end, float time = 0.15f, Space endSpace = Space.World)
		{
			Vector3 realEnd = endSpace == Space.World ? end : transform.TransformPoint(end);
			Vector3 start = transform.position;

			float currentTime = 0.0f;
			float moveTime = time / 2.0f;

			mRenderer.positionCount = 2;
			mRenderer.SetPosition(0, start);
			mRenderer.SetPosition(1, start);

			while (currentTime < moveTime)
			{
				mRenderer.SetPosition(1, Vector3.Lerp(start, realEnd, currentTime / moveTime));
				currentTime += Time.deltaTime;
				yield return null;
			}

			currentTime = 0.0f;
			while (currentTime < moveTime)
			{
				mRenderer.SetPosition(0, Vector3.Lerp(start, realEnd, currentTime / moveTime));
				currentTime += Time.deltaTime;
				yield return null;
			}

			mRenderer.positionCount = 0;
		}
	}
}
