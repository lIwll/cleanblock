using System;
using System.Text;
using System.Runtime.InteropServices;

namespace UEngine.ULua
{
//#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
//    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
//#endif
	
	public partial class ULuaAPI
	{
        public static int LUA_MULTRET = -1;
#if UNITY_IPHONE && !UNITY_EDITOR
        const string LUA_API = "__Internal";
#else
        const string LUA_API = "luaex";
#endif

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_tothread(IntPtr L, int index);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern int luaex_get_lib_version();

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_gc(IntPtr L, ELuaGCOptions what, int data);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_getupvalue(IntPtr L, int funcindex, int n);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_setupvalue(IntPtr L, int funcindex, int n);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern int lua_pushthread(IntPtr L);

		public static bool lua_isfunction(IntPtr L, int stackPos)
		{
			return lua_type(L, stackPos) == ELuaTypes.LUA_TFUNCTION;
		}

		public static bool lua_islightuserdata(IntPtr L, int stackPos)
		{
			return lua_type(L, stackPos) == ELuaTypes.LUA_TLIGHTUSERDATA;
		}

		public static bool lua_istable(IntPtr L, int stackPos)
		{
			return lua_type(L, stackPos) == ELuaTypes.LUA_TTABLE;
		}

		public static bool lua_isthread(IntPtr L, int stackPos)
		{
			return lua_type(L, stackPos) == ELuaTypes.LUA_TTHREAD;
		}

        public static int luaL_error(IntPtr L, string message) //[-0, +1, m]
        {
            luaex_csharp_str_error(L, message);

            return 0;
        }

		[DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern int lua_setfenv(IntPtr L, int stackPos);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr luaL_newstate();

		[DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_close(IntPtr L);

		[DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)] //[-0, +0, m]
        public static extern void luaopen_luaex(IntPtr L);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)] //[-0, +0, m]
        public static extern void luaL_openlibs(IntPtr L);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint luaex_objlen(IntPtr L, int stackPos);

		[DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_createtable(IntPtr L, int narr, int nrec);//[-0, +0, m]

        public static void lua_newtable(IntPtr L)//[-0, +0, m]
        {
			lua_createtable(L, 0, 0);
		}

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaex_getglobal(IntPtr L, string name);//[-1, +0, m]

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaex_setglobal(IntPtr L, string name);//[-1, +0, m]

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaex_getloaders(IntPtr L);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_settop(IntPtr L, int newTop);

		public static void lua_pop(IntPtr L, int amount)
		{
			lua_settop(L, -(amount) - 1);
		}

		[DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_insert(IntPtr L, int newTop);

		[DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_remove(IntPtr L, int index);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_gettable(IntPtr luaState, int index);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_settable(IntPtr luaState, int index);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_rawget(IntPtr L, int index);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_rawset(IntPtr L, int index);//[-2, +0, m]

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_setmetatable(IntPtr L, int objIndex);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_rawequal(IntPtr L, int index1, int index2);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_pushvalue(IntPtr L, int index);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushcclosure(IntPtr L, IntPtr fn, int n);//[-n, +1, m]

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_replace(IntPtr L, int index);

		[DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern int lua_gettop(IntPtr L);

		[DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern ELuaTypes lua_type(IntPtr L, int index);

		public static bool lua_isnil(IntPtr L, int index)
		{
			return (lua_type(L, index) == ELuaTypes.LUA_TNIL);
		}

		[DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool lua_isnumber(IntPtr L, int index);

		public static bool lua_isboolean(IntPtr L, int index)
		{
			return lua_type(L, index) == ELuaTypes.LUA_TBOOLEAN;
		}

		[DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern int luaL_ref(IntPtr L, int registryIndex);

        public static int luaL_ref(IntPtr L)//[-1, +0, m]
        {
			return luaL_ref(L, ULuaIndexes.LUA_REGISTRYINDEX);
		}

		[DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern void luaex_rawgeti(IntPtr L, int tableIndex, long index);

		[DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern void luaex_rawseti(IntPtr L, int tableIndex, long index);//[-1, +0, m]

        public static void lua_getref(IntPtr L, int reference)
		{
			luaex_rawgeti(L, ULuaIndexes.LUA_REGISTRYINDEX, reference);
		}

		[DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern void luaL_unref(IntPtr L, int registryIndex, int reference);

		public static void lua_unref(IntPtr L, int reference)
		{
			luaL_unref(L, ULuaIndexes.LUA_REGISTRYINDEX, reference);
		}

		[DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool lua_isstring(IntPtr L, int index);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_isinteger(IntPtr L, int index);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_pushnil(IntPtr L);

		public static void lua_pushstdcallcfunction(IntPtr L, lua_CSFunction function, int n = 0)//[-0, +1, m]
        {
            IntPtr fn = Marshal.GetFunctionPointerForDelegate(function);

            luaex_push_csharp_function(L, fn, n);
        }

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaex_upvalueindex(int n);

        //[DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        //public static extern int luaL_loadfile(IntPtr luaState, string filename);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_pcall(IntPtr L, int nArgs, int nResults, int errfunc);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern double lua_tonumber(IntPtr L, int index);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaex_tointeger(IntPtr L, int index);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint luaex_touint(IntPtr L, int index);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool lua_toboolean(IntPtr L, int index);

		[DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr lua_tolstring(IntPtr L, int index, out IntPtr strLen);//[-0, +0, m]

        public static string lua_tostring(IntPtr L, int index)
		{
            IntPtr strlen;

            IntPtr str = lua_tolstring(L, index, out strlen);
            if (str != IntPtr.Zero)
			{
                string ret = Marshal.PtrToStringAnsi(str, strlen.ToInt32());
                if (ret == null)
                {
                    int len = strlen.ToInt32();

                    byte[] buffer = new byte[len];
                    Marshal.Copy(str, buffer, 0, len);

                    return Encoding.ASCII.GetString(buffer);
                }

                return ret;
            } else
			{
                return null;
			}
		}

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_atpanic(IntPtr L, lua_CSFunction panicf);

		[DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_pushnumber(IntPtr L, double number);

		[DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_pushboolean(IntPtr L, bool value);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaex_pushinteger(IntPtr L, int value);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaex_pushuint(IntPtr L, uint value);

        public static void lua_pushstring(IntPtr L, string str)
        {
            if (str == null)
            {
                lua_pushnil(L);
            } else
            {
                if (Encoding.UTF8.GetByteCount(str) > str_buff.Length)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(str);

                    luaex_pushlstring(L, bytes, bytes.Length);
                } else
                {
                    int bytes_len = Encoding.UTF8.GetBytes(str, 0, str.Length, str_buff, 0);

                    luaex_pushlstring(L, str_buff, bytes_len);
                }
            }
        }

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaex_pushlstring(IntPtr L, byte[] str, int size);

        
        static byte[] str_buff = new byte[256];
        public static void luaex_pushasciistring(IntPtr L, string str) // for inner use only
        {
            if (null == str)
            {
                lua_pushnil(L);
            } else
            {
                int str_len = str.Length;
                if (str_buff.Length < str_len)
                    str_buff = new byte[str_len];

                int bytes_len = Encoding.UTF8.GetBytes(str, 0, str_len, str_buff, 0);

                luaex_pushlstring(L, str_buff, bytes_len);
            }
        }

        public static void lua_pushstring(IntPtr L, byte[] str)
        {
            if (str == null)
                lua_pushnil(L);
            else
                luaex_pushlstring(L, str, str.Length);
        }

        public static byte[] lua_tobytes(IntPtr L, int index)//[-0, +0, m]
        {
            if (lua_type(L, index) == ELuaTypes.LUA_TSTRING)
            { 
                IntPtr strlen;
                IntPtr str = lua_tolstring(L, index, out strlen);
                if (str != IntPtr.Zero)
                {
                    int buff_len = strlen.ToInt32();

                    byte[] buffer = new byte[buff_len];
                    Marshal.Copy(str, buffer, 0, buff_len);

                    return buffer;
                }
            }

            return null;
        }

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_getfield(IntPtr luaState, int stackPos, string meta);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern int luaL_newmetatable(IntPtr L, string meta);//[-0, +1, m]

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaex_pgettable(IntPtr L, int idx);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaex_psettable(IntPtr L, int idx);

        public static void luaL_getmetatable(IntPtr L, string meta)
		{
            luaex_pushasciistring(L, meta);

			lua_rawget(L, ULuaIndexes.LUA_REGISTRYINDEX);
		}

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaexL_loadbuffer(IntPtr L, byte[] buff, int size, string name);

        public static int luaL_loadbuffer(IntPtr L, string buff, string name)//[-0, +1, m]
        {
            byte[] bytes = Encoding.UTF8.GetBytes(buff);

            return luaexL_loadbuffer(L, bytes, bytes.Length, name);
        }

		[DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern int luaex_tocsobj_safe(IntPtr L,int obj);//[-0, +0, m]

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern int luaex_tocsobj_fast(IntPtr L,int obj);

        public static int lua_error(IntPtr L)
        {
            luaex_csharp_error(L);

            return 0;
        }

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool lua_checkstack(IntPtr L,int extra);//[-0, +0, m]

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern int lua_next(IntPtr L, int index);//[-1, +(2|0), e]

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_pushlightuserdata(IntPtr L, IntPtr udata);

 		[DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr luaex_tag();

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaL_where(IntPtr L, int level);//[-0, +1, m]

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaex_tryget_cachedud(IntPtr L, int key, int cache_ref);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaex_pushcsobj(IntPtr L, int key, int meta_ref, bool need_cache, int cache_ref);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaex_gen_obj_index(IntPtr L);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaex_gen_obj_newindex(IntPtr L);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaex_gen_cls_index(IntPtr L);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaex_gen_cls_newindex(IntPtr L);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaex_get_error_func_ref(IntPtr L);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaex_load_error_func(IntPtr L, int Ref);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_i64lib(IntPtr L);//[,,m]

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_socket_core(IntPtr L);//[,,m]

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushint64(IntPtr L, long n);//[,,m]

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushuint64(IntPtr L, ulong n);//[,,m]

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_isint64(IntPtr L, int idx);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_isuint64(IntPtr L, int idx);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern long lua_toint64(IntPtr L, int idx);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong lua_touint64(IntPtr L, int idx);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaex_push_csharp_function(IntPtr L, IntPtr fn, int n);//[-0,+1,m]

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaex_csharp_str_error(IntPtr L, string message);//[-0,+1,m]

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]//[-0,+0,m]
        public static extern int luaex_csharp_error(IntPtr L);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool luaex_pack_int8_t(IntPtr buff, int offset, byte field);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool luaex_unpack_int8_t(IntPtr buff, int offset, out byte field);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool luaex_pack_int16_t(IntPtr buff, int offset, short field);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool luaex_unpack_int16_t(IntPtr buff, int offset, out short field);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool luaex_pack_int32_t(IntPtr buff, int offset, int field);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool luaex_unpack_int32_t(IntPtr buff, int offset, out int field);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool luaex_pack_int64_t(IntPtr buff, int offset, long field);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool luaex_unpack_int64_t(IntPtr buff, int offset, out long field);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool luaex_pack_float(IntPtr buff, int offset, float field);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool luaex_unpack_float(IntPtr buff, int offset, out float field);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool luaex_pack_double(IntPtr buff, int offset, double field);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool luaex_unpack_double(IntPtr buff, int offset, out double field);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr luaex_pushstruct(IntPtr L, uint size, int meta_ref);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_touserdata(IntPtr L, int idx);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaex_gettypeid(IntPtr L, int idx);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaex_get_registry_index();

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaex_pgettable_bypath(IntPtr L, int idx, string path);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaex_psettable_bypath(IntPtr L, int idx, string path);

        //[DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        //public static extern void luaex_pushbuffer(IntPtr L, byte[] buff);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool luaex_pack_float2(IntPtr buff, int offset, float f1, float f2);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool luaex_unpack_float2(IntPtr buff, int offset, out float f1, out float f2);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool luaex_pack_float3(IntPtr buff, int offset, float f1, float f2, float f3);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool luaex_unpack_float3(IntPtr buff, int offset, out float f1, out float f2, out float f3);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool luaex_pack_float4(IntPtr buff, int offset, float f1, float f2, float f3, float f4);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool luaex_unpack_float4(IntPtr buff, int offset, out float f1, out float f2, out float f3, out float f4);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool luaex_pack_float5(IntPtr buff, int offset, float f1, float f2, float f3, float f4, float f5);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool luaex_unpack_float5(IntPtr buff, int offset, out float f1, out float f2, out float f3, out float f4, out float f5);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool luaex_pack_float6(IntPtr buff, int offset, float f1, float f2, float f3, float f4, float f5, float f6);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool luaex_unpack_float6(IntPtr buff, int offset, out float f1, out float f2, out float f3, out float f4, out float f5, out float f6);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool luaex_pack_decimal(IntPtr buff, int offset, ref decimal dec);

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool luaex_unpack_decimal(IntPtr buff, int offset, out byte scale, out byte sign, out int hi32, out ulong lo64);

        public static bool luaex_is_eq_str(IntPtr L, int index, string str)
        {
            return luaex_is_eq_str(L, index, str, str.Length);
        }

        [DllImport(LUA_API, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool luaex_is_eq_str(IntPtr L, int index, string str, int str_len);

        public static void lua_getglobal(IntPtr L, string name)
        {
            luaex_getglobal(L, name);
/*
            lua_pushstring(luaState, name);
            lua_gettable(luaState, ULuaIndexes.LUA_GLOBALSINDEX);
*/
        }

        public static void lua_setglobal(IntPtr L, string name)
        {
            luaex_setglobal(L, name);
/*
            lua_pushstring(luaState, name);
            lua_insert(luaState, -2);
            lua_settable(luaState, ULuaIndexes.LUA_GLOBALSINDEX);
*/
        }

        //public static int luaL_dofile(IntPtr luaState, string fileName)
        //{
        //    int result = luaL_loadfile(luaState, fileName);
        //    if (result != 0)
        //        return result;

        //    return lua_pcall(luaState, 0, -1, 0);
        //}
    }
}
