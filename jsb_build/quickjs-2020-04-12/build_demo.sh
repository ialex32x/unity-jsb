#!/bin/sh

make libquickjs.dll
make examples/demo.exe
if [ $? -eq 0 ]; then
    echo "build succeed"
    ./examples/demo.exe
    cp -f libquickjs.dll ../../Assets/jsb/Plugins/x64/
else
    echo "build failed"
fi
