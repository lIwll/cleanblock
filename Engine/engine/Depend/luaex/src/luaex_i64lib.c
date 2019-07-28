#define LUA_LIB

#include "luaex_i64lib.h"

#include <string.h>
#include <math.h>
#include <stdlib.h>

#if (defined(_WIN32) ||  defined(_WIN64)) && !defined(__MINGW32__) && !defined(__MINGW64__)
#	if !defined(PRId64)
#		if __WORDSIZE == 64
#			define PRId64    "ld"
#		else
#			define PRId64    "lld"
#		endif 
#	endif
#	if !defined(PRIu64)
#		if __WORDSIZE == 64
#			define PRIu64    "lu"
#		else
#			define PRIu64    "llu"
#		endif
#	endif
#else
#	include <inttypes.h>
#endif

LUALIB_API void lua_pushint64(lua_State* L, int64_t n)
{
	lua_pushinteger(L, n);
}

LUALIB_API void lua_pushuint64(lua_State* L, uint64_t n)
{
	lua_pushinteger(L, n);
}

LUALIB_API int lua_isint64(lua_State* L, int pos)
{
	return lua_isinteger(L, pos);
}

LUALIB_API int lua_isuint64(lua_State* L, int pos)
{
	return lua_isinteger(L, pos);
}

LUALIB_API int64_t lua_toint64(lua_State* L, int pos)
{
	return lua_tointeger(L, pos);
}

LUALIB_API uint64_t lua_touint64(lua_State* L, int pos)
{
	return lua_tointeger(L, pos);
}

static int uint64_tostring(lua_State* L)
{
	char temp[72];
	uint64_t n = lua_touint64(L, 1);
#if ( defined (_WIN32) ||  defined (_WIN64) ) && !defined (__MINGW32__) && !defined (__MINGW64__)
	sprintf_s(temp, sizeof(temp), "%"PRIu64, n);
#else
	snprintf(temp, sizeof(temp), "%"PRIu64, n);
#endif
	
	lua_pushstring(L, temp);
	
	return 1;
}

static int uint64_compare(lua_State* L)
{
	uint64_t lhs = lua_touint64(L, 1);
	uint64_t rhs = lua_touint64(L, 2);

	lua_pushinteger(L, lhs == rhs ? 0 : (lhs < rhs ? -1 : 1));

	return 1;
}

static int uint64_divide(lua_State* L)
{
	uint64_t lhs = lua_touint64(L, 1);
	uint64_t rhs = lua_touint64(L, 2);
	if (rhs == 0)
        return luaL_error(L, "div by zero");
	lua_pushuint64(L, lhs / rhs);

	return 1;
}

static int uint64_remainder(lua_State* L)
{
	uint64_t lhs = lua_touint64(L, 1);
	uint64_t rhs = lua_touint64(L, 2);
	if (rhs == 0)
        return luaL_error(L, "div by zero");
	lua_pushuint64(L, lhs % rhs);

	return 1;
}

int uint64_parse(lua_State* L)
{
    const char* str = lua_tostring(L, 1);

    lua_pushuint64(L, strtoull(str, NULL, 0));

    return 1;
}

LUALIB_API int luaopen_i64lib(lua_State* L)
{
    lua_newtable(L);
	
	lua_pushcfunction(L, uint64_tostring);
	lua_setfield(L, -2, "tostring");
	
	lua_pushcfunction(L, uint64_compare);
	lua_setfield(L, -2, "compare");
	
	lua_pushcfunction(L, uint64_divide);
	lua_setfield(L, -2, "divide");
	
	lua_pushcfunction(L, uint64_remainder);
	lua_setfield(L, -2, "remainder");
	
	lua_pushcfunction(L, uint64_parse);
	lua_setfield(L, -2, "parse");
	
	lua_setglobal(L, "uint64");

	return 0;
}
