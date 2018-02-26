using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace KeatsLib.Collections
{
	/// <summary>
	/// A collection of extensions and static classes for general purposes.
	/// </summary>
	public static class SystemExtensions
	{
		public static class Types
		{
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

		/// <summary>
		/// Returns a random value within the provided enum.
		/// </summary>
		/// <typeparam name="T">The type of enum to choose from.</typeparam>
		public static T RandomEnum<T>() 
		{
			Type type = typeof(T);
			object val = Enum.GetValues(type).ChooseRandom();

			return (T)val;
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
		public static IList<T> Shuffle<T>(this IList<T> list)
		{
			for (int i = 0; i < list.Count; i++)
				list.SwapElement(i, Random.Range(i, list.Count));
			return list;
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

		/// <summary>
		/// Chooses a key from a dictionary which has the highest value paired to it.
		/// </summary>
		/// <param name="list">The dictionary from which to choose.</param>
		/// <returns>Returns the T1 Key with the highest T2 value.</returns>
		public static T1 GetHighestValueKey<T1, T2>(this IDictionary<T1, T2> list) where T2 : IComparable
		{
			return list.Aggregate((l, r) => l.Value.CompareTo(r.Value) >= 0 ? l : r).Key;
		}

		/// <summary>
		/// Rescales a value that was previously in range [oldMin, oldMax] to range [newMin, newMax].
		/// </summary>
		/// <param name="val">The value to transform.</param>
		/// <param name="oldMin">The lowest possible value in the previous scale.</param>
		/// <param name="oldMax">The highest possible value in the previous scale.</param>
		/// <param name="newMin">The lowest possible value in the new scale. Defaults to 0.</param>
		/// <param name="newMax">The highest possible value in the new scale. Defaults to 1.</param>
		/// <returns></returns>
		public static float Rescale(this float val, float oldMin, float oldMax, float newMin = 0.0f, float newMax = 1.0f)
		{
			float oldRange = oldMax - oldMin;
			float newRange = newMax - newMin;

			return (val - oldMin) / oldRange * newRange + newMin;
		}
	}
}
