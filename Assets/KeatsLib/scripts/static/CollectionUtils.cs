using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace KeatsLib.Collections
{
	public static class SystemExtensions
	{
		public static class Types
		{
			/// <summary>
			/// Utility struct used for forward-iterating over a fixed-size container.
			/// Wraps around to index 0 when you attempt to increment past the end.
			/// </summary>
			public struct IteratorIndex
			{
				private int mValue;
				private int mSize;

				public IteratorIndex(int size, int initialValue = 0)
				{
					mSize = size;

					mValue = initialValue >= size ? size - 1 : initialValue;
				}

				public void SetSize(int newSize)
				{
					mSize = newSize;
				}

				public void ResetValue()
				{
					mValue = 0;
				}

				public static implicit operator int(IteratorIndex i)
				{
					return i.mValue;
				}

				public static IteratorIndex operator ++(IteratorIndex i)
				{
					i.mValue++;
					if (i.mValue >= i.mSize)
						i.mValue = 0;

					return i;
				}
			}

			/// <summary>
			/// Utility class similar in function to C++'s std::pair class.
			/// </summary>
			public class Pair<TX, TY>
			{
				public TX first { get; private set; }

				public TY second { get; private set; }

				public Pair(TX first, TY second)
				{
					this.first = first;
					this.second = second;
				}

				public override bool Equals(object obj)
				{
					if (obj == null)
						return false;
					if (obj == this)
						return true;

					var other = obj as Pair<TX, TY>;
					if (other == null)
						return false;

					return
						(first == null && other.first == null
						|| first != null && first.Equals(other.first))
						&&
						(second == null && other.second == null
						|| second != null && second.Equals(other.second));
				}

				public override int GetHashCode()
				{
					int hashcode = 0;
					if (first != null)
						hashcode += first.GetHashCode();
					if (second != null)
						hashcode += second.GetHashCode();

					return hashcode;
				}
			}
		}

		/// <summary>
		/// Convert a string to an enum of the given type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public static T ToEnum<T>(this string value)
		{
			return (T)Enum.Parse(typeof(T), value, true);
		}

		public static T RandomEnum<T>()
		{
			Type type = typeof(T);
			object val = Enum.GetValues(type).ChooseRandom();

			return (T)Convert.ChangeType(val, type);
		}

		/// <summary>
		/// Extension to select and return a random item from any IEnumerable.
		/// </summary>
		/// <param name="collection">The IEnumerable to iterate over and choose an item from.</param>
		/// <returns>A random item from the provided IEnumerable.</returns>
		public static T ChooseRandom<T>(this IEnumerable<T> collection)
		{
			var iList = collection as IList<T> ?? collection.ToArray();
			if (iList.Count == 0)
				throw new ArgumentException("Trying to choose random item from an empty list!");

			return iList[Random.Range(0, iList.Count)];
		}

		/// <summary>
		/// Extension to select and return a random weighted item from an IEnumerable.
		/// </summary>
		/// <param name="collection">The IEnumerable to iterate through and choose an item from.</param>
		/// <param name="weight">
		/// A function that takes each item from the list and returns its weight.
		/// If called for every item in "collection", the sum of the results should equal 1.0f.
		/// </param>
		/// <returns></returns>
		public static T ChooseRandomWeighted<T>(this IEnumerable<T> collection, Func<T, float> weight)
		{
			var iList = collection as IList<T> ?? collection.ToArray();
			float ranVal = Random.value;

			float accumulator = 0.0f;
			foreach (T t in iList)
			{
				accumulator += weight(t);
				if (accumulator >= ranVal)
					return t;
			}

			throw new ArgumentException("Weights did not sum to 1.00f!");
		}

		/// <summary>
		/// Extension to select and return a random item from any IEnumerable.
		/// NOTE: If the collection does not implement IList, the collection will be iterated through up to twice!
		/// </summary>
		/// <param name="collection">The IEnumerable to iterate over and choose an item from.</param>
		/// <returns>A random item from the provided IEnumerable.</returns>
		public static object ChooseRandom(this IEnumerable collection)
		{
			IList iList = collection as IList;
			if (iList != null)
			{
				if (iList.Count == 0)
					throw new ArgumentException("Trying to choose random item from an empty list!");

				return iList[Random.Range(0, iList.Count)];
			}

			IEnumerator enumerator = collection.GetEnumerator();
			int count = 0;

			while (enumerator.MoveNext())
				count++;

			if (count == 0)
				throw new ArgumentException("Trying to choose random item from an empty list!");

			enumerator.Reset();

			int index = Random.Range(0, count);
			for (int i = 0; i < index; i++)
				enumerator.MoveNext();

			return enumerator.Current;
		}

		/// <summary>
		/// Extension to randomly rearrange the items in an IList.
		/// </summary>
		/// <param name="list">The list to rearrange.</param>
		public static void Shuffle<T>(this IList<T> list)
		{
			for (int i = 0; i < list.Count; i++)
				list.SwapElement(i, Random.Range(i, list.Count));
		}

		/// <summary>
		/// Swap two elements in a list based on their index.
		/// </summary>
		/// <param name="list">The list to swap the elements in.</param>
		/// <param name="i">The index of the first element to swap.</param>
		/// <param name="j">The index of the second element to swap.</param>
		/// <exception cref="System.IndexOutOfRangeException">Either i or j is out of range of the list.</exception>
		public static void SwapElement<T>(this IList<T> list, int i, int j)
		{
			T tmp = list[i];
			list[i] = list[j];
			list[j] = tmp;
		}

		public static T1 GetHighestValueKey<T1, T2>(this IDictionary<T1, T2> list) where T2 : IComparable
		{
			return list.Aggregate((l, r) => l.Value.CompareTo(r.Value) >= 0 ? l : r).Key;
		}

		public static float Rescale(this float val, float oldMin, float oldMax, float newMin = 0.0f, float newMax = 1.0f)
		{
			float oldRange = oldMax - oldMin;
			float newRange = newMax - newMin;

			return (val - oldMin) / oldRange * newRange + newMin;
		}
	}
}
