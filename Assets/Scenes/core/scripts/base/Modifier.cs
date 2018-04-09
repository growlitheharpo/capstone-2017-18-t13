using System;
using UnityEngine;

namespace FiringSquad.Data
{
	/// <summary>
	/// Utility class used for modifying other values.
	/// </summary>
	[Serializable]
	public class Modifier
	{
		[Serializable]
		public class Float
		{
			[SerializeField] private readonly float mAmount;
			[SerializeField] private readonly ModType mType;

			/// <summary>
			/// Apply this Modifier's effect to the provided number.
			/// </summary>
			public float Apply(float input)
			{
				switch (mType)
				{
					case ModType.SetAbsolute:
						return mAmount;
					case ModType.SetPercentage:
						return input * mAmount;
					case ModType.AdditiveAbsolute:
						return input + mAmount;
					case ModType.AdditivePercent:
						return input + input * mAmount;
				}

				return input;
			}

			public Float(float value, ModType type)
			{
				mType = type;
				mAmount = value;
			}
		}

		[Serializable]
		public class Int
		{
			[SerializeField] private readonly int mAmount;
			[SerializeField] private readonly ModType mType;

			/// <summary>
			/// Apply this Modifier's effect to the provided number.
			/// </summary>
			public int Apply(int input)
			{
				switch (mType)
				{
					case ModType.SetAbsolute:
						return mAmount;
					case ModType.SetPercentage:
						return (int)(input * (mAmount / 100.0f));
					case ModType.AdditiveAbsolute:
						return input + mAmount;
					case ModType.AdditivePercent:
						return (int)(input + input * (mAmount / 100.0f));
				}

				return input;
			}

			public Int(int value, ModType type)
			{
				mType = type;
				mAmount = value;
			}
		}

		public enum ModType
		{
			AdditiveAbsolute,
			SetAbsolute,
			AdditivePercent,
			SetPercentage
		}
	}
}
