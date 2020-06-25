#!/usr/bin/env sh

rm -rf ../out/websockets/macos_release
mkdir -p ../out/websockets/macos_release
cd ../out/websockets/macos_release
cmake -GXcode ../../../websockets
cd ..
echo building...
cmake --build macos_release --config Release
mkdir -p ../../prebuilt/Plugins/libwebsockets.bundle/Contents/MacOS/
pwd
cp ./macos_release/Release/websockets.bundle/Contents/MacOS/websockets ../../prebuilt/Plugins/libwebsockets.bundle/Contents/MacOS/libwebsockets
cd ..
