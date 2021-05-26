#!/usr/bin/env sh

rm -rf ../out/quickjs/linux_release
mkdir -p ../out/quickjs/linux_release
cd ../out/quickjs/linux_release
cmake -DQJS_VERSION=2021-03-27 -DCONFIG_LTO=y -G"Unix Makefiles" ../../../quickjs
cd ..
echo building...
cmake --build linux_release --config Release
mkdir -p ../../prebuilt/Plugins/x64/
pwd
ls -l ./linux_release/Release/x64/
cp ./linux_release/Release/x64/libquickjs.so ../../prebuilt/Plugins/x64/
cd ..
