using System.Collections;
using UnityEngine;

public class HitscanShootEffect : MonoBehaviour
{
	private LineRenderer mRenderer;

	private void Awake()
	{
		mRenderer = GetComponent<LineRenderer>();
	}

	public IEnumerator Flash(Vector3 end, float time = 0.15f, Space endSpace = Space.World)
	{
		Vector3 realEnd = endSpace == Space.World ? transform.InverseTransformPoint(end) : end;

		float currentTime = 0.0f;
		float moveTime = time / 2.0f;

		mRenderer.positionCount = 2;
		mRenderer.SetPosition(0, Vector3.zero);
		mRenderer.SetPosition(1, Vector3.zero);

		while (currentTime < moveTime)
		{
			mRenderer.SetPosition(1, Vector3.Lerp(Vector3.zero, realEnd, currentTime / moveTime));
			currentTime += Time.deltaTime;
			yield return null;
		}

		currentTime = 0.0f;
		while (currentTime < moveTime)
		{
			mRenderer.SetPosition(0, Vector3.Lerp(Vector3.zero, realEnd, currentTime / moveTime));
			currentTime += Time.deltaTime;
			yield return null;
		}

		mRenderer.positionCount = 0;
	}
}
