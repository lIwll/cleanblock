LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)
include $(LOCAL_PATH)/common.mk

LOCAL_MODULE := lzma
LOCAL_CFLAGS := -DANDROID -D_7ZIP_ST -std=gnu11 -fPIC $(COMMON_CFLAGS)

ifeq ($(TARGET_ARCH),x86)
TARGET_ARCH_ABI := x86
else
TARGET_ARCH_ABI := armeabi-v7a
endif

LZMA_SRC_PATH := ../../

LOCAL_DISABLE_FORMAT_STRING_CHECKS := true  # rip -Werror-format-security

LOCAL_SRC_FILES := \
$(LZMA_SRC_PATH)LzFind.c  \
$(LZMA_SRC_PATH)LzmaDec.c  \
$(LZMA_SRC_PATH)LzmaEnc.c  \
$(LZMA_SRC_PATH)dll.c  \

LOCAL_C_INCLUDES += $(LZMA_SRC_PATH)

include $(BUILD_SHARED_LIBRARY)

$(call import-module,android/cpufeatures)
