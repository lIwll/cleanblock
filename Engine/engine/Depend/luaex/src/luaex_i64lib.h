#pragma once

#include <stdint.h>

#include "lua.h"
#include "lauxlib.h"
#include "lualib.h"

#ifdef __cplusplus
#	if __cplusplus
		extern "C"
		{
#	endif
#endif

LUALIB_API void
	lua_pushint64(lua_State* L, int64_t n);
LUALIB_API void
	lua_pushuint64(lua_State* L, uint64_t n);

LUALIB_API int
	lua_isint64(lua_State* L, int pos);
LUALIB_API int
	lua_isuint64(lua_State* L, int pos);

LUALIB_API int64_t
	lua_toint64(lua_State* L, int pos);
LUALIB_API uint64_t
	lua_touint64(lua_State* L, int pos);

#ifdef __cplusplus
#	if __cplusplus
		}
#	endif
#endif
