LOCAL_PATH := $(call my-dir)  
include $(CLEAR_VARS)  
LOCAL_MODULE    := kernel32
LOCAL_SRC_FILES := ../../shared/threading.cpp
include $(BUILD_SHARED_LIBRARY) 