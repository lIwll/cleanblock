using System;

using ULuaState = System.IntPtr;

namespace UEngine.ULua
{
    public static partial class UEncoder
    {
        public static bool Encode(IntPtr buff, int offset, byte field)
        {
            return ULuaAPI.luaex_pack_int8_t(buff, offset, field);
        }

        public static bool Decode(IntPtr buff, int offset, out byte field)
        {
            return ULuaAPI.luaex_unpack_int8_t(buff, offset, out field);
        }

        public static bool Encode(IntPtr buff, int offset, sbyte field)
        {
            return ULuaAPI.luaex_pack_int8_t(buff, offset, (byte)field);
        }

        public static bool Decode(IntPtr buff, int offset, out sbyte field)
        {
            byte tfield;
            bool ret = ULuaAPI.luaex_unpack_int8_t(buff, offset, out tfield);
            field = (sbyte)tfield;

            return ret;
        }

        public static bool Encode(IntPtr buff, int offset, short field)
        {
            return ULuaAPI.luaex_pack_int16_t(buff, offset, field);
        }

        public static bool Decode(IntPtr buff, int offset, out short field)
        {
            return ULuaAPI.luaex_unpack_int16_t(buff, offset, out field);
        }

        public static bool Encode(IntPtr buff, int offset, ushort field)
        {
            return ULuaAPI.luaex_pack_int16_t(buff, offset, (short)field);
        }

        public static bool Decode(IntPtr buff, int offset, out ushort field)
        {
            short tfield;
            bool ret = ULuaAPI.luaex_unpack_int16_t(buff, offset, out tfield);
            field = (ushort)tfield;

            return ret;
        }

        public static bool Encode(IntPtr buff, int offset, int field)
        {
            return ULuaAPI.luaex_pack_int32_t(buff, offset, field);
        }

        public static bool Decode(IntPtr buff, int offset, out int field)
        {
            return ULuaAPI.luaex_unpack_int32_t(buff, offset, out field);
        }

        public static bool Encode(IntPtr buff, int offset, uint field)
        {
            return ULuaAPI.luaex_pack_int32_t(buff, offset, (int)field);
        }

        public static bool Decode(IntPtr buff, int offset, out uint field)
        {
            int tfield;
            bool ret = ULuaAPI.luaex_unpack_int32_t(buff, offset, out tfield);
            field = (uint)tfield;

            return ret;
        }

        public static bool Encode(IntPtr buff, int offset, long field)
        {
            return ULuaAPI.luaex_pack_int64_t(buff, offset, field);
        }

        public static bool Decode(IntPtr buff, int offset, out long field)
        {
            return ULuaAPI.luaex_unpack_int64_t(buff, offset, out field);
        }

        public static bool Encode(IntPtr buff, int offset, ulong field)
        {
            return ULuaAPI.luaex_pack_int64_t(buff, offset, (long)field);
        }

        public static bool Decode(IntPtr buff, int offset, out ulong field)
        {
            long tfield;
            bool ret = ULuaAPI.luaex_unpack_int64_t(buff, offset, out tfield);
            field = (ulong)tfield;

            return ret;
        }

        public static bool Encode(IntPtr buff, int offset, float field)
        {
            return ULuaAPI.luaex_pack_float(buff, offset, field);
        }

        public static bool Decode(IntPtr buff, int offset, out float field)
        {
            return ULuaAPI.luaex_unpack_float(buff, offset, out field);
        }

        public static bool Encode(IntPtr buff, int offset, double field)
        {
            return ULuaAPI.luaex_pack_double(buff, offset, field);
        }

        public static bool Decode(IntPtr buff, int offset, out double field)
        {
            return ULuaAPI.luaex_unpack_double(buff, offset, out field);
        }

        public static bool Encode(IntPtr buff, int offset, decimal field)
        {
            return ULuaAPI.luaex_pack_decimal(buff, offset, ref field);
        }

        public static bool Decode(IntPtr buff, int offset, out decimal field)
        {
            byte scale;
            byte sign;
            int hi32;
            ulong lo64;
            if (!ULuaAPI.luaex_unpack_decimal(buff, offset, out scale, out sign, out hi32, out lo64))
            {
                field = default(decimal);

                return false;
            }

            field = new Decimal((int)(lo64 & 0xFFFFFFFF), (int)(lo64 >> 32), hi32, (sign & 0x80) != 0, scale);

            return true;
        }

        public static bool Encode(IntPtr buff, int offset, UnityEngine.Vector2 field)
        {
            if (!ULuaAPI.luaex_pack_float2(buff, offset, field.x, field.y))
                return false;

            return true;
        }

        public static void Decode(UObjectParser parser, ULuaState L, int idx, out UnityEngine.Vector2 val)
        {
            val = new UnityEngine.Vector2();

            int top = ULuaAPI.lua_gettop(L);

            if (Utils.LoadField(L, idx, "x"))
                parser.Get(L, top + 1, out val.x);
            ULuaAPI.lua_pop(L, 1);

            if (Utils.LoadField(L, idx, "y"))
                parser.Get(L, top + 1, out val.y);
            ULuaAPI.lua_pop(L, 1);
        }

        public static bool Decode(IntPtr buff, int offset, out UnityEngine.Vector2 field)
        {
            field = default(UnityEngine.Vector2);

            float x = default(float);
            float y = default(float);

            if (!ULuaAPI.luaex_unpack_float2(buff, offset, out x, out y))
                return false;

            field.x = x;
            field.y = y;

            return true;
        }

        public static bool Encode(IntPtr buff, int offset, UnityEngine.Vector3 field)
        {
            if (!ULuaAPI.luaex_pack_float3(buff, offset, field.x, field.y, field.z))
                return false;

            return true;
        }

        public static void Decode(UObjectParser parser, ULuaState L, int idx, out UnityEngine.Vector3 val)
        {
            val = new UnityEngine.Vector3();

            int top = ULuaAPI.lua_gettop(L);

            if (Utils.LoadField(L, idx, "x"))
                parser.Get(L, top + 1, out val.x);
            ULuaAPI.lua_pop(L, 1);

            if (Utils.LoadField(L, idx, "y"))
                parser.Get(L, top + 1, out val.y);
            ULuaAPI.lua_pop(L, 1);

            if (Utils.LoadField(L, idx, "z"))
                parser.Get(L, top + 1, out val.z);
            ULuaAPI.lua_pop(L, 1);
        }

        public static bool Decode(IntPtr buff, int offset, out UnityEngine.Vector3 field)
        {
            field = default(UnityEngine.Vector3);

            float x = default(float);
            float y = default(float);
            float z = default(float);

            if (!ULuaAPI.luaex_unpack_float3(buff, offset, out x, out y, out z))
                return false;

            field.x = x;
            field.y = y;
            field.z = z;

            return true;
        }

        public static bool Encode(IntPtr buff, int offset, UnityEngine.Vector4 field)
        {
            if (!ULuaAPI.luaex_pack_float4(buff, offset, field.x, field.y, field.z, field.w))
                return false;

            return true;
        }

        public static void Decode(UObjectParser parser, ULuaState L, int idx, out UnityEngine.Vector4 val)
        {
            val = new UnityEngine.Vector4();

            int top = ULuaAPI.lua_gettop(L);

            if (Utils.LoadField(L, idx, "x"))
                parser.Get(L, top + 1, out val.x);
            ULuaAPI.lua_pop(L, 1);

            if (Utils.LoadField(L, idx, "y"))
                parser.Get(L, top + 1, out val.y);
            ULuaAPI.lua_pop(L, 1);

            if (Utils.LoadField(L, idx, "z"))
                parser.Get(L, top + 1, out val.z);
            ULuaAPI.lua_pop(L, 1);

            if (Utils.LoadField(L, idx, "w"))
                parser.Get(L, top + 1, out val.w);
            ULuaAPI.lua_pop(L, 1);
        }

        public static bool Decode(IntPtr buff, int offset, out UnityEngine.Vector4 field)
        {
            field = default(UnityEngine.Vector4);

            float x = default(float);
            float y = default(float);
            float z = default(float);
            float w = default(float);

            if (!ULuaAPI.luaex_unpack_float4(buff, offset, out x, out y, out z, out w))
                return false;

            field.x = x;
            field.y = y;
            field.z = z;
            field.w = w;

            return true;
        }

        public static bool Encode(IntPtr buff, int offset, UnityEngine.Color field)
        {
            if (!ULuaAPI.luaex_pack_float4(buff, offset, field.r, field.g, field.b, field.a))
                return false;

            return true;
        }

        public static void Decode(UObjectParser parser, ULuaState L, int idx, out UnityEngine.Color val)
        {
            val = new UnityEngine.Color();

            int top = ULuaAPI.lua_gettop(L);

            if (Utils.LoadField(L, idx, "r"))
                parser.Get(L, top + 1, out val.r);
            ULuaAPI.lua_pop(L, 1);

            if (Utils.LoadField(L, idx, "g"))
                parser.Get(L, top + 1, out val.g);
            ULuaAPI.lua_pop(L, 1);

            if (Utils.LoadField(L, idx, "b"))
                parser.Get(L, top + 1, out val.b);
            ULuaAPI.lua_pop(L, 1);

            if (Utils.LoadField(L, idx, "a"))
                parser.Get(L, top + 1, out val.a);
            ULuaAPI.lua_pop(L, 1);
        }

        public static bool Decode(IntPtr buff, int offset, out UnityEngine.Color field)
        {
            field = default(UnityEngine.Color);

            float r = default(float);
            float g = default(float);
            float b = default(float);
            float a = default(float);

            if (!ULuaAPI.luaex_unpack_float4(buff, offset, out r, out g, out b, out a))
                return false;

            field.r = r;
            field.g = g;
            field.b = b;
            field.a = a;

            return true;
        }

        public static bool Encode(IntPtr buff, int offset, UnityEngine.Quaternion field)
        {
            if (!ULuaAPI.luaex_pack_float4(buff, offset, field.x, field.y, field.z, field.w))
                return false;

            return true;
        }

        public static void Decode(UObjectParser parser, ULuaState L, int idx, out UnityEngine.Quaternion val)
        {
            val = new UnityEngine.Quaternion();

            int top = ULuaAPI.lua_gettop(L);

            if (Utils.LoadField(L, idx, "x"))
                parser.Get(L, top + 1, out val.x);
            ULuaAPI.lua_pop(L, 1);

            if (Utils.LoadField(L, idx, "y"))
                parser.Get(L, top + 1, out val.y);
            ULuaAPI.lua_pop(L, 1);

            if (Utils.LoadField(L, idx, "z"))
                parser.Get(L, top + 1, out val.z);
            ULuaAPI.lua_pop(L, 1);

            if (Utils.LoadField(L, idx, "w"))
                parser.Get(L, top + 1, out val.w);
            ULuaAPI.lua_pop(L, 1);
        }

        public static bool Decode(IntPtr buff, int offset, out UnityEngine.Quaternion field)
        {
            field = default(UnityEngine.Quaternion);

            float x = default(float);
            float y = default(float);
            float z = default(float);
            float w = default(float);

            if (!ULuaAPI.luaex_unpack_float4(buff, offset, out x, out y, out z, out w))
                return false;

            field.x = x;
            field.y = y;
            field.z = z;
            field.w = w;

            return true;
        }

        public static bool Encode(IntPtr buff, int offset, UnityEngine.Ray field)
        {
            if (!Encode(buff, offset, field.origin))
                return false;

            if (!Encode(buff, offset + 12, field.direction))
                return false;

            return true;
        }

        public static void Decode(UObjectParser parser, ULuaState L, int idx, out UnityEngine.Ray val)
        {
            val = new UnityEngine.Ray();

            int top = ULuaAPI.lua_gettop(L);

            if (Utils.LoadField(L, idx, "origin"))
            {
                var origin = val.origin;
                parser.Get(L, top + 1, out origin);

                val.origin = origin;
            }
            ULuaAPI.lua_pop(L, 1);

            if (Utils.LoadField(L, idx, "direction"))
            {
                var direction = val.direction;
                parser.Get(L, top + 1, out direction);

                val.direction = direction;
            }
            ULuaAPI.lua_pop(L, 1);
        }

        public static bool Decode(IntPtr buff, int offset, out UnityEngine.Ray field)
        {
            field = default(UnityEngine.Ray);

            var origin = field.origin;
            if (!Decode(buff, offset, out origin))
                return false;
            field.origin = origin;

            var direction = field.direction;
            if (!Decode(buff, offset + 12, out direction))
                return false;
            field.direction = direction;

            return true;
        }

        public static bool Encode(IntPtr buff, int offset, UnityEngine.Bounds field)
        {
            if (!Encode(buff, offset, field.center))
                return false;

            if (!Encode(buff, offset + 12, field.extents))
                return false;

            return true;
        }

        public static void Decode(UObjectParser parser, ULuaState L, int idx, out UnityEngine.Bounds val)
        {
            val = new UnityEngine.Bounds();

            int top = ULuaAPI.lua_gettop(L);

            if (Utils.LoadField(L, idx, "center"))
            {
                var center = val.center;
                parser.Get(L, top + 1, out center);

                val.center = center;

            }
            ULuaAPI.lua_pop(L, 1);

            if (Utils.LoadField(L, idx, "extents"))
            {
                var extents = val.extents;
                parser.Get(L, top + 1, out extents);

                val.extents = extents;
            }
            ULuaAPI.lua_pop(L, 1);
        }

        public static bool Decode(IntPtr buff, int offset, out UnityEngine.Bounds field)
        {
            field = default(UnityEngine.Bounds);

            var center = field.center;
            if (!Decode(buff, offset, out center))
                return false;
            field.center = center;

            var extents = field.extents;
            if (!Decode(buff, offset + 12, out extents))
                return false;
            field.extents = extents;

            return true;
        }

        public static bool Encode(IntPtr buff, int offset, UnityEngine.Ray2D field)
        {
            if (!Encode(buff, offset, field.origin))
                return false;

            if (!Encode(buff, offset + 8, field.direction))
                return false;

            return true;
        }

        public static void Decode(UObjectParser parser, ULuaState L, int idx, out UnityEngine.Ray2D val)
        {
            val = new UnityEngine.Ray2D();

            int top = ULuaAPI.lua_gettop(L);

            if (Utils.LoadField(L, idx, "origin"))
            {
                var origin = val.origin;
                parser.Get(L, top + 1, out origin);

                val.origin = origin;
            }
            ULuaAPI.lua_pop(L, 1);

            if (Utils.LoadField(L, idx, "direction"))
            {
                var direction = val.direction;
                parser.Get(L, top + 1, out direction);

                val.direction = direction;
            }
            ULuaAPI.lua_pop(L, 1);
        }

        public static bool Decode(IntPtr buff, int offset, out UnityEngine.Ray2D field)
        {
            field = default(UnityEngine.Ray2D);

            var origin = field.origin;
            if (!Decode(buff, offset, out origin))
                return false;
            field.origin = origin;

            var direction = field.direction;
            if (!Decode(buff, offset + 8, out direction))
                return false;
            field.direction = direction;

            return true;
        }

        public static bool IsStruct(Type type)
        {
            return type.IsValueType && !type.IsEnum && !type.IsPrimitive;
        }
    }
}
