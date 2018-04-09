using System.Collections;
using System.Collections.Generic;

// ReSharper disable InconsistentNaming
// Disabled because this class is meant to match the Stack interface 1-to-1

namespace KeatsLib.Collections
{
	/// <summary>
	/// A standard Stack implementation which will only allow unique items.
	/// Pushing an existing item onto the stack will move it to the end.
	/// </summary>
	/// <typeparam name="T">The type to place in the stack.</typeparam>
	/// <inheritdoc />
	public class UniqueStack<T> : IEnumerable<T> where T : class
	{
		private readonly List<T> mItems;

		/// <summary>
		/// A standard Stack implementation which will only allow unique items.
		/// Pushing an existing item onto the stack will move it to the end.
		/// </summary>
		public UniqueStack()
		{
			mItems = new List<T>();
		}

		/// <summary>
		/// A standard Stack implementation which will only allow unique items.
		/// Pushing an existing item onto the stack will move it to the end.
		/// </summary>
		/// <param name="count">The size of the stack.</param>
		public UniqueStack(int count)
		{
			mItems = new List<T>(count);
		}

		/// <summary>
		/// A standard Stack implementation which will only allow unique items.
		/// Pushing an existing item onto the stack will move it to the end.
		/// </summary>
		/// <param name="collection">A collection to copy into the stack.</param>
		public UniqueStack(IEnumerable<T> collection)
		{
			mItems = new List<T>(collection);
		}

		/// <summary>
		/// The number of items currently in the stack.
		/// </summary>
		public int Count { get { return mItems.Count; }}

		/// <summary>
		/// Remove all items.
		/// </summary>
		public void Clear()
		{
			mItems.Clear();
		}

		/// <summary>
		/// True if the stack contains the given item.
		/// </summary>
		public bool Contains(T item)
		{
			return mItems.Contains(item);
		}

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator()
		{
			return mItems.GetEnumerator();
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Return the item at the top of the stack.
		/// </summary>
		public T Peek()
		{
			return mItems[Count - 1];
		}

		/// <summary>
		/// Removes and returns the item at the top of the stack.
		/// </summary>
		public T Pop()
		{
			T obj = Peek();
			mItems.RemoveAt(Count - 1);
			return obj;
		}

		/// <summary>
		/// Remove an arbitrary item from the stack.
		/// </summary>
		/// <param name="item">The item to be removed.</param>
		/// <returns>True if the item existed, false if not.</returns>
		public bool Remove(T item)
		{
			return mItems.Remove(item);
		}

		/// <summary>
		/// Push a new item onto the top of the stack.
		/// If the item already exists, it will be moved to the top.
		/// </summary>
		/// <param name="item">The item to be added.</param>
		public void Push(T item)
		{
			mItems.RemoveAll(x => x == item);
			mItems.Add(item);
		}

		/// <summary>
		/// Convert the current state of the stack to an array.
		/// </summary>
		public T[] ToArray()
		{
			return mItems.ToArray();
		}

		/// <summary>
		/// Sets the capacity to the actual number of elements in the stack, if that number is less than a threshold value.
		/// </summary>
		public void TrimExcess()
		{
			mItems.TrimExcess();
		}
	}
}
