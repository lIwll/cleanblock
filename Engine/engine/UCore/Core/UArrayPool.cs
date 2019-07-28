using System.Collections.Generic;

namespace UEngine
{
	public class UArrayPool< T >
	{
		public const int kMAX_COUNT = 16;

		private Queue< T[] >[] mPool = new Queue< T[] >[16];

		public UArrayPool()
		{
			for (int i = 0; i < 16; i ++)
				mPool[i] = new Queue< T[] >();
		}

		public int NextPowerOfTwo(int v)
		{
			v --;
			v |= v >> 16;
			v |= v >> 8;
			v |= v >> 4;
			v |= v >> 2;
			v |= v >> 1;

			return v + 1;
		}

		public T[] Alloc(int n)
		{
			int num = NextPowerOfTwo(n);

			int slot = GetSlot(num);
			if (slot >= 0 && slot < 16)
			{
				Queue< T[] > queue = mPool[slot];
				if (queue.Count > 0)
					return queue.Dequeue();

				return new T[num];
			}

			return new T[n];
		}

		public T[] Grow(T[] item, int size, int num)
		{
			T[] array = Alloc(num);

			System.Array.Copy(item, array, size); Collect(item);

			return array;
		}

		public void Collect(T[] buffer)
		{
			if (buffer != null)
			{
				int slot = GetSlot(buffer.Length);
				if (slot >= 0 && slot < 16)
					mPool[slot].Enqueue(buffer);
			}
		}

		private int GetSlot(int value)
		{
			int slot = 0;
			while (value > 0)
			{
				slot ++;

				value >>= 1;
			}

			return slot;
		}
	}
}
