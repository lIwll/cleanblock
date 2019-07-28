using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UEngine
{
	public class UList< T > : IList< T >, ICollection< T >, IEnumerable< T >, IEnumerable, IList, ICollection, IDisposable
	{
		[Serializable]
		public struct UEnumerator : IEnumerator< T >, IDisposable, IEnumerator
		{
			private UList< T > mList;

			private int mNext;

			private int mVersion;

			private T mCurrent;
			public T Current
			{
				get { return mCurrent; }
			}

			object IEnumerator.Current
			{
				get
				{
					VerifyState();
					if (mNext <= 0)
						throw new InvalidOperationException();

					return mCurrent;
				}
			}

			internal UEnumerator(UList< T > l)
			{
				this = default(UEnumerator);

				mList		= l;
				mVersion	= l.mVersion;
			}

			public void Dispose()
			{
				mList = null;
			}

			private void VerifyState()
			{
				if (mList == null)
					throw new ObjectDisposedException(GetType().FullName);

				if (mVersion != mList.mVersion)
					throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}

			public bool MoveNext()
			{
				VerifyState();
				if (mNext < 0)
					return false;

				if (mNext < mList.mSize)
				{
					mCurrent = mList.mData[mNext ++];

					return true;
				}
				mNext = -1;

				return false;
			}

			void IEnumerator.Reset()
			{
				VerifyState();

				mNext = 0;
			}
		}

		private T[] mData;
		private int mSize;

		private int mVersion;

		private static readonly T[] kEmptyArray = new T[0];

		private static UArrayPool< T > mPool = new UArrayPool< T >();

		public int Count
		{
			get { return mSize; }
		}

		public T this[int index]
		{
			get
			{
				if ((uint)index >= (uint)mSize)
					throw new ArgumentOutOfRangeException("index");

				return mData[index];
			}
			set
			{
				CheckIndex(index);
				if (index == mSize)
					throw new ArgumentOutOfRangeException("index");

				mData[index] = value;
			}
		}

		bool ICollection< T >.IsReadOnly
		{
			get { return false; }
		}

		bool ICollection.IsSynchronized
		{
			get { return false; }
		}

		object ICollection.SyncRoot
		{
			get { return this; }
		}

		bool IList.IsFixedSize
		{
			get { return false; }
		}

		bool IList.IsReadOnly
		{
			get { return false; }
		}

		object IList.this[int index]
		{
			get
			{
				return this[index];
			}
			set
			{
				try
				{
					CheckIndex(index);
					if (index == mSize)
						throw new ArgumentOutOfRangeException("index");

					mData[index] = (T)value;

					return;
				} catch (NullReferenceException)
				{
				} catch (InvalidCastException)
				{
				}

				throw new ArgumentException("value");
			}
		}

		public UList()
		{
			mData = kEmptyArray;
		}

		public UList(IEnumerable< T > collection)
		{
			CheckCollection(collection);

			ICollection< T > _collection = collection as ICollection< T >;
			if (_collection == null)
			{
				mData = kEmptyArray;

				AddEnumerable(collection);
			} else
			{
				mData = mPool.Alloc(_collection.Count);

				AddCollection(_collection);
			}
		}

		public UList(int capacity)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException("capacity");

			mData = mPool.Alloc(capacity);
		}

		internal UList(T[] data, int size)
		{
			mData = data;
			mSize = size;
		}

		~UList()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (mData != kEmptyArray)
				mPool.Collect(mData);
			mData = kEmptyArray;
			mSize = 0;
		}

		public void Add(T value)
		{
			if (mSize == mData.Length)
				GrowIfNeeded(1);
			mData[mSize] = value;

			mSize ++;

			mVersion ++;
		}

		private void GrowIfNeeded(int count)
		{
			int i = mSize + count;
			if (i > mData.Length)
				mData = mPool.Grow(mData, mSize, i);
		}

		private void CheckRange(int idx, int count)
		{
			if (idx < 0)
				throw new ArgumentOutOfRangeException("index");

			if (count < 0)
				throw new ArgumentOutOfRangeException("count");

			if ((uint)(idx + count) > (uint)mSize)
				throw new ArgumentException("index and count exceed length of list");
		}

		private void AddCollection(ICollection< T > collection)
		{
			int count = collection.Count;
			if (count != 0)
			{
				GrowIfNeeded(count);

				collection.CopyTo(mData, mSize);

				mSize += count;
			}
		}

		private void AddEnumerable(IEnumerable< T > enumerable)
		{
			var iter = enumerable.GetEnumerator();
			while (iter.MoveNext())
				Add(iter.Current);
		}

		public void AddRange(IEnumerable< T > collection)
		{
			CheckCollection(collection);

			ICollection< T > _collection = collection as ICollection< T >;
			if (_collection != null)
				AddCollection(_collection);
			else
				AddEnumerable(collection);

			mVersion ++;
		}

		public void AddRange(UList< T > list, int length)
		{
			if (length < 0)
				throw new ArgumentOutOfRangeException("length");

			GrowIfNeeded(length);

			Array.Copy(list.mData, 0, mData, mSize, length);

			mSize += length;
		}

		public void AddRange(T[] array, int length)
		{
			if (length < 0)
				throw new ArgumentOutOfRangeException("length");

			GrowIfNeeded(length);

			Array.Copy(array, 0, mData, mSize, length);

			mSize += length;
		}

		public ReadOnlyCollection< T > AsReadOnly()
		{
			return new ReadOnlyCollection< T >(this);
		}

		public int BinarySearch(T value)
		{
			return Array.BinarySearch(mData, 0, mSize, value);
		}

		public int BinarySearch(T value, IComparer< T > comparer)
		{
			return Array.BinarySearch(mData, 0, mSize, value, comparer);
		}

		public int BinarySearch(int index, int count, T value, IComparer< T > comparer)
		{
			CheckRange(index, count);

			return Array.BinarySearch(mData, index, count, value, comparer);
		}

		public void Clear()
		{
			mSize = 0;

			mVersion ++;
		}

		public bool Contains(T value)
		{
			return Array.IndexOf(mData, value, 0, mSize) != -1;
		}

		public UList< K > ConvertAll< K >(Converter< T, K > converter)
		{
			if (converter == null)
				throw new ArgumentNullException("converter");

			UList< K > freeList = new UList< K >(mSize);
			for (int i = 0; i < mSize; i ++)
				freeList.mData[i] = converter(mData[i]);
			freeList.mSize = mSize;

			return freeList;
		}

		public void CopyTo(T[] array)
		{
			Array.Copy(mData, 0, array, 0, mSize);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			Array.Copy(mData, 0, array, arrayIndex, mSize);
		}

		public void CopyTo(int index, T[] array, int arrayIndex, int count)
		{
			CheckRange(index, count);

			Array.Copy(mData, index, array, arrayIndex, count);
		}

		public bool Exists(Predicate< T > match)
		{
			CheckMatch(match);

			return GetIndex(0, mSize, match) != -1;
		}

		public T Find(Predicate< T > match)
		{
			CheckMatch(match);

			int index = GetIndex(0, mSize, match);
			if (index == -1)
				return default(T);

			return mData[index];
		}

		private static void CheckMatch(Predicate< T > match)
		{
			if (match == null)
				throw new ArgumentNullException("match");
		}

		public UList< T > FindAll(Predicate< T > match)
		{
			CheckMatch(match);

			if (mSize <= 65536)
				return FindAllStackBits(match);

			return FindAllList(match);
		}

		protected unsafe UList< T > FindAllStackBits(Predicate< T > match)
		{
			uint* ptr1 = stackalloc uint[mSize / 32 + 1];
			uint* ptr2 = ptr1;

			int i1 = 0;
			uint i2 = 2147483648u;
			for (int i = 0; i < mSize; i ++)
			{
				if (match(mData[i]))
				{
					*ptr2 |= i2;

					i1 ++;
				}

				i2 >>= 1;
				if (i2 == 0)
				{
					ptr2 ++;
					i2 = 2147483648u;
				}
			}

			T[] array = mPool.Alloc(i1);
			i2 = 2147483648u;
			ptr2 = ptr1;

			int num3 = 0;
			for (int k = 0; k < mSize; k ++)
			{
				if (num3 >= i1)
					break;

				if ((*ptr2 & i2) == i2)
					array[num3 ++] = mData[k];

				i2 >>= 1;
				if (i2 == 0)
				{
					ptr2 ++;
					i2 = 2147483648u;
				}
			}

			return new UList< T >(array, i1);
		}

		protected UList< T > FindAllList(Predicate< T > match)
		{
			UList< T > freeList = new UList< T >();
			for (int i = 0; i < mSize; i ++)
			{
				if (match(mData[i]))
					freeList.Add(mData[i]);
			}

			return freeList;
		}

		public int FindIndex(Predicate< T > match)
		{
			CheckMatch(match);

			return GetIndex(0, mSize, match);
		}

		public int FindIndex(int startIndex, Predicate< T > match)
		{
			CheckMatch(match);
			CheckIndex(startIndex);

			return GetIndex(startIndex, mSize - startIndex, match);
		}

		public int FindIndex(int startIndex, int count, Predicate< T > match)
		{
			CheckMatch(match);
			CheckRange(startIndex, count);

			return GetIndex(startIndex, count, match);
		}

		private int GetIndex(int startIndex, int count, Predicate< T > match)
		{
			int k = startIndex + count;
			for (int i = startIndex; i < k; i ++)
			{
				if (match(mData[i]))
					return i;
			}

			return -1;
		}

		public T FindLast(Predicate< T > match)
		{
			CheckMatch(match);

			int lastIndex = GetLastIndex(0, mSize, match);
			if (lastIndex != -1)
				return this[lastIndex];

			return default(T);
		}

		public int FindLastIndex(Predicate< T > match)
		{
			CheckMatch(match);

			return GetLastIndex(0, mSize, match);
		}

		public int FindLastIndex(int startIndex, Predicate< T > match)
		{
			CheckMatch(match);
			CheckIndex(startIndex);

			return GetLastIndex(0, startIndex + 1, match);
		}

		public int FindLastIndex(int startIndex, int count, Predicate< T > match)
		{
			CheckMatch(match);

			int i = startIndex - count + 1;

			CheckRange(i, count);

			return GetLastIndex(i, count, match);
		}

		private int GetLastIndex(int startIndex, int count, Predicate< T > match)
		{
			int i = startIndex + count;
			while (i != startIndex)
			{
				if (match(mData[-- i]))
					return i;
			}

			return -1;
		}

		public void ForEach(Action< T > action)
		{
			if (action == null)
				throw new ArgumentNullException("action");

			for (int i = 0; i < mSize; i ++)
				action(mData[i]);
		}

		public UEnumerator GetEnumerator()
		{
			return new UEnumerator(this);
		}

		public UList< T > GetRange(int index, int count)
		{
			CheckRange(index, count);

			T[] array = mPool.Alloc(count);
			Array.Copy(mData, index, array, 0, count);

			return new UList< T >(array, count);
		}

		public int IndexOf(T value)
		{
			return Array.IndexOf(mData, value, 0, mSize);
		}

		public int IndexOf(T value, int index)
		{
			CheckIndex(index);

			return Array.IndexOf(mData, value, index, mSize - index);
		}

		public int IndexOf(T value, int index, int count)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException("index");

			if (count < 0)
				throw new ArgumentOutOfRangeException("count");

			if ((uint)(index + count) > (uint)mSize)
				throw new ArgumentOutOfRangeException("index and count exceed length of list");

			return Array.IndexOf(mData, value, index, count);
		}

		private void Shift(int start, int delta)
		{
			if (delta < 0)
				start -= delta;

			if (start < mSize)
				Array.Copy(mData, start, mData, start + delta, mSize - start);

			mSize += delta;

			if (delta < 0)
				Array.Clear(mData, mSize, -delta);
		}

		private void CheckIndex(int index)
		{
			if (index < 0 || (uint)index > (uint)mSize)
				throw new ArgumentOutOfRangeException("index");
		}

		public void Insert(int index, T value)
		{
			CheckIndex(index);

			if (mSize == mData.Length)
				GrowIfNeeded(1);

			Shift(index, 1);

			mData[index] = value;

			mVersion ++;
		}

		private void CheckCollection(IEnumerable< T > collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");
		}

		public void InsertRange(int index, IEnumerable< T > collection)
		{
			CheckCollection(collection);
			CheckIndex(index);

			if (collection == this)
			{
				T[] array = mPool.Alloc(mSize);

				CopyTo(array, 0);
				GrowIfNeeded(mSize);
				Shift(index, array.Length);

				Array.Copy(array, 0, mData, index, array.Length);

				mPool.Collect(array);
			} else
			{
				ICollection< T > _collection = collection as ICollection< T >;
				if (_collection != null)
					InsertCollection(index, _collection);
				else
					InsertEnumeration(index, collection);
			}

			mVersion ++;
		}

		private void InsertCollection(int index, ICollection< T > collection)
		{
			int count = collection.Count;

			GrowIfNeeded(count);
			Shift(index, count);

			collection.CopyTo(mData, index);
		}

		private void InsertEnumeration(int index, IEnumerable< T > enumerable)
		{
			foreach (T v in enumerable)
				Insert(index ++, v);
		}

		public int LastIndexOf(T value)
		{
			return Array.LastIndexOf(mData, value, mSize - 1, mSize);
		}

		public int LastIndexOf(T value, int index)
		{
			CheckIndex(index);

			return Array.LastIndexOf(mData, value, index, index + 1);
		}

		public int LastIndexOf(T value, int index, int count)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException("index", index, "index is negative");

			if (count < 0)
				throw new ArgumentOutOfRangeException("count", count, "count is negative");

			if (index - count + 1 < 0)
				throw new ArgumentOutOfRangeException("cound", count, "count is too large");

			return Array.LastIndexOf(mData, value, index, count);
		}

		public bool Remove(T value)
		{
			int i = IndexOf(value);
			if (i != -1)
				RemoveAt(i);

			return i != -1;
		}

		public int RemoveAll(Predicate< T > match)
		{
			CheckMatch(match);

			int i1 = 0;
			for (i1 = 0; i1 < mSize && !match(mData[i1]); i1 ++)
			{ }

			if (i1 == mSize)
				return 0;

			mVersion ++;

			int i2 = 0;
			for (i2 = i1 + 1; i2 < mSize; i2 ++)
			{
				if (!match(mData[i2]))
					mData[i1 ++] = mData[i2];
			}

			var delta = i2 - i1;
			if (delta > 0)
				Array.Clear(mData, i1, delta);

			mSize = i1;

			return delta;
		}

		public void RemoveAt(int index)
		{
			if (index < 0 || (uint)index >= (uint)mSize)
				throw new ArgumentOutOfRangeException("index");
			Shift(index, -1);

			mVersion ++;
		}

		public T Pop()
		{
			if (mSize <= 0)
				throw new InvalidOperationException();

			T result = mData[-- mSize];

			mData[mSize] = default(T);

			mVersion ++;

			return result;
		}

		public void RemoveRange(int index, int count)
		{
			CheckRange(index, count);
			if (count > 0)
			{
				Shift(index, -count);

				mVersion ++;
			}
		}

		public void Reverse()
		{
			Array.Reverse(mData, 0, mSize);

			mVersion ++;
		}

		public void Reverse(int index, int count)
		{
			CheckRange(index, count);

			Array.Reverse(mData, index, count);

			mVersion ++;
		}

		public void Sort()
		{
			Array.Sort(mData, 0, mSize, Comparer< T >.Default);

			mVersion ++;
		}

		public void Sort(IComparer< T > comparer)
		{
			Array.Sort(mData, 0, mSize, comparer);

			mVersion ++;
		}

		public void Sort(Comparison< T > comparison)
		{
			if (comparison == null)
				throw new ArgumentNullException("comparison");

			if (mSize > 1 && mData.Length > 1)
			{
				try
				{
					int lo = 0;
					int hi = mSize - 1;

					QSort(mData, lo, hi, comparison);
				} catch (Exception innerException)
				{
					throw new InvalidOperationException("Comparison threw an exception.", innerException);
				}

				mVersion ++;
			}
		}

		private static void QSort(T[] array, int lo0, int hi0, Comparison< T > comparison)
		{
			if (lo0 >= hi0)
				return;

			int i1 = lo0;
			int i2 = hi0;
			int i3 = i1 + (i2 - i1) / 2;

			T val = array[i3];
			while (true)
			{
				if (i1 < hi0 && comparison(array[i1], val) < 0)
				{
					i1 ++;

					continue;
				}

				while (i2 > lo0 && comparison(val, array[i2]) < 0)
					i2 --;

				if (i1 > i2)
					break;

				Swap(array, i1, i2);

				i1 ++;
				i2 --;
			}

			if (lo0 < i2)
				QSort(array, lo0, i2, comparison);

			if (i1 < hi0)
				QSort(array, i1, hi0, comparison);
		}

		private static void Swap(T[] array, int i, int k)
		{
			T val = array[i];

			array[i] = array[k];
			array[k] = val;
		}

		public void Sort(int index, int count, IComparer< T > comparer)
		{
			CheckRange(index, count);

			Array.Sort(mData, index, count, comparer);

			mVersion ++;
		}

		public T[] ToArray()
		{
			T[] array = new T[mSize];

			Array.Copy(mData, array, mSize);

			return array;
		}

		public T[] ToArraySelf()
		{
			if (mSize < mData.Length / 2)
			{
				T[] array = mPool.Alloc(mSize);

				Array.Copy(mData, array, mSize);

				mPool.Collect(mData);

				mData = array;
			}

			return mData;
		}

		public bool TrueForAll(Predicate< T > match)
		{
			CheckMatch(match);

			for (int i = 0; i < mSize; i ++)
			{
				if (!match(mData[i]))
					return false;
			}

			return true;
		}

		IEnumerator< T > IEnumerable< T >.GetEnumerator()
		{
			return GetEnumerator();
		}

		void ICollection.CopyTo(Array array, int arrayIndex)
		{
			Array.Copy(mData, 0, array, arrayIndex, mSize);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		int IList.Add(object value)
		{
			try
			{
				Add((T)value);

				return mSize - 1;
			} catch (NullReferenceException)
			{
			} catch (InvalidCastException)
			{
			}

			throw new ArgumentException("value");
		}

		bool IList.Contains(object value)
		{
			try
			{
				return Contains((T)value);
			} catch (NullReferenceException)
			{
			} catch (InvalidCastException)
			{
			}

			return false;
		}

		int IList.IndexOf(object value)
		{
			try
			{
				return IndexOf((T)value);
			} catch (NullReferenceException)
			{
			} catch (InvalidCastException)
			{
			}

			return -1;
		}

		void IList.Insert(int index, object value)
		{
			CheckIndex(index);
			try
			{
				Insert(index, (T)value);

				return;
			} catch (NullReferenceException)
			{
			} catch (InvalidCastException)
			{
			}

			throw new ArgumentException("value");
		}

		void IList.Remove(object value)
		{
			try
			{
				Remove((T)value);
			} catch (NullReferenceException)
			{
			} catch (InvalidCastException)
			{
			}
		}
	}
}
