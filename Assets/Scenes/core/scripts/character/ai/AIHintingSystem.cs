using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

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

	[Serializable]
	public class AINavigationValues
	{
		public float mPlayerDistanceExponent;
		public float mPlayerDistanceWeight = 1.0f;

		public float mPlayerForwardExponent;
		public float mPlayerForwardWeight = 1.0f;

		public float mEnemyDistanceExponent;
		public float mEnemyDistanceWeight = 1.0f;
	}

	public AINavigationValues mValues;

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
		if (Vector3.Distance(transform.position, mCachedPosition) < 0.5f
			&& (transform.forward + mCachedForward).magnitude >= 1.995f)
			return mValidPositions;

		mCachedPosition = transform.position;
		mCachedForward = transform.forward;

		foreach (NavPoint t in mPositions) {
			float dist = t.localPos.magnitude;
			Vector3 up = new Vector3(0.0f, 0.25f, 0.0f);
			Ray ray = new Ray(transform.position + up, (t.worldPos - transform.position) + up);

			Debug.DrawLine(ray.origin, ray.origin + ray.direction * dist, Color.magenta, 0.5f);

			RaycastHit hitInfo;
			t.valid = !Physics.Raycast(ray, out hitInfo, dist, mVisibilityLayers);
		}
		mValidPositions = mPositions
			.Where(pos => pos.valid)
			.Select(pos => pos.worldPos)
			.ToArray();

		return mValidPositions;
	}


	// Update is called once per frame
	private void Update()
	{
		CheckValidPositions();
	}

	private void OnDrawGizmos()
	{
		if (mPositions == null || mValidPositions == null)
			return;

		float hRed, hGreen, s, v;
		Color.RGBToHSV(Color.red, out hRed, out s, out v);
		Color.RGBToHSV(Color.green, out hGreen, out s, out v);

		/*foreach (NavPoint pos in mPositions)
		{
			Color col;
			if (!pos.valid)
				col = Color.black;
			else
			{
				//float hue = Mathf.Lerp(hRed, hGreen, ScorePositionDistanceFromPlayer(pos.worldPos, Vector3.down, goalDist));
				col = Color.HSVToRGB(hGreen, s, v);
			}

			//Gizmos.color = pos.valid ? Color.green : Color.grey;
			Gizmos.color = col;
			Gizmos.DrawSphere(pos.worldPos, 0.25f);
		}*/
		var enemyPos = FindObjectOfType<FiringSquad.Gameplay.AICharacter>();

		var allPositions = mPositions;
		var forwardScores = allPositions.Select(x => RawScorePositionPlayerForward(x.worldPos));
		var playerDistScores = allPositions.Select(x => RawScorePositionDistanceFromPlayer(x.worldPos));
		var enemyDistScores = allPositions.Select(x => RawScorePositionDistanceFromEnemy(x.worldPos, enemyPos.transform.position));

		forwardScores = NormalizeValues(forwardScores);
		playerDistScores = NormalizeValues(playerDistScores);
		enemyDistScores = NormalizeValues(enemyDistScores);

		forwardScores = ModValuesExponent(forwardScores, mValues.mPlayerForwardExponent);
		playerDistScores = ModValuesExponent(playerDistScores, mValues.mPlayerDistanceExponent);
		enemyDistScores = ModValuesExponent(enemyDistScores, mValues.mEnemyDistanceExponent);

		var finalScore = CreateFinalScore(new[] { forwardScores.ToArray(), playerDistScores.ToArray(), enemyDistScores.ToArray() },
			new[] { mValues.mPlayerForwardWeight, mValues.mEnemyDistanceWeight, mValues.mEnemyDistanceWeight });

		for (int i = 0; i < allPositions.Length; i++)
		{
			Color col;
			if (!allPositions[i].valid)
				col = Color.black;
			else
			{
				float hue = Mathf.Lerp(hRed, hGreen, finalScore[i]);
				col = Color.HSVToRGB(hue, s, v);
			}

			Gizmos.color = col;
			Gizmos.DrawSphere(allPositions[i].worldPos, 0.25f);
		}
	}

	// evaluations: dot product with player's forward, distance from player, distance from enemy
	public float RawScorePositionPlayerForward(Vector3 worldPos)
	{
		Vector3 direction = worldPos - transform.position;
		return Vector3.Dot(transform.forward, direction.normalized);
	}

	public float RawScorePositionDistanceFromPlayer(Vector3 worldPos)
	{
		return Vector3.Distance(worldPos, transform.position);
	}

	public float RawScorePositionDistanceFromEnemy(Vector3 worldPos, Vector3 enemyPos)
	{
		return Vector3.Distance(worldPos, enemyPos);
	}

	/// <see cref="https://stackoverflow.com/a/5384025"/>
	public static float[] NormalizeValues(IEnumerable<float> values)
	{
		var floats = values as float[] ?? values.ToArray();
		float dataMin = floats.Min();
		float dataMax = floats.Max();
		float range = dataMax - dataMin;

		return floats
			.Select(d => (d - dataMin) / range)
			.ToArray();
	}

	public static IEnumerable<float> ModValuesInverse(IEnumerable<float> values)
	{
		return values.Select(x => 1.0f / x);
	}

	public static IEnumerable<float> ModValuesExponent(IEnumerable<float> values, float exponent)
	{
		return values.Select(x => Mathf.Pow(x, exponent)).ToArray();
	}

	public static IEnumerable<float> ModValuesInverseSquare(IEnumerable<float> values, float exponent = 2.0f)
	{
		return values.Select(x => 1.0f / Mathf.Pow(x, exponent)).ToArray();
	}

	public static float[] CreateFinalScore(float[][] allValues, float[] weights)
	{
		var vals = new float[allValues[0].Length];

		for (int i = 0; i < vals.Length; i++)
		{
			float sum = allValues.Select((t, j) => t[i] * weights[j]).Sum() / weights.Length;
			vals[i] = sum;
		}

		return vals;
	}
}
