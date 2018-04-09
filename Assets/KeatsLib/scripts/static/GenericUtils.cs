using System;
using UnityEngine;

namespace KeatsLib.Collections
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

		/// <summary>
		/// Convert a number to an ordinal representation (1 -> 1st, 2 -> 2nd, etc.)
		/// Thanks to: https://stackoverflow.com/a/20175
		/// </summary>
		/// <param name="val">The number to convert.</param>
		public static string ToStringOrdinal(this uint val)
		{
			switch (val % 100)
			{
				case 11:
				case 12:
				case 13:
					return val + "th";
				default:
					switch (val % 10)
					{
						case 1:
							return val + "st";
						case 2:
							return val + "nd";
						case 3:
							return val + "rd";
						default:
							return val + "th";
					}
			}
		}
	}
}
