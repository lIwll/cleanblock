LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)
LOCAL_MODULE := luaex
LOCAL_CFLAGS := -DANDROID -fPIC -DLUA_COMPAT_5_2

ifeq ($(TARGET_ARCH),x86)
TARGET_ARCH_ABI := x86
else
TARGET_ARCH_ABI := armeabi-v7a
endif

LUA_SRC_PATH := ./lua-5.3.3/src/
LUASOCKET_SRC_PATH := ./luasocket/
LUAEX_SRC_PATH := ./src/

LOCAL_C_INCLUDES := $(LUA_SRC_PATH)
LOCAL_C_INCLUDES += $(LUASOCKET_SRC_PATH)
LOCAL_C_INCLUDES += $(LUAEX_SRC_PATH)
LOCAL_C_INCLUDES += $(LOCAL_PATH)

#lua src
LOCAL_SRC_FILES := \
$(LUA_SRC_PATH)lapi.c \
$(LUA_SRC_PATH)lcode.c \
$(LUA_SRC_PATH)lctype.c \
$(LUA_SRC_PATH)ldebug.c \
$(LUA_SRC_PATH)ldo.c \
$(LUA_SRC_PATH)ldump.c \
$(LUA_SRC_PATH)lfunc.c \
$(LUA_SRC_PATH)lgc.c \
$(LUA_SRC_PATH)llex.c \
$(LUA_SRC_PATH)lmem.c \
$(LUA_SRC_PATH)lobject.c \
$(LUA_SRC_PATH)lopcodes.c \
$(LUA_SRC_PATH)lparser.c \
$(LUA_SRC_PATH)lstate.c \
$(LUA_SRC_PATH)lstring.c \
$(LUA_SRC_PATH)ltable.c \
$(LUA_SRC_PATH)ltm.c \
$(LUA_SRC_PATH)lundump.c \
$(LUA_SRC_PATH)lvm.c \
$(LUA_SRC_PATH)lauxlib.c \
$(LUA_SRC_PATH)lbaselib.c \
$(LUA_SRC_PATH)lbitlib.c \
$(LUA_SRC_PATH)lcorolib.c \
$(LUA_SRC_PATH)ldblib.c \
$(LUA_SRC_PATH)liolib.c \
$(LUA_SRC_PATH)lmathlib.c \
$(LUA_SRC_PATH)loslib.c \
$(LUA_SRC_PATH)lstrlib.c \
$(LUA_SRC_PATH)ltablib.c \
$(LUA_SRC_PATH)linit.c \
$(LUA_SRC_PATH)lutf8lib.c \
$(LUA_SRC_PATH)loadlib.c \
$(LUA_SRC_PATH)lzio.c

#lua socket src
LOCAL_SRC_FILES += \
$(LUASOCKET_SRC_PATH)auxiliar.c \
$(LUASOCKET_SRC_PATH)buffer.c \
$(LUASOCKET_SRC_PATH)except.c \
$(LUASOCKET_SRC_PATH)inet.c \
$(LUASOCKET_SRC_PATH)io.c \
$(LUASOCKET_SRC_PATH)luasocket.c \
$(LUASOCKET_SRC_PATH)mime.c \
$(LUASOCKET_SRC_PATH)options.c \
$(LUASOCKET_SRC_PATH)select.c \
$(LUASOCKET_SRC_PATH)tcp.c \
$(LUASOCKET_SRC_PATH)timeout.c \
$(LUASOCKET_SRC_PATH)udp.c \
$(LUASOCKET_SRC_PATH)usocket.c

#lua ext src
LOCAL_SRC_FILES += \
$(LUAEX_SRC_PATH)luaex.c \
$(LUAEX_SRC_PATH)luaex_i64lib.c

LOCAL_LDLIBS := -lm

include $(BUILD_SHARED_LIBRARY)
