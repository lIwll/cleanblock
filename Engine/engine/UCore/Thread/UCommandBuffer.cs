using System;
using System.Threading;

namespace UEngine
{
	public class UCommandBuffer< T >
	{
		private int mSpace = 0;
		public int Space
		{
			get { return mSpace; }
		}

		private int mAvailable = 0;
		public int Available
		{
			get { return mAvailable; }
		}

		private int mCapacity = 512;
		public int Capacity
		{
			get { return mCapacity; }
		}

		private int mTimeout = -1;
		public int Timeout
		{
			get { return mTimeout; }
			set { mTimeout = value; }
		}

		private int mRPos = 0; // read
		private int mWPos = 0; // write

		private T[] mBuffer;

		private object mBufferLock = new object();

#if USE_SEMAPHORE
		private Semaphore mSemaphore = new Semaphore(0, 1);
#endif

		public UCommandBuffer()
			: this(512)
		{
		}

		public UCommandBuffer(int capacity)
			: this(new T[capacity])
		{
		}

		public UCommandBuffer(T[] buffer)
		{
			mBuffer		= buffer;
			mCapacity	= buffer.Length;

			mAvailable	= 0;
			mSpace		= mCapacity;
			mRPos		= 0;
			mWPos		= 0;
		}

		public void Clear()
		{
			lock (mBufferLock)
			{
#if USE_SEMAPHORE
				if (mAvailable == 0)
					mSemaphore.Release();
#endif

				mAvailable	= 0;
				mSpace		= mCapacity;
				mRPos		= 0;
				mWPos		= 0;
			}
		}

#if OP_WITH_ALLOC_xxxx
		public bool Read(ref T cmd)
		{
			T[] cmds = new T[1];
			if (Read(cmds) > 0)
			{
				cmd = cmds[0];

				return true;
			}

			return false;
		}
#else
        public bool Read(ref T cmd)
        {
            #if USE_SEMAPHORE
			if (!mSemaphore.WaitOne(mTimeout, false))
				throw new ApplicationException("Read timeout.");
            #endif

            lock (mBufferLock)
            {
                if (mAvailable < 1)
                    return false;

                int data = mCapacity - mRPos;
                if (mRPos < mWPos || data >= 1)
                {
                    cmd = mBuffer[mRPos];
                    mRPos += 1;
                }
                else
                {
                    cmd = mBuffer[0];
                    mRPos = 1;
                }

                mSpace += 1;
                mAvailable -= 1;

            #if USE_SEMAPHORE
				if (mAvailable > 0)
					mSemaphore.Release();
            #endif

                return true;
            }
        }
#endif

		private int Read(T[] cmds)
		{
			return Read(cmds, 0, cmds.Length);
		}

		private int Read(T[] cmds, int offset, int size)
		{
#if USE_SEMAPHORE
			if (!mSemaphore.WaitOne(mTimeout, false))
				throw new ApplicationException("Read timeout.");
#endif

			lock (mBufferLock)
			{
				int read = (mAvailable >= size) ? size : mAvailable;
				if (read <= 0)
					return 0;

				int data = mCapacity - mRPos;
				if (mRPos < mWPos || data >= read)
				{
					Array.Copy(mBuffer, mRPos, cmds, offset, read);

					mRPos += read;
				} else
				{
					Array.Copy(mBuffer, mRPos, cmds, offset, data);

					mRPos = read - data;

					Array.Copy(mBuffer, 0, cmds, offset + data, mRPos);
				}

				mSpace += read;
				mAvailable -= read;

#if USE_SEMAPHORE
				if (mAvailable > 0)
					mSemaphore.Release();
#endif

				return read;
			}
		}

#if OP_WITH_ALLOC_xxxx
		public void Write(T cmd)
		{
			Write(new T[] { cmd }, 0, 1);
		}
#else
        public void Write(T cmd)
        {
            lock (mBufferLock)
            {
                if (mSpace < 1)
                    throw new ApplicationException("Not enough space.");

                int space = mCapacity - mWPos;
                if (mWPos < mRPos || space >= 1)
                {
                    mBuffer[mWPos] = cmd;

                    mWPos += 1;
                }
                else
                {
                    mBuffer[0] = cmd;
                    mWPos = 1;
                }

        #if USE_SEMAPHORE
				if (mAvailable == 0)
					mSemaphore.Release();
        #endif

                mSpace -= 1;
                mAvailable += 1;
            }
        }
#endif
		private void Write(T[] cmds)
		{
			Write(cmds, 0, cmds.Length);
		}

		private void Write(T[] cmds, int offset, int size)
		{
			lock (mBufferLock)
			{
				if (mSpace < size)
					throw new ApplicationException("Not enough space.");

				int space = mCapacity - mWPos;
				if (mWPos < mRPos || space >= size)
				{
					Array.Copy(cmds, offset, mBuffer, mWPos, size);

					mWPos += size;
				} else
				{
					Array.Copy(cmds, offset, mBuffer, mWPos, space);

					mWPos = size - space;

					Array.Copy(cmds, offset + space, mBuffer, 0, mWPos);
				}

#if USE_SEMAPHORE
				if (mAvailable == 0)
					mSemaphore.Release();
#endif

				mSpace -= size;
				mAvailable += size;
			}
		}
	}
}
