#include "expat.h"

#include <lua.h>
#include <lauxlib.h>
#include <lualib.h>
#include <stdio.h>
#include <string.h>

#define JXML_LIBNAME "jxml"

#if LUA_VERSION_NUM >= 502
#  define new_lib(L, l) (luaL_newlib(L, l))
#else
#  define new_lib(L, l) (lua_newtable(L), luaL_register(L, NULL, l))
#endif

typedef struct _JXML_Info
{
	lua_State *L;
	int errorCode;
} JXML_Info;


void XMLCALL JXML_StartElementHandler(void *userData, const XML_Char *name, const XML_Char **atts)
{
	JXML_Info* info = (JXML_Info*)userData;
	if (info->errorCode != 0)
		return;
	lua_pushstring(info->L, name);
	lua_newtable(info->L);
	if (atts != NULL)
	{
		int i = 0;
		while (atts[i] != NULL && atts[i+1] != NULL)
		{
			lua_pushstring(info->L, atts[i]);
			lua_pushstring(info->L, atts[i+1]);
			lua_settable(info->L, -3);
			i += 2;
		}
	}
}

void XMLCALL JXML_EndElementHandler(void *userData, const XML_Char *name)
{
	JXML_Info* info = (JXML_Info*)userData;
	if (info->errorCode != 0)
		return;
	if (strcmp(lua_tostring(info->L, -2), name) != 0)
	{
		printf("XML Tag starts with %s and ends with %s\n", lua_tostring(info->L, -2), name);
		info->errorCode = 1;
	}
	lua_settable(info->L, -3);
}

void XMLCALL JXML_CharacterDataHandler(void *userData, const XML_Char *s, int len)
{
	JXML_Info* info = (JXML_Info*)userData;
	if (info->errorCode != 0)
		return;
	if (len == 0)
		return;
	char *value = malloc(len + 1);
	if (value == NULL)
		return;
	value[len] = '\0';
	memcpy(value, s, len);
	lua_pushstring(info->L, "__jxml__value");
	lua_pushstring(info->L, value);
	lua_settable(info->L, -3);
}


static int parser(lua_State *L) {
	const char *path = luaL_checklstring(L, 1, NULL);
	if (path == NULL)
	{
		lua_pop(L, 1);
		lua_pushnil(L);
		lua_pushinteger(L, -1);
		return 2;
	}
	FILE *pFile = fopen(path, "r");
	path = NULL;
	lua_pop(L, 1);
	if (pFile == NULL)
	{
		lua_pushnil(L);
		lua_pushinteger(L, -2);
		return 2;
	}
	XML_Parser p = XML_ParserCreate(NULL);
	if (p == NULL)
	{
		lua_pushnil(L);
		lua_pushinteger(L, -3);
		return 2;
	}
	int oldStack = lua_gettop(L);

	JXML_Info info;
	info.L = L;
	info.errorCode = 0;
	XML_SetElementHandler(p, JXML_StartElementHandler, JXML_EndElementHandler);
	XML_SetCharacterDataHandler(p, JXML_CharacterDataHandler);
	XML_SetUserData(p, &info);
	lua_newtable(L);
	int BUFF_SIZE = 10240;
	for (;;) {
		int bytes_read;
		void *buff = XML_GetBuffer(p, BUFF_SIZE);
		if (buff == NULL) {
			lua_settop(L, oldStack);
			lua_pushnil(L);
			lua_pushinteger(L, -3);
			return 2;
		}

		bytes_read = fread(buff, 1, BUFF_SIZE, pFile);
		if (bytes_read < 0) {
			lua_settop(L, oldStack);
			lua_pushnil(L);
			lua_pushinteger(L, -4);
			return 2;
		}

		if (!XML_ParseBuffer(p, bytes_read, bytes_read == 0)) {
			lua_settop(L, oldStack);
			lua_pushnil(L);
			lua_pushinteger(L, -5);
			return 2;
		}

		if (info.errorCode != 0)
		{
			lua_settop(L, oldStack);
			lua_pushnil(L);
			lua_pushinteger(L, -6);
			return 2;
		}

		if (bytes_read == 0)
			break;
	}
	lua_pushinteger(L, 0);
	return 2;
}

static void set_jxml_info(lua_State *L) {
	lua_pushstring(L, "Copyright (C) Joypie");
	lua_setfield(L, -2, "_COPYRIGHT");
	lua_pushstring(L, "ParserXml");
	lua_setfield(L, -2, "_DESCRIPTION");
	lua_pushstring(L, "0.0.1");
	lua_setfield(L, -2, "_VERSION");
}

static const struct luaL_Reg jxmllib[] = {
	{ "parser", parser },
	{ NULL, NULL },
};

LUAMOD_API int luaopen_jxml(lua_State *L) {
	//dir_create_meta(L);
	//lock_create_meta(L);
	new_lib(L, jxmllib);
	lua_pushvalue(L, -1);
	lua_setglobal(L, JXML_LIBNAME);
	set_jxml_info(L);
	return 1;
}
