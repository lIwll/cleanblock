#define UNSAFE_BYTEBUFFER

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;

namespace UEngine
{
	public class UDisposable : IDisposable
	{
		~UDisposable()
        {
            Dispose(false);
        }

        public virtual void Dispose()
        {
			GC.SuppressFinalize(this);

            Dispose(true);
        }

        public virtual void Dispose(bool releaseRes)
        {
        }
	}

	public abstract class UByteBufferAllocator : UDisposable
	{
        public unsafe byte* Buffer
        {
            get;
            protected set;
        }

		public int Length
		{
			get;
			protected set;
		}

		public abstract byte[] ByteArray
		{
			get;
		}

		public abstract void Grow(int newSize);
	}

	public class UByteArrayAllocator : UByteBufferAllocator
	{
		private byte[] mBuffer;

		public override byte[] ByteArray
		{
			get { return mBuffer; }
		}

        private GCHandle mHandle;

		public UByteArrayAllocator(byte[] buffer)
		{
			Init(buffer);
		}

		public override void Grow(int newSize)
		{
			if ((Length & 0xC0000000) != 0)
				throw new Exception("ByteBuffer: cannot grow buffer beyond 2 gigabytes.");

			if (newSize < Length)
				throw new Exception("ByteBuffer: cannot truncate buffer.");

			byte[] newBuffer = new byte[newSize];

			System.Buffer.BlockCopy(mBuffer, 0, newBuffer, newSize - Length, Length);

			Init(newBuffer);
		}

		public override void Dispose(bool releaseRes)
		{
            if (mHandle.IsAllocated)
                mHandle.Free();
		}

		private void Init(byte[] buffer)
		{
			mBuffer = buffer;
			Length	= mBuffer.Length;

            if (mHandle.IsAllocated)
                mHandle.Free();
            mHandle = GCHandle.Alloc(mBuffer, GCHandleType.Pinned);
            unsafe
            {
                Buffer = (byte*)mHandle.AddrOfPinnedObject().ToPointer();
            }
		}
	}

	public class UByteBuffer : UDisposable
	{
		private static Dictionary< Type, int > msGenericSizes = new Dictionary< Type, int >()
        {
            { typeof(bool),			sizeof(bool)		},
            { typeof(float),		sizeof(float)		},
            { typeof(double),		sizeof(double)		},
            { typeof(sbyte),		sizeof(sbyte)		},
            { typeof(byte),			sizeof(byte)		},
            { typeof(short),		sizeof(short)		},
            { typeof(ushort),		sizeof(ushort)		},
            { typeof(int),			sizeof(int)			},
            { typeof(uint),			sizeof(uint)		},
            { typeof(long),			sizeof(long)		},
            { typeof(ulong),		sizeof(ulong)		},
            { typeof(Vector2),		sizeof(float) * 2	},
            { typeof(Vector3),		sizeof(float) * 3	},
            { typeof(Vector4),		sizeof(float) * 4	},
            { typeof(Quaternion),	sizeof(float) * 4	},
            { typeof(Matrix4x4),	sizeof(float) * 16	},
            { typeof(Color),		sizeof(float) * 4	},
            { typeof(Color32),		sizeof(byte) * 4	},
        };

		UByteBufferAllocator mAllocator;

        int mPosition;
		public int Position
		{
			get { return mPosition; }
			set { mPosition = value; }
		}

		public int Length
		{
			get { return mAllocator.Length; }
		}

		public UByteBuffer(UByteBufferAllocator allocator, int position)
        {
            mAllocator	= allocator;
            mPosition	= position;
        }

        public UByteBuffer(int size)
			: this(new byte[size])
		{ }

        public UByteBuffer(byte[] buffer)
			: this(buffer, 0)
		{ }

        public UByteBuffer(byte[] buffer, int position)
        {
            mAllocator	= new UByteArrayAllocator(buffer);
            mPosition	= position;
        }

		public static int SizeOf< T >()
		{
			return msGenericSizes[typeof(T)];
		}

		public static bool IsSupportedType< T >()
		{
			return msGenericSizes.ContainsKey(typeof(T));
		}

		public static int ArraySize< T >(T[] x)
		{
			return SizeOf< T >() * x.Length;
		}

		public static ushort ReverseBytes(ushort v)
		{
			return (ushort)(((v & 0x00FFU) << 8) | ((v & 0xFF00U) >> 8));
		}

		public static uint ReverseBytes(uint v)
		{
			return ((v & 0x000000FFU) << 24) | ((v & 0x0000FF00U) << 8) | ((v & 0x00FF0000U) >> 8) | ((v & 0xFF000000U) >> 24);
		}

		public static ulong ReverseBytes(ulong v)
		{
			return (((v & 0x00000000000000FFUL) << 56) | ((v & 0x000000000000FF00UL) << 40) | ((v & 0x0000000000FF0000UL) << 24) | ((v & 0x00000000FF000000UL) << 8) |
					((v & 0x000000FF00000000UL) >> 8) | ((v & 0x0000FF0000000000UL) >> 24) | ((v & 0x00FF000000000000UL) >> 40) | ((v & 0xFF00000000000000UL) >> 56));
		}

		public override void Dispose(bool releaseRes)
        {
            if (mAllocator != null)
                mAllocator.Dispose();
			mAllocator = null;
        }

		public void Reset()
		{
			mPosition = 0;
		}

		public UByteBuffer Duplicate()
		{
			return new UByteBuffer(mAllocator, mPosition);
		}

		public void Grow(int newSize)
		{
			if (null != mAllocator)
				mAllocator.Grow(newSize);
		}

		public byte[] ToArray(int pos, int len)
		{
			return ToArray< byte >(pos, len);
		}

		public T[] ToArray< T >(int pos, int len) where T : struct
		{
			AssertOffsetAndLength(pos, len);

			T[] arr = new T[len];

			Buffer.BlockCopy(mAllocator.ByteArray, pos, arr, 0, ArraySize(arr));

			return arr;
		}

		public byte[] ToSizedArray()
		{
			return ToArray< byte >(mPosition, Length - mPosition);
		}

		public byte[] ToFullArray()
		{
			return ToArray< byte >(0, Length);
		}

		public ArraySegment< byte > ToArraySegment(int pos, int len)
		{
			return new ArraySegment< byte >(mAllocator.ByteArray, pos, len);
		}

		public MemoryStream ToMemoryStream(int pos, int len)
		{
			return new MemoryStream(mAllocator.ByteArray, pos, len);
		}

		// write value
		public unsafe void WriteB8(int offset, byte value)
		{
			AssertOffsetAndLength(offset, sizeof(byte));

			mAllocator.Buffer[offset] = value;
		}

		public unsafe void WriteB8(int offset, byte value, int count)
		{
			AssertOffsetAndLength(offset, sizeof(byte) * count);

			for (var i = 0; i < count; ++ i)
				mAllocator.Buffer[offset + i] = value;
		}

		public unsafe void WriteS8(int offset, sbyte value)
		{
			AssertOffsetAndLength(offset, sizeof(sbyte));

			mAllocator.Buffer[offset] = (byte)value;
		}

		public void WriteI16(int offset, short value)
		{
			WriteU16(offset, (ushort)value);
		}

		public unsafe void WriteU16(int offset, ushort value)
		{
			AssertOffsetAndLength(offset, sizeof(ushort));

			byte* ptr = mAllocator.Buffer;

			*(ushort*)(ptr + offset) = BitConverter.IsLittleEndian ? value : ReverseBytes(value);
		}

		public void WriteI32(int offset, int value)
		{
			WriteU32(offset, (uint)value);
		}

		public unsafe void WriteU32(int offset, uint value)
		{
			AssertOffsetAndLength(offset, sizeof(uint));

			byte* ptr = mAllocator.Buffer;

			*(uint*)(ptr + offset) = BitConverter.IsLittleEndian ? value : ReverseBytes(value);
		}

		public unsafe void WriteI64(int offset, long value)
		{
			WriteU64(offset, (ulong)value);
		}

		public unsafe void WriteU64(int offset, ulong value)
		{
			AssertOffsetAndLength(offset, sizeof(ulong));

			byte* ptr = mAllocator.Buffer;

			*(ulong*)(ptr + offset) = BitConverter.IsLittleEndian ? value : ReverseBytes(value);
		}

		public unsafe void WriteF32(int offset, float value)
		{
			AssertOffsetAndLength(offset, sizeof(float));

			byte* ptr = mAllocator.Buffer;

			ULogger.Error("write f32 0");

			if (BitConverter.IsLittleEndian)
			{
				ULogger.Error("write f32 1 {0}, {1}", offset, value);
				ULogger.Error("write f32 2 {0}", *(float*)(ptr + offset));

				*(float*)(ptr + offset) = value;
			} else
			{
				*(uint*)(ptr + offset) = ReverseBytes(*(uint*)(&value));
			}

			ULogger.Error("write f32 3");
		}

		public unsafe void WriteF64(int offset, double value)
		{
			AssertOffsetAndLength(offset, sizeof(double));

			byte* ptr = mAllocator.Buffer;

			if (BitConverter.IsLittleEndian)
				*(double*)(ptr + offset) = value;
			else
				*(ulong*)(ptr + offset) = ReverseBytes(*(ulong*)(ptr + offset));
		}

		public void WriteString(int offset, string value)
		{
			AssertOffsetAndLength(offset, value.Length);

			Encoding.UTF8.GetBytes(value, 0, value.Length, mAllocator.ByteArray, offset);
		}

		public int WriteArray< T >(int offset, T[] x) where T : struct
		{
			if (x == null)
				throw new ArgumentNullException("Cannot put a null array");

			if (x.Length == 0)
				throw new ArgumentException("Cannot put an empty array");

			if (!IsSupportedType< T >())
				throw new ArgumentException("Cannot put an array of type " + typeof(T) + " into this buffer");

			if (BitConverter.IsLittleEndian)
			{
				int numBytes = UByteBuffer.ArraySize(x);

				offset -= numBytes;

				AssertOffsetAndLength(offset, numBytes);

				Buffer.BlockCopy(x, 0, mAllocator.ByteArray, offset, numBytes);
			} else
			{
				throw new NotImplementedException("Big Endian Support not implemented yet for putting typed arrays");
			}

			return offset;
		}

		// read value
        public unsafe byte ReadB8(int offset)
        {
            AssertOffsetAndLength(offset, sizeof(byte));

            return mAllocator.Buffer[offset];
        }

        public unsafe sbyte ReadS8(int offset)
        {
            AssertOffsetAndLength(offset, sizeof(sbyte));

            return (sbyte)mAllocator.Buffer[offset];
        }

		public short ReadI16(int offset)
		{
			return (short)ReadU16(offset);
		}

		public unsafe ushort ReadU16(int offset)
		{
			AssertOffsetAndLength(offset, sizeof(ushort));

			byte* ptr = mAllocator.Buffer;
			{
				return BitConverter.IsLittleEndian ? *(ushort*)(ptr + offset) : ReverseBytes(*(ushort*)(ptr + offset));
			}
		}

		public int ReadI32(int offset)
		{
			return (int)ReadU32(offset);
		}

		public unsafe uint ReadU32(int offset)
		{
			AssertOffsetAndLength(offset, sizeof(uint));

			byte* ptr = mAllocator.Buffer;
			{
				return BitConverter.IsLittleEndian ? *(uint*)(ptr + offset) : ReverseBytes(*(uint*)(ptr + offset));
			}
		}

		public long ReadI64(int offset)
		{
			return (long)ReadU64(offset);
		}

		public unsafe ulong ReadU64(int offset)
		{
			AssertOffsetAndLength(offset, sizeof(ulong));

			byte* ptr = mAllocator.Buffer;
			{
				return BitConverter.IsLittleEndian ? *(ulong*)(ptr + offset) : ReverseBytes(*(ulong*)(ptr + offset));
			}
		}

		public unsafe float ReadF32(int offset)
		{
			AssertOffsetAndLength(offset, sizeof(float));

			byte* ptr = mAllocator.Buffer;
			{
				if (BitConverter.IsLittleEndian)
				{
					return *(float*)(ptr + offset);
				} else
				{
					uint uvalue = ReverseBytes(*(uint*)(ptr + offset));

					return *(float*)(&uvalue);
				}
			}
		}

		public unsafe double ReadF64(int offset)
		{
			AssertOffsetAndLength(offset, sizeof(double));

			byte* ptr = mAllocator.Buffer;
			{
				if (BitConverter.IsLittleEndian)
				{
					return *(double*)(ptr + offset);
				} else
				{
					ulong uvalue = ReverseBytes(*(ulong*)(ptr + offset));

					return *(double*)(&uvalue);
				}
			}
		}

		public string ReadString(int startPos, int len)
		{
			return Encoding.UTF8.GetString(mAllocator.ByteArray, startPos, len);
		}

		public T[] ReadArray< T >(ref int offset, int size) where T : struct
		{
			if (!IsSupportedType< T >())
				throw new ArgumentException("Cannot put an array of type " + typeof(T) + " into this buffer");

			int numBytes = size * SizeOf< T >();

			T[] data = new T[numBytes];

			if (BitConverter.IsLittleEndian)
			{
				offset -= numBytes;

				Buffer.BlockCopy(mAllocator.ByteArray, offset, data, 0, numBytes);
			} else
			{
				throw new NotImplementedException("Big Endian Support not implemented yet for putting typed arrays");
			}

			return data;
		}

		void AssertOffsetAndLength(int offset, int length)
		{
			if (offset < 0 || offset > mAllocator.Length - length)
				throw new ArgumentOutOfRangeException();
		}
	}
}
