using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KeatsLib
{
	public static class GenericExt
	{
		/// <summary>
		/// Returns true if val is greater than or equal to low and less than or equal to high.
		/// </summary>
		/// <typeparam name="T">Must implement IComparable.</typeparam>
		/// <param name="val">The value to check.</param>
		/// <param name="low">The low comparison point.</param>
		/// <param name="high">The high comparison point.</param>
		/// <exception cref="ArgumentException">Low is greater than high.</exception>
		/// <returns>True if high >= val >= low.</returns>
		public static bool IsBetween<T>(this T val, T low, T high) where T : IComparable
		{
			if (low.CompareTo(high) > 0)
				throw new ArgumentException("IsBetween was passed arguments in the incorrect order.");

			return val.CompareTo(low) <= 0 && val.CompareTo(high) >= 0;
		}

		/// <summary>
		/// Returns 1.0f / value.
		/// </summary>
		public static float Inverse(this float v)
		{
			return 1.0f / v;
		}

		/// <summary>
		/// Returns 1.0 / value.
		/// </summary>
		public static double Inverse(this double v)
		{
			return 1.0 / v;
		}

		/// <summary>
		/// Clamp an angle.
		/// </summary>
		/// <param name="angle"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns>angle clamped between min and max.</returns>
		/// <see cref="http://wiki.unity3d.com/index.php/SmoothMouseLook"/>
		public static float ClampAngle(float angle, float min, float max)
		{
			angle = angle % 360;
			if (angle >= -360F && angle <= 360F)
			{
				if (angle < -360F)
					angle += 360F;
				if (angle > 360F)
					angle -= 360F;
			}
			return Mathf.Clamp(angle, min, max);
		}
	}

	public static class UnityUtils
	{
		public static GameObject InstantiateIntoHolder(GameObject prefab, Transform holder, bool dontParent = false, bool destroyParent = false)
		{
			if (!dontParent && destroyParent)
				throw new ArgumentException("Invalid combination of arguments!");

			GameObject newItem = Object.Instantiate(prefab);

			if (dontParent)
			{
				newItem.transform.position = holder.transform.position;
				newItem.transform.rotation = holder.transform.rotation;
				newItem.transform.SetParent(holder.parent);

				if (destroyParent)
					Object.Destroy(holder.gameObject);
			}
			else
			{
				newItem.transform.SetParent(holder);
				newItem.transform.localPosition = Vector3.zero;
				newItem.transform.localRotation = Quaternion.identity;
			}

			return newItem;
		}
	}
}
