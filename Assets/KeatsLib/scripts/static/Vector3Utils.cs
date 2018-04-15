using System.Collections.Generic;
using KeatsLib.Collections;
using UnityEngine;

namespace KeatsLib.Unity
{
	/// <summary>
	/// A collection of useful Vector2, Vector3, and Rect functions.
	/// </summary>
	public static class Vector3Ext
	{
		/// <summary>
		/// Get the average of a collection of Vectors.
		/// </summary>
		/// <param name="vecs">The collection of Vectors to average</param>
		/// <returns>The average of all the provided Vectors.</returns>
		public static Vector3 Average(ICollection<Vector3> vecs)
		{
			Vector3 result = Vector3.zero;

			foreach (Vector3 v in vecs)
				result += v;

			result /= vecs.Count;
			return result;
		}

		/// <summary>
		/// Get the square of the distance between two Vectors.
		/// </summary>
		/// <param name="a">The first point.</param>
		/// <param name="b">The second point.</param>
		/// <returns>The square of the distance between a and b.</returns>
		public static float DistanceSqrd(Vector3 a, Vector3 b)
		{
			return (a - b).sqrMagnitude;
		}

		/// <summary> Gets a random position within a Bounds object. </summary>
		/// <param name="bounding">The bounding box for the position.</param>
		/// <returns>A valid position</returns>
		public static Vector3 RandomContainedPosition(Bounds bounding)
		{
			return new Vector3(
				Random.Range(bounding.min.x, bounding.max.x),
				Random.Range(bounding.min.y, bounding.max.y),
				Random.Range(bounding.min.z, bounding.max.z));
		}

		/// <summary>
		/// Gets a random position with a Transform's bounding box.
		/// </summary>
		/// <param name="cube">The Transform within which to generate a position.</param>
		/// <returns>A valid position.</returns>
		public static Vector3 RandomContainedPosition(Transform cube)
		{
			Vector3 rndPosWithin = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
			rndPosWithin = cube.TransformPoint(rndPosWithin * 0.5f);

			return rndPosWithin;
		}

		/// <summary>
		/// Checks if a position provided is within a Transform's bounding box.
		/// </summary>
		/// <param name="point">The position to check.</param>
		/// <param name="cube">The Transform cube to check.</param>
		/// <returns>True if the point is inside cube. Otherwise, false.</returns>
		public static bool CubeContains(Vector3 point, Transform cube)
		{
			if (cube == null)
				return false;

			Vector3 pointScale = cube.InverseTransformPoint(point) * 2.0f;

			return pointScale.x.IsBetween(-1.0f, 1.0f) && pointScale.y.IsBetween(-1.0f, 1.0f) && pointScale.z.IsBetween(-1.0f, 1.0f);
		}

		/// <summary>
		/// Get the distance from a point to the nearest point on a cube.
		/// </summary>
		/// <returns>The distance from the point to the nearest position on the cube.</returns>
		public static float DistanceCubePoint(Vector3 point, Transform cube)
		{
			Vector3 boxMin = new Vector3(cube.position.x - cube.localScale.x / 2.0f, cube.position.y - cube.localScale.y / 2.0f,
				cube.position.z - cube.localScale.z / 2.0f);
			Vector3 boxMax = new Vector3(cube.position.x + cube.localScale.x / 2.0f, cube.position.y + cube.localScale.y / 2.0f,
				cube.position.z + cube.localScale.z / 2.0f);

			Vector3 closestPoint = point.ClampComponentwise(boxMin, boxMax);
			return Vector3.Distance(point, closestPoint);
		}

		/// <summary>
		/// Get the distance from a point to the nearest point on a cube.
		/// </summary>
		/// <returns>The distance from the point to the nearest position on the cube.</returns>
		public static float DistanceCubePoint(Vector3 point, Bounds cube)
		{
			Vector3 closestPoint = point.ClampComponentwise(cube.min, cube.max);
			return Vector3.Distance(point, closestPoint);
		}

		/// <summary>
		/// Provide a copy of a Vector with its magnitude clamped to maxMagnitude.
		/// </summary>
		/// <param name="v">The Vector to copy.</param>
		/// <param name="maxMagnitude">The new maximum magnitude.</param>
		/// <returns>A new Vector in the direction of v with a maximum magnitude of maxMagnitude.</returns>
		public static Vector3 ClampMagnitude(this Vector3 v, float maxMagnitude)
		{
			if (v.magnitude > maxMagnitude)
				return v.normalized * maxMagnitude;

			return v;
		}

		/// <summary>
		/// Provide a copy of a Vector with its components individually clamped without regard for the original direction of
		/// the Vector.
		/// </summary>
		/// <param name="v">The Vector to copy with clamped components.</param>
		/// <param name="min">The Vector whose components will be used as the minimum values.</param>
		/// <param name="max">The Vector whose components will be used as the maximum values.</param>
		/// <returns>A new Vector copy of v with its components clamped without regard for the original direction.</returns>
		public static Vector3 ClampComponentwise(this Vector3 v, Vector3 min, Vector3 max)
		{
			return new Vector3(
				Mathf.Clamp(v.x, min.x, max.x),
				Mathf.Clamp(v.y, min.y, max.y),
				Mathf.Clamp(v.z, min.z, max.z));
		}

		/// <summary>
		/// Provide a copy of a Vector with its components individually clamped without regard for the original direction of
		/// the Vector.
		/// </summary>
		/// <param name="v">The Vector to copy with clamped components.</param>
		/// <param name="min">The minimum value for each component.</param>
		/// <param name="max">The maximum value for each component.</param>
		/// <returns>A new Vector copy of v with its components clamped without regard for the original direction.</returns>
		public static Vector3 ClampComponentwise(this Vector3 v, float min, float max)
		{
			return new Vector3(
				Mathf.Clamp(v.x, min, max),
				Mathf.Clamp(v.y, min, max),
				Mathf.Clamp(v.z, min, max));
		}
	}

	public static class Vector2Ext
	{
		/// <summary>
		/// Get the average of a collection of Vectors.
		/// </summary>
		/// <param name="vecs">The collection of Vectors to average</param>
		/// <returns>The average of all the provided Vectors.</returns>
		public static Vector2 Average(ICollection<Vector2> vecs)
		{
			Vector2 result = Vector2.zero;

			foreach (Vector2 v in vecs)
				result += v;

			result /= vecs.Count;
			return result;
		}

		/// <summary>
		/// Get the square of the distance between two Vectors.
		/// </summary>
		/// <param name="a">The first point.</param>
		/// <param name="b">The second point.</param>
		/// <returns>The square of the distance between a and b.</returns>
		public static float DistanceSqrd(Vector2 a, Vector2 b)
		{
			return (a - b).sqrMagnitude;
		}

		/// <summary>
		/// Provide a copy of a Vector with its magnitude clamped to maxMagnitude.
		/// </summary>
		/// <param name="v">The Vector to copy.</param>
		/// <param name="maxMagnitude">The new maximum magnitude.</param>
		/// <returns>A new Vector in the direction of v with a maximum magnitude of maxMagnitude.</returns>
		public static Vector2 ClampMagnitude(this Vector2 v, float maxMagnitude)
		{
			if (v.magnitude > maxMagnitude)
				return v.normalized * maxMagnitude;

			return v;
		}

		/// <summary>
		/// Provide a copy of a Vector with its components individually clamped without regard for the original direction of
		/// the Vector.
		/// </summary>
		/// <param name="v">The Vector to copy with clamped components.</param>
		/// <param name="min">The Vector whose components will be used as the minimum values.</param>
		/// <param name="max">The Vector whose components will be used as the maximum values.</param>
		/// <returns>A new Vector copy of v with its components clamped without regard for the original direction.</returns>
		public static Vector2 ClampComponentwise(this Vector2 v, Vector2 min, Vector2 max)
		{
			return new Vector2(
				Mathf.Clamp(v.x, min.x, max.x),
				Mathf.Clamp(v.y, min.y, max.y));
		}

		/// <summary>
		/// Provide a copy of a Vector with its components individually clamped without regard for the original direction of
		/// the Vector.
		/// </summary>
		/// <param name="v">The Vector to copy with clamped components.</param>
		/// <param name="min">The minimum value for each component.</param>
		/// <param name="max">The maximum value for each component.</param>
		/// <returns>A new Vector copy of v with its components clamped without regard for the original direction.</returns>
		public static Vector2 ClampComponentwise(this Vector2 v, float min, float max)
		{
			return new Vector2(
				Mathf.Clamp(v.x, min, max),
				Mathf.Clamp(v.y, min, max));
		}
	}

	public static class RectExt
	{
		/// <summary>
		/// Shift a rectangle's X position to the right by its width.
		/// </summary>
		/// <param name="r">The rectangle to shift.</param>
		/// <param name="width">The width of the resulting rectangle.</param>
		public static Rect ShiftAlongX(this Rect r, float width)
		{
			return new Rect(r.x + r.width, r.y, width, r.height);
		}

		/// <summary>
		/// Shift a rectangle's Y position down by its height.
		/// </summary>
		/// <param name="r">The rectangle to shift.</param>
		/// <param name="width">The WIDTH of the resulting rectangle.</param>
		public static Rect ShiftAlongY(this Rect r, float width)
		{
			return new Rect(r.x, r.y + r.height, width, r.height);
		}

		/// <summary>
		/// Shift a rectangle's X position to the right by its width.
		/// </summary>
		/// <param name="r">The rectangle to shift.</param>
		/// <param name="width">The width of the resulting rectangle.</param>
		/// <param name="height">The height of the resulting rectangle.</param>
		public static Rect ShiftAlongX(this Rect r, float width, float height)
		{
			return new Rect(r.x + r.width, r.y, width, height);
		}

		/// <summary>
		/// Shift a rectangle's Y position down by its height.
		/// </summary>
		/// <param name="r">The rectangle to shift.</param>
		/// <param name="width">The width of the resulting rectangle.</param>
		/// <param name="height">The height of the resulting rectangle.</param>
		public static Rect ShiftAlongY(this Rect r, float width, float height)
		{
			return new Rect(r.x, r.y + r.height, width, height);
		}
	}

	public static class RaycastUtils
	{
		/// <summary>
		/// Utility IComparer for sorting on distance
		/// NOTE: This is a struct, so it should be allocated on the stack instead of the heap!
		/// </summary>
		public struct RaycastHitDistComparer : IComparer<RaycastHit>
		{
			public int Compare(RaycastHit a, RaycastHit b)
			{
				if (a.distance < b.distance)
					return -1;
				return 1;
			}
		}
	}

	public static class TransformExt
	{
		/// <summary>
		/// Sets local position and rotation to (0,0,0), and local scale to (1,1,1)
		/// </summary>
		public static Transform ResetLocalValues(this Transform t)
		{
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
			t.localScale = Vector3.one;
			return t;
		}

		/// <summary>
		/// Resets everything on a RectTransform to how it appears by default when adding
		/// a new canvas item. (Anchors of (0.5, 0.5) (0.5, 0.5) and a given size.
		/// </summary>
		/// <param name="t">The RectTransform to affect.</param>
		/// <param name="size">The square size of the new Rect.</param>
		public static RectTransform ResetEverything(this RectTransform t, float size)
		{
			t.anchorMax = new Vector2(0.5f, 0.5f);
			t.anchorMin = new Vector2(0.5f, 0.5f);

			t.localScale = Vector3.one;
			t.anchoredPosition3D = Vector3.zero;
			t.localRotation = Quaternion.identity;
			t.sizeDelta = new Vector2(size, size);

			return t;
		}

		/// <summary>
		/// Resets everything on a RectTransform to how it appears by default, except
		/// with the provided anchors and no sizeDelta.
		/// </summary>
		/// <param name="t">The RectTransform to affect.</param>
		/// <param name="anchorMin">The new minimum anchors.</param>
		/// <param name="anchorMax">The new maximum anchors.</param>
		/// <returns></returns>
		public static RectTransform ResetEverything(this RectTransform t, Vector2 anchorMin, Vector2 anchorMax)
		{
			t.anchorMin = anchorMin;
			t.anchorMax = anchorMax;

			t.localScale = Vector3.one;
			t.anchoredPosition3D = Vector3.zero;
			t.localRotation = Quaternion.identity;
			t.sizeDelta = new Vector2(0.0f, 0.0f);

			return t;
		}
	}
}
