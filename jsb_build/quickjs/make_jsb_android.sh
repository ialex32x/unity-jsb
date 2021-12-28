#!/usr/bin/env sh

if [ -z "$ANDROID_NDK" ]; then
    export ANDROID_NDK=~/android-ndk-r15c
fi

BASE_PATH=$(cd `dirname $0`;pwd)
BUILD_PATH=$BASE_PATH/../out/quickjs
echo $BUILD_PATH

mkdir -p $BUILD_PATH
cd $BUILD_PATH

rm -rf $BUILD_PATH/Android_v8a_release
echo building v8a release
mkdir -p $BUILD_PATH/Android_v8a_release
cd $BUILD_PATH/Android_v8a_release
cmake -DQJS_VERSION=2021-03-27 -DCONFIG_LTO=y -DCMAKE_BUILD_TYPE=RELEASE -DANDROID_ABI=arm64-v8a -DCMAKE_TOOLCHAIN_FILE=$ANDROID_NDK/build/cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=arm-linux-androideabi-clang -DANDROID_NATIVE_API_LEVEL=android-9 $BASE_PATH
cd $BUILD_PATH
cmake --build Android_v8a_release --config Release
mkdir -p ../../prebuilt/Plugins/Android/libs/arm64-v8a/
ls -l ./Android_v8a_release/
file ./Android_v8a_release/libquickjs.so
cp ./Android_v8a_release/libquickjs.so ../../prebuilt/Plugins/Android/libs/arm64-v8a/

rm -rf $BUILD_PATH/Android_v7a_release
echo building v7a release
mkdir -p $BUILD_PATH/Android_v7a_release
cd $BUILD_PATH/Android_v7a_release
cmake -DQJS_VERSION=2021-03-27 -DCONFIG_LTO=y -DJSB_DEF_LOG2=y -DCMAKE_BUILD_TYPE=RELEASE -DANDROID_ABI=armeabi-v7a -DCMAKE_TOOLCHAIN_FILE=$ANDROID_NDK/build/cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=arm-linux-androideabi-clang -DANDROID_NATIVE_API_LEVEL=android-9 $BASE_PATH
cd $BUILD_PATH
cmake --build Android_v7a_release --config Release
mkdir -p ../../prebuilt/Plugins/Android/libs/armeabi-v7a/
ls -l ./Android_v7a_release/
file ./Android_v7a_release/libquickjs.so
cp ./Android_v7a_release/libquickjs.so ../../prebuilt/Plugins/Android/libs/armeabi-v7a/

rm -rf $BUILD_PATH/Android_x86_release
echo building x86 release
mkdir -p $BUILD_PATH/Android_x86_release
cd $BUILD_PATH/Android_x86_release
cmake -DQJS_VERSION=2021-03-27 -DCONFIG_LTO=y -DJSB_DEF_LOG2=y -DCMAKE_BUILD_TYPE=RELEASE -DANDROID_ABI=x86 -DCMAKE_TOOLCHAIN_FILE=$ANDROID_NDK/build/cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=x86-clang -DANDROID_NATIVE_API_LEVEL=android-9 $BASE_PATH
cd $BUILD_PATH
cmake --build Android_x86_release --config Release
mkdir -p ../../prebuilt/Plugins/Android/libs/x86/
ls -l ./Android_x86_release/
file ./Android_x86_release/libquickjs.so
cp ./Android_x86_release/libquickjs.so ../../prebuilt/Plugins/Android/libs/x86/

cd $BASE_PATH
