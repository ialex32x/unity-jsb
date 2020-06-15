#!/usr/bin/env sh

rm -rf ../out/quickjs/windows_x64
mkdir -p ../out/quickjs/windows_x64
cd ../out/quickjs/windows_x64
cmake -DCONFIG_LTO=y -DCONFIG_WIN32=y -DCONFIG_WIN32_64=y -G"Unix Makefiles" ../../../quickjs
cd ..
echo building...
cmake --build windows_x64 --config Release
mkdir -p ../../prebuilt/Plugins/x64/
pwd
cp ./windows_x64/libquickjs.dll ../../prebuilt/Plugins/x64/
cd ..

rm -rf ../out/quickjs/windows_x86
mkdir -p ../out/quickjs/windows_x86
cd ../out/quickjs/windows_x86
cmake -DCONFIG_LTO=y -DCONFIG_WIN32=y -G"Unix Makefiles" ../../../quickjs
cd ..
echo building...
cmake --build windows_x86 --config Release
mkdir -p ../../prebuilt/Plugins/x86/
cp ./windows_x86/libquickjs.dll ../../prebuilt/Plugins/x86/
cd ..
