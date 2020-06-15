#!/usr/bin/env sh

rm -rf out/windows_x64
mkdir -p out/windows_x64
cd out/windows_x64

cmake -DCONFIG_LTO=y -DCONFIG_WIN32=y -G"Unix Makefiles" ../..
cd ..
echo building...
cmake --build windows_x64 --config Release
cd ..
