#!/usr/bin/env sh

BASE_PATH=$(cd `dirname $0`;pwd)
BUILD_PATH=$BASE_PATH/../out/quickjs
echo $BUILD_PATH

mkdir -p $BUILD_PATH
cd $BUILD_PATH

rm -rf $BUILD_PATH/wsa_x64_release
echo building wsa_x64_release
mkdir -p $BUILD_PATH/wsa_x64_release
cd $BUILD_PATH/wsa_x64_release
cmake -DCONFIG_WSA=y -DQJS_VERSION=2021-03-27 -DCONFIG_LTO=y -DCMAKE_BUILD_TYPE=RELEASE -G"Unix Makefiles" $BASE_PATH
cd $BUILD_PATH
cmake --build wsa_x64_release --config Release
mkdir -p ../../prebuilt/Plugins/WSA/x64/
cp ./wsa_x64_release/Releasa/quickjs.dll ../../prebuilt/Plugins/WSA/x64/
