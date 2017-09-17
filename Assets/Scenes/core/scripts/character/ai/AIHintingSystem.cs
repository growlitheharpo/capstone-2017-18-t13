using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIHintingSystem : MonoBehaviour
{
	private class NavPoint
	{
		public Vector3 localPos { get; set; }
		public bool valid { get; set; }
		public Vector3 worldPos
		{
			get { return mTransform.TransformPoint(localPos); }
		}

		private readonly Transform mTransform;

		public NavPoint(Transform t, Vector3 localPos)
		{
			mTransform = t;
			this.localPos = localPos;
		}
	}

	[SerializeField]
	private LayerMask mVisibilityLayers;

	private NavPoint[] mPositions;
	private Vector3[] mValidPositions;
	public Vector3[] validWorldPositions { get { return CheckValidPositions(); } }

	private Vector3[] worldPositions { get { return mPositions.Select(x => x.worldPos).ToArray(); } }
	private Vector3 mCachedPosition, mCachedForward;

	// Use this for initialization
	void Start()
	{
		BuildSpots();
		CheckValidPositions();
	}

	private void BuildSpots()
	{
		var spots = new List<Vector3>();

		Action<int, float> addSpots = (size, radius) =>
		{
			for (float i = 0; i < size; i++)
			{
				float x = Mathf.Cos(i / size * Mathf.PI * 2.0f);
				float y = Mathf.Sin(i / size * Mathf.PI * 2.0f);
				spots.Add(new Vector3(x, 0.0f, y) * radius);
			}
		};

		addSpots(16, 6.0f);
		addSpots(24, 8.0f);
		addSpots(32, 10.0f);
		addSpots(48, 12.0f);

		mPositions = spots.Select(x => new NavPoint(transform, x)).ToArray();
	}

	private Vector3[] CheckValidPositions()
	{
		/*if (Vector3.Distance(transform.position, mCachedPosition) < 0.5f
			&& (transform.forward + mCachedForward).magnitude >= 1.8f)
			return mValidPositions;*/

		mCachedPosition = transform.position;
		mCachedForward = transform.forward;

		foreach (NavPoint t in mPositions) {
			Ray ray = new Ray(t.worldPos, transform.position - t.worldPos);

			RaycastHit hitInfo;
			t.valid = Physics.Raycast(ray, out hitInfo, t.localPos.magnitude * 1.5f, mVisibilityLayers)
					&& hitInfo.collider.CompareTag("Player");
		}
		mValidPositions = mPositions
			.Where(pos => pos.valid)
			.Select(pos => pos.worldPos)
			.ToArray();

		return mValidPositions;
	}


	// Update is called once per frame
	void Update()
	{
		CheckValidPositions();
	}

	private void OnDrawGizmos()
	{
		if (mPositions == null)
			BuildSpots();

		Debug.Log(mValidPositions.Length);

		foreach (NavPoint pos in mPositions)
		{
			Gizmos.color = pos.valid ? Color.green : Color.grey;
			Gizmos.DrawSphere(pos.worldPos, 0.25f);
		}
	}
}
