using System;
using System.Linq;
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
			[SerializeField] private float mAmount;
			[SerializeField] private ModType mType;

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
			[SerializeField] private int mAmount;
			[SerializeField] private ModType mType;

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

		[Serializable]
		public class Array<T>
		{
			[SerializeField] private T[] mAmount;
			[SerializeField] private ModType mType;

			public T[] Apply(T[] input)
			{
				switch (mType)
				{
					case ModType.SetAbsolute:
					case ModType.SetPercentage:
						return mAmount;
					case ModType.AdditiveAbsolute:
					case ModType.AdditivePercent:
						return input.Concat(mAmount).ToArray();
				}

				return input;
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
