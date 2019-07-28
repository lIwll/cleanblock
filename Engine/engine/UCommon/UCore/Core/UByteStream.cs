using System;
using System.Text;
using System.Collections.Generic;

namespace UEngine
{
	public class UByteStream
	{
		private UByteBuffer mBuffer;
		public UByteBuffer DataBuffer { get { return mBuffer; } }

		private int mSize;
		private int mMinAlign = 1;

		private int[] mVTable = new int[16];
		private int mVTableSize = -1;

		private int mObjectStart;

		private int[] mVTables = new int[16];
		private int mNumVTable = 0;

		private int mVectorNumElems = 0;

		public const int kFileIdentifierLength = 4;

		public UByteStream(int initialSize)
		{
			if (initialSize <= 0)
				throw new ArgumentOutOfRangeException("initialSize", initialSize, "Must be greater than zero");

			mSize	= initialSize;
			mBuffer = new UByteBuffer(initialSize);
		}

		public UByteStream(byte[] buffer)
		{
			mBuffer = new UByteBuffer(buffer);
			mSize	= buffer.Length;
		}

		public UByteStream(UByteBuffer buffer)
		{
			mBuffer = buffer;
			mSize	= buffer.Length;

			buffer.Reset();
		}

		public void Clear()
		{
			mSize = mBuffer.Length;
			mBuffer.Reset();

			mMinAlign = 1;
			while (mVTableSize > 0)
				mVTable[-- mVTableSize] = 0;
			mVTableSize = -1;

			mObjectStart	= 0;
			mNumVTable		= 0;
			mVectorNumElems = 0;
		}

		public int Offset
		{
			get { return mBuffer.Length - mSize; }
		}

		public void Pad(int size)
		{
			mBuffer.WriteB8(mSize -= size, 0, size);
		}

		void Grow()
		{
			mBuffer.Grow(mBuffer.Length << 1);
		}

		public void Prep(int size, int additionalBytes)
		{
			if (size > mMinAlign)
				mMinAlign = size;

			var alignSize = ((~(mBuffer.Length - mSize + additionalBytes)) + 1) & (size - 1);
			while (mSize < alignSize + size + additionalBytes)
			{
				var oldBufSize = mBuffer.Length;
				Grow();
				mSize += (int)mBuffer.Length - oldBufSize;
			}

			if (alignSize > 0)
				Pad(alignSize);
		}

		public int AddBool_Fast(bool x)
		{
			mBuffer.WriteB8(mSize -= sizeof(byte), (byte)(x ? 1 : 0));

			return mSize;
		}

		public int AddB8_Fast(byte x)
		{
			mBuffer.WriteB8(mSize -= sizeof(byte), x);

			return mSize;
		}

		public int AddS8_Fast(sbyte x)
		{
			mBuffer.WriteS8(mSize -= sizeof(sbyte), x);

			return mSize;
		}

		public int AddI16_Fast(short x)
		{
			mBuffer.WriteI16(mSize -= sizeof(short), x);

			return mSize;
		}

		public int AddU16_Fast(ushort x)
		{
			mBuffer.WriteU16(mSize -= sizeof(ushort), x);

			return mSize;
		}

		public int AddI32_Fast(int x)
		{
			mBuffer.WriteI32(mSize -= sizeof(int), x);

			return mSize;
		}

		public int AddU32_Fast(uint x)
		{
			mBuffer.WriteU32(mSize -= sizeof(uint), x);

			return mSize;
		}

		public int AddI64_Fast(long x)
		{
			mBuffer.WriteI64(mSize -= sizeof(long), x);

			return mSize;
		}

		public int AddU64_Fast(ulong x)
		{
			mBuffer.WriteU64(mSize -= sizeof(ulong), x);

			return mSize;
		}

		public int AddF32_Fast(float x)
		{
			mBuffer.WriteF32(mSize -= sizeof(float), x);

			return mSize;
		}

		public int AddF64_Fast(double x)
		{
			mBuffer.WriteF64(mSize -= sizeof(double), x);

			return mSize;
		}

		public int Add_Fast< T >(T[] x) where T : struct
		{
			mSize = mBuffer.WriteArray(mSize, x);

			return mSize;
		}

		public int AddBool(bool x)		{ Prep(sizeof(byte), 0);		return AddBool_Fast(x); }

		public int AddB8(byte x)		{ Prep(sizeof(byte), 0);		return AddB8_Fast(x); }

		public int AddS8(sbyte x)		{ Prep(sizeof(sbyte), 0);		return AddS8_Fast(x); }

		public int AddI16(short x)		{ Prep(sizeof(short), 0);		return AddI16_Fast(x); }

		public int AddU16(ushort x)		{ Prep(sizeof(ushort), 0);		return AddU16_Fast(x); }

		public int AddI32(int x)		{ Prep(sizeof(int), 0);			return AddI32_Fast(x); }

		public int AddU32(uint x)		{ Prep(sizeof(uint), 0);		return AddU32_Fast(x); }

		public int AddI64(long x)		{ Prep(sizeof(long), 0);		return AddI64_Fast(x); }

		public int AddU64(ulong x)		{ Prep(sizeof(ulong), 0);		return AddU64_Fast(x); }

		public int AddF32(float x)		{ Prep(sizeof(float), 0);		return AddF32_Fast(x); }

		public int AddF64(double x)		{ Prep(sizeof(double), 0);		return AddF64_Fast(x); }

		public int Add< T >(T[] x) where T : struct
		{
			if (x == null)
				throw new ArgumentNullException("UByteStream: Cannot add a null array");

			if (x.Length == 0)
				return mSize;

			if (!UByteBuffer.IsSupportedType< T >())
				throw new ArgumentException("UByteStream: Cannot add this Type array to the builder");

			AddI32(x.Length);

			int size = UByteBuffer.SizeOf< T >();

			Prep(size + sizeof(int), size * (x.Length - 1));

			return Add_Fast(x);
		}

		public int AddString(string s)
		{
			var len = Encoding.UTF8.GetByteCount(s);

			AddI32(len);

			Prep(sizeof(sbyte), len);

			mBuffer.WriteString(mSize -= len, s);

			return mSize;

//			return AddI64(UStringTable.AddString(s));
		}

		public int AddOffset(int off)
		{
			Prep(sizeof(int), 0);

			if (off > Offset)
				throw new ArgumentException();

			off = Offset - off + sizeof(int);

			return AddI32_Fast(off);
		}

		public int GetPosition()
		{
			return mSize;
		}

		public int GetPosition(int off)
		{
			return mBuffer.Length - off;
		}

		public int GetOffset()
		{
			return Offset;
		}

		public int GetOffset(int off)
		{
			return mBuffer.Length - off;
		}

		public void SetBool(int pos, bool x)	{ mBuffer.WriteB8(pos, (byte)(x ? 1 : 0)); }

		public void SetB8(int pos, byte x)		{ mBuffer.WriteB8(pos, x); }

		public void SetS8(int pos, sbyte x)		{ mBuffer.WriteS8(pos, x); }

		public void SetI16(int pos, short x)	{ mBuffer.WriteI16(pos, x); }

		public void SetU16(int pos, ushort x)	{ mBuffer.WriteU16(pos, x); }

		public void SetI32(int pos, int x)		{ mBuffer.WriteI32(pos, x); }

		public void SetU32(int pos, uint x)		{ mBuffer.WriteU32(pos, x); }

		public void SetI64(int pos, long x)		{ mBuffer.WriteI64(pos, x); }

		public void SetU64(int pos, ulong x)	{ mBuffer.WriteU64(pos, x); }

		public void SetF32(int pos, float x)	{ mBuffer.WriteF32(pos, x); }

		public void SeteF64(int pos, double x)	{ mBuffer.WriteF64(pos, x); }

		public bool GetBool()
		{
			Prep(sizeof(bool), 0);

			return (mBuffer.ReadB8(mSize -= sizeof(byte)) == 1) ? true : false;
		}

		public byte GetB8()
		{
			Prep(sizeof(byte), 0);

			return mBuffer.ReadB8(mSize -= sizeof(byte));
		}

		public sbyte GetS8()
		{
			Prep(sizeof(sbyte), 0);

			return mBuffer.ReadS8(mSize -= sizeof(sbyte));
		}

		public short GetI16()
		{
			Prep(sizeof(short), 0);

			return mBuffer.ReadI16(mSize -= sizeof(short));
		}

		public ushort GetU16()
		{
			Prep(sizeof(ushort), 0);

			return mBuffer.ReadU16(mSize -= sizeof(ushort));
		}

		public int GetI32()
		{
			Prep(sizeof(int), 0);

			return mBuffer.ReadI32(mSize -= sizeof(int));
		}

		public uint GetU32()
		{
			Prep(sizeof(uint), 0);

			return mBuffer.ReadU32(mSize -= sizeof(uint));
		}

		public long GetI64()
		{
			Prep(sizeof(long), 0);

			return mBuffer.ReadI64(mSize -= sizeof(long));
		}

		public ulong GetU64()
		{
			Prep(sizeof(ulong), 0);

			return mBuffer.ReadU64(mSize -= sizeof(ulong));
		}

		public float GetF32()
		{
			Prep(sizeof(float), 0);

			return mBuffer.ReadF32(mSize -= sizeof(float));
		}

		public double GetF64()
		{
			Prep(sizeof(double), 0);

			return mBuffer.ReadF64(mSize -= sizeof(double));
		}

		public T[] Get< T >() where T : struct
		{
			return mBuffer.ReadArray< T >(ref mSize, GetI32());
		}

		public T[] Get_Fast< T >(int num) where T : struct
		{
			return mBuffer.ReadArray< T >(ref mSize, num);
		}

		public string GetString()
		{
			var len = GetI32();

			Prep(sizeof(sbyte), len);

			return mBuffer.ReadString(mSize -= len, len);
/*
			var hash = GetI64();

			return UStringTable.GetString(hash);
*/
		}

		public void StartVector(int elemSize, int count, int alignment)
		{
			NotNested();

			mVectorNumElems = count;
			Prep(sizeof(int), elemSize * count);
			Prep(alignment, elemSize * count);
		}

		public int EndVector()
		{
			AddI32_Fast(mVectorNumElems);

			return Offset;
		}

		public int CreateVectorOfTables(int[] offsets)
		{
			NotNested();

			StartVector(sizeof(int), offsets.Length, sizeof(int));

			for (int i = offsets.Length - 1; i >= 0; i --)
				AddOffset(offsets[i]);

			return EndVector();
		}

		public int CreateString(string s)
		{
			NotNested();

			AddB8(0);

			var utf8StringLen = Encoding.UTF8.GetByteCount(s);

			StartVector(1, utf8StringLen, 1);

			mBuffer.WriteString(mSize -= utf8StringLen, s);

			return EndVector();
		}

		// object
		public void StartObject(int numfields)
		{
			if (numfields < 0)
				throw new ArgumentOutOfRangeException("UByteStream: invalid numfields");

			NotNested();

			if (mVTable.Length < numfields)
				mVTable = new int[numfields];
			mVTableSize = numfields;

			mObjectStart = Offset;
		}

		public void Slot(int offset)
		{
			if (offset >= mVTableSize)
				throw new IndexOutOfRangeException("UByteStream: invalid voffset");

			mVTable[offset] = Offset;
		}

		public void AddBool(int o, bool x, bool d)		{ if (x != d) { AddBool(x); Slot(o); } }

		public void AddB8(int o, byte x, byte d)		{ if (x != d) { AddB8(x); Slot(o); } }

		public void AddS8(int o, sbyte x, sbyte d)		{ if (x != d) { AddS8(x); Slot(o); } }

		public void AddI16(int o, short x, int d)		{ if (x != d) { AddI16(x); Slot(o); } }

		public void AddU16(int o, ushort x, ushort d)	{ if (x != d) { AddU16(x); Slot(o); } }

		public void AddI32(int o, int x, int d)			{ if (x != d) { AddI32(x); Slot(o); } }

		public void AddU32(int o, uint x, uint d)		{ if (x != d) { AddU32(x); Slot(o); } }

		public void AddI64(int o, long x, long d)		{ if (x != d) { AddI64(x); Slot(o); } }

		public void AddU64(int o, ulong x, ulong d)		{ if (x != d) { AddU64(x); Slot(o); } }

		public void AddF32(int o, float x, double d)	{ if (x != d) { AddF32(x); Slot(o); } }

		public void AddF64(int o, double x, double d)	{ if (x != d) { AddF64(x); Slot(o); } }

		public void AddOffset(int o, int x, int d)		{ if (x != d) { AddOffset(x); Slot(o); } }

		public void AddStruct(int offset, int x, int d)
		{
			if (x != d)
			{
				Nested(x);

				Slot(offset);
			}
		}

		public int EndObject()
		{
			if (mVTableSize < 0)
				throw new InvalidOperationException("UByteStream: calling endObject without a startObject");

			AddI32((int)0);

			var vtableloc = Offset;

			int i = mVTableSize - 1;
			for (; i >= 0 && mVTable[i] == 0; i --)
			{ }

			int trimmedSize = i + 1;
			for (; i >= 0; i --)
			{
				short off = (short)(mVTable[i] != 0 ? vtableloc - mVTable[i] : 0);

				AddI16(off);

				mVTable[i] = 0;
			}

			const int standardFields = 2;
			AddI16((short)(vtableloc - mObjectStart));
			AddI16((short)((trimmedSize + standardFields) * sizeof(short)));

			int existingVtable = 0;
			for (i = 0; i < mNumVTable; i ++)
			{
				int vt1 = mBuffer.Length - mVTables[i];
				int vt2 = mSize;
				short len = mBuffer.ReadI16(vt1);
				if (len == mBuffer.ReadI16(vt2))
				{
					for (int k = sizeof(short); k < len; k += sizeof(short))
					{
						if (mBuffer.ReadI16(vt1 + k) != mBuffer.ReadI16(vt2 + k))
							goto endLoop;
					}
					existingVtable = mVTables[i];

					break;
				}

endLoop: 
				{ }
			}

			if (existingVtable != 0)
			{
				mSize = mBuffer.Length - vtableloc;

				mBuffer.WriteI32(mSize, existingVtable - vtableloc);
			} else
			{
				if (mNumVTable == mVTables.Length)
				{
					var newvtables = new int[mNumVTable * 2];

					Array.Copy(mVTables, newvtables, mVTables.Length);

					mVTables = newvtables;
				}
				mVTables[mNumVTable ++] = Offset;

				mBuffer.WriteI32(mBuffer.Length - vtableloc, Offset - vtableloc);
			}

			mVTableSize = -1;

			return vtableloc;
		}

		public void Required(int table, int field)
		{
			int table_start = mBuffer.Length - table;

			int vtable_start = table_start - mBuffer.ReadI32(table_start);
			bool ok = mBuffer.ReadI16(vtable_start + field) != 0;
			if (!ok)
				throw new InvalidOperationException("UbyteStream: field " + field + " must be set");
		}

		public void Finish(bool sizePrefix = false)
		{
			Prep(mMinAlign, sizeof(int) + (sizePrefix ? sizeof(int) : 0));

			if (sizePrefix)
				AddI32(mBuffer.Length - mSize);
			mBuffer.Position = mSize;
		}

		public void Finish(int rootTable, bool sizePrefix = false)
		{
			Prep(mMinAlign, sizeof(int) + (sizePrefix ? sizeof(int) : 0));

			AddOffset(rootTable);
			if (sizePrefix)
				AddI32(mBuffer.Length - mSize);
			mBuffer.Position = mSize;
		}

		public void Finish(string fileIdentifier, bool sizePrefix = false)
		{
			Prep(mMinAlign, sizeof(int) + (sizePrefix ? sizeof(int) : 0) + kFileIdentifierLength);
			if (fileIdentifier.Length != kFileIdentifierLength)
				throw new ArgumentException("UByteStream: file identifier must be length " + kFileIdentifierLength, "fileIdentifier");

			for (int i = kFileIdentifierLength - 1; i >= 0; i--)
				AddB8((byte)fileIdentifier[i]);

			Finish(sizePrefix);
		}

		public void Finish(int rootTable, string fileIdentifier, bool sizePrefix = false)
		{
			Prep(mMinAlign, sizeof(int) + (sizePrefix ? sizeof(int) : 0) + kFileIdentifierLength);
			if (fileIdentifier.Length != kFileIdentifierLength)
				throw new ArgumentException("UByteStream: file identifier must be length " + kFileIdentifierLength, "fileIdentifier");

			for (int i = kFileIdentifierLength - 1; i >= 0; i --)
				AddB8((byte)fileIdentifier[i]);

			Finish(rootTable, sizePrefix);
		}

		public void Finish(byte[] fileIdentifier, bool sizePrefix = false)
		{
			Prep(mMinAlign, sizeof(int) + (sizePrefix ? sizeof(int) : 0) + kFileIdentifierLength);
			if (fileIdentifier.Length != kFileIdentifierLength)
				throw new ArgumentException("UByteStream: file identifier must be length " + kFileIdentifierLength, "fileIdentifier");

			for (int i = kFileIdentifierLength - 1; i >= 0; i --)
				AddB8((byte)fileIdentifier[i]);

			Finish(sizePrefix);
		}

		public void Finish(int rootTable, byte[] fileIdentifier, bool sizePrefix = false)
		{
			Prep(mMinAlign, sizeof(int) + (sizePrefix ? sizeof(int) : 0) + kFileIdentifierLength);
			if (fileIdentifier.Length != kFileIdentifierLength)
				throw new ArgumentException("UByteStream: file identifier must be length " + kFileIdentifierLength, "fileIdentifier");

			for (int i = kFileIdentifierLength - 1; i >= 0; i --)
				AddB8((byte)fileIdentifier[i]);

			Finish(rootTable, sizePrefix);
		}

		public byte[] SizedByteArray()
		{
			return mBuffer.ToSizedArray();
		}

		void Nested(int obj)
		{
			if (obj != Offset)
				throw new Exception("UByteStream: struct must be serialized inline.");
		}

		void NotNested()
		{
			if (mVTableSize >= 0)
				throw new Exception("UByteStream: object serialization must not be nested.");
		}
	}
}
