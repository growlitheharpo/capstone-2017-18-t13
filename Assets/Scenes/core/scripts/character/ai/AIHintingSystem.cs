using System;
using System.Collections.Generic;
using System.Linq;
using FiringSquad.Data;
using UnityEngine;

namespace FiringSquad.Gameplay.AI
{
	public class AIHintingSystem : MonoBehaviour
	{
		private class NavPoint
		{
			public Vector3 localPos { get; private set; }
			public bool valid { get; set; }
			public Vector3 worldPos { get { return mTransform.TransformPoint(localPos); } }

			private readonly Transform mTransform;

			public NavPoint(Transform t, Vector3 localPos)
			{
				mTransform = t;
				this.localPos = localPos;
			}
		}

		[SerializeField] private LayerMask mVisibilityLayers;

		private NavPoint[] mPositions;
		private Vector3[] mValidPositions;
		public Vector3[] validWorldPositions { get { return CheckValidPositions(); } }

		private Vector3 mCachedPosition, mCachedForward;
		
		// Use this for initialization
		private void Start()
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

			foreach (NavPoint t in mPositions)
			{
				float dist = t.localPos.magnitude;
				Vector3 up = new Vector3(0.0f, 0.25f, 0.0f);
				Ray ray = new Ray(transform.position + up, (t.worldPos - transform.position) + up);

				RaycastHit hitInfo;
				t.valid = !Physics.Raycast(ray, out hitInfo, dist, mVisibilityLayers);
			}
			mValidPositions = mPositions
				.Where(pos => pos.valid)
				.Select(pos => pos.worldPos)
				.ToArray();

			return mValidPositions;
		}
		
		public Dictionary<Vector3, float> EvaluatePosition(Vector3 enemyPosition, AIHintValueData values)
		{
			var allPositions = validWorldPositions;
			var forwardScores = allPositions.Select(RawScorePositionPlayerForward);
			var playerDistScores = allPositions.Select(RawScorePositionDistanceFromPlayer);
			var enemyDistScores = allPositions.Select(x => RawScorePositionDistanceFromEnemy(x, enemyPosition));

			forwardScores = NormalizeValues(forwardScores);
			playerDistScores = NormalizeValues(playerDistScores);
			enemyDistScores = NormalizeValues(enemyDistScores);

			playerDistScores = ReversedNormalizedValues(playerDistScores);
			enemyDistScores = ReversedNormalizedValues(enemyDistScores);

			forwardScores = ModValuesExponent(forwardScores, values.playerForwardExponent);
			playerDistScores = ModValuesExponent(playerDistScores, values.playerDistanceExponent);
			enemyDistScores = ModValuesExponent(enemyDistScores, values.enemyDistanceExponent);

			var scores = CreateFinalScore(
				new[] { forwardScores.ToArray(), playerDistScores.ToArray(), enemyDistScores.ToArray() },
				new[] { values.playerForwardWeight, values.playerDistanceWeight, values.enemyDistanceWeight }).ToArray();

			var result = new Dictionary<Vector3, float>();
			for (int i = 0; i < allPositions.Length; i++)
				result[allPositions[i]] = scores[i];
			return result;
		}

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
		public static IEnumerable<float> NormalizeValues(IEnumerable<float> values)
		{
			var floats = values as float[] ?? values.ToArray();
			float dataMin = floats.Min();
			float dataMax = floats.Max();
			float range = dataMax - dataMin;

			return floats.Select(d => (d - dataMin) / range);
		}

		public static IEnumerable<float> ReversedNormalizedValues(IEnumerable<float> values)
		{
			return values.Select(x => 1.0f - x);
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

		public static IEnumerable<float> CreateFinalScore(float[][] allValues, float[] weights)
		{
			var vals = new float[allValues[0].Length];

			for (int i = 0; i < vals.Length; i++)
			{
				float sum = allValues.Select((t, j) => t[i] * weights[j]).Sum();
				sum /= weights.Sum();

				vals[i] = sum;
			}

			return vals;
		}
	}
}
