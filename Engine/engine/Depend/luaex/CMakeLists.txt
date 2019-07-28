cmake_minimum_required (VERSION 3.2)
project (luaex C)

option(BUILD_BUNDLE_LIBS "Build Xcode bundle" OFF)
option(BUILD_SHARED_LIBS "Build the shared library" OFF)
add_definitions(-DIOS -fPIC -DLUA_COMPAT_5_2)

set (LUA_SRC_PATH lua-5.3.3/src/)
set (LUASOCKET_SRC_PATH luasocket/)
set (LUAEX_SRC_PATH src/)

# Includes
include_directories (
  ${LUA_SRC_PATH}
  ${LUASOCKET_SRC_PATH}
  ${LUAEX_SRC_PATH}
  ./
)

# Source files
set (SOURCES
  #lua src
  ${LUA_SRC_PATH}lapi.c
  ${LUA_SRC_PATH}lcode.c
  ${LUA_SRC_PATH}lctype.c
  ${LUA_SRC_PATH}ldebug.c
  ${LUA_SRC_PATH}ldo.c
  ${LUA_SRC_PATH}ldump.c
  ${LUA_SRC_PATH}lfunc.c
  ${LUA_SRC_PATH}lgc.c
  ${LUA_SRC_PATH}llex.c
  ${LUA_SRC_PATH}lmem.c
  ${LUA_SRC_PATH}lobject.c
  ${LUA_SRC_PATH}lopcodes.c
  ${LUA_SRC_PATH}lparser.c
  ${LUA_SRC_PATH}lstate.c
  ${LUA_SRC_PATH}lstring.c
  ${LUA_SRC_PATH}ltable.c
  ${LUA_SRC_PATH}ltm.c
  ${LUA_SRC_PATH}lundump.c
  ${LUA_SRC_PATH}lvm.c
  ${LUA_SRC_PATH}lauxlib.c
  ${LUA_SRC_PATH}lbaselib.c
  ${LUA_SRC_PATH}lbitlib.c
  ${LUA_SRC_PATH}lcorolib.c
  ${LUA_SRC_PATH}ldblib.c
  ${LUA_SRC_PATH}liolib.c
  ${LUA_SRC_PATH}lmathlib.c
  ${LUA_SRC_PATH}loslib.c
  ${LUA_SRC_PATH}lstrlib.c
  ${LUA_SRC_PATH}ltablib.c
  ${LUA_SRC_PATH}linit.c
  ${LUA_SRC_PATH}lutf8lib.c
  ${LUA_SRC_PATH}loadlib.c
  ${LUA_SRC_PATH}lzio.c

  #lua socket src
  ${LUASOCKET_SRC_PATH}auxiliar.c
  ${LUASOCKET_SRC_PATH}buffer.c
  ${LUASOCKET_SRC_PATH}except.c
  ${LUASOCKET_SRC_PATH}inet.c
  ${LUASOCKET_SRC_PATH}io.c
  ${LUASOCKET_SRC_PATH}luasocket.c
  ${LUASOCKET_SRC_PATH}mime.c
  ${LUASOCKET_SRC_PATH}options.c
  ${LUASOCKET_SRC_PATH}select.c
  ${LUASOCKET_SRC_PATH}tcp.c
  ${LUASOCKET_SRC_PATH}timeout.c
  ${LUASOCKET_SRC_PATH}udp.c
  ${LUASOCKET_SRC_PATH}usocket.c

  #lua ext src
  ${LUAEX_SRC_PATH}luaex.c
  ${LUAEX_SRC_PATH}luaex_i64lib.c
)

# Library
if (BUILD_BUNDLE_LIBS)
  add_library(luaex MODULE ${SOURCES} ${HEADERS})
  set_target_properties(luaex PROPERTIES BUNDLE TRUE)
  message(STATUS "Building bundle version...")
else()
  if (BUILD_SHARED_LIBS)
    add_library (luaex SHARED ${SOURCES})
    message(STATUS "Building shared version...")
  else()
    add_library (luaex STATIC ${SOURCES})
    message(STATUS "Building static version...")
  endif()

  # Debug symbols set in XCode project
  set_xcode_property (luaex GCC_GENERATE_DEBUGGING_SYMBOLS YES "All")
endif()