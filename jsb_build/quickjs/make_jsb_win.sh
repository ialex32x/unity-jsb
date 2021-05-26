#!/usr/bin/env sh

rm -rf ../out/quickjs/windows_x64
mkdir -p ../out/quickjs/windows_x64
cd ../out/quickjs/windows_x64
cmake -DQJS_VERSION=2021-03-27 -DCONFIG_LTO=y -DCONFIG_WIN32=y -DCONFIG_WIN32_64=y -G"Unix Makefiles" ../../../quickjs
cd ..
echo building...
cmake --build windows_x64 --config Release
mkdir -p ../../prebuilt/Plugins/x64/
pwd
ls -l ./windows_x64/
cp ./windows_x64/libquickjs.dll ../../prebuilt/Plugins/x64/quickjs.dll
cd ..

rm -rf ../out/quickjs/windows_x86
mkdir -p ../out/quickjs/windows_x86
cd ../out/quickjs/windows_x86
cmake -DQJS_VERSION=2021-03-27 -DCONFIG_LTO=y -DCONFIG_WIN32=y -G"Unix Makefiles" ../../../quickjs
cd ..
echo building...
cmake --build windows_x86 --config Release
mkdir -p ../../prebuilt/Plugins/x86/
pwd
ls -l ./windows_x86/
cp ./windows_x86/libquickjs.dll ../../prebuilt/Plugins/x86/quickjs.dll
cd ..
