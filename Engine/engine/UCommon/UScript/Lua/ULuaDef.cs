using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UEngine.ULua
{
    public enum ELuaTypes
    {
        LUA_TNONE       = -1,
        LUA_TNIL        = 0,
        LUA_TNUMBER     = 3,
        LUA_TSTRING     = 4,
        LUA_TBOOLEAN    = 1,
        LUA_TTABLE      = 5,
        LUA_TFUNCTION   = 6,
        LUA_TUSERDATA   = 7,
        LUA_TTHREAD     = 8,
        LUA_TLIGHTUSERDATA = 2
    }

    public enum ELuaGCOptions
    {
        LUA_GCSTOP      = 0,
        LUA_GCRESTART   = 1,
        LUA_GCCOLLECT   = 2,
        LUA_GCCOUNT     = 3,
        LUA_GCCOUNTB    = 4,
        LUA_GCSTEP      = 5,
        LUA_GCSETPAUSE  = 6,
        LUA_GCSETSTEPMUL = 7,
    }

    public enum ELuaThreadStatus
    {
        LUA_RESUME_ERROR    = -1,
        LUA_OK              = 0,
        LUA_YIELD           = 1,
        LUA_ERRRUN          = 2,
        LUA_ERRSYNTAX       = 3,
        LUA_ERRMEM          = 4,
        LUA_ERRERR          = 5,
    }

    public enum ELuaGenFlag
    {
        eGF_No          = 0,
        eGF_GCOptimize  = 1
    }

    public enum ELuaHotfixFlag
    {
        eHF_Stateless   = 0,
        eHF_Stateful    = 1,
    }

    enum ELogLevel
    {
        eLV_NO,
        eLV_INFO,
        eLV_WARN,
        eLV_ERROR
    }

    sealed class ULuaIndexes
    {
        public static int LUA_REGISTRYINDEX = -10000;
        public static int LUA_ENVIRONINDEX  = -10001;
        public static int LUA_GLOBALSINDEX  = -10002;
    }

    public delegate int lua_CSFunction(IntPtr L);
}
