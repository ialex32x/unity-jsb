@echo off

if not defined ANDROID_NDK (
    set ANDROID_NDK=D:/android-ndk-r23b
)
set BASE_PATH=%~dp0
set BUILD_PATH=%~dp0..\out\quickjs
echo %BUILD_PATH%

mkdir %BUILD_PATH% 2>nul
cd %BUILD_PATH%

rd /s /q %BUILD_PATH%\Android_v7a_release 2>nul
echo building v7a release
mkdir %BUILD_PATH%\Android_v7a_release
cd %BUILD_PATH%\Android_v7a_release
cmake -DQJS_VERSION=2021-03-27 -DCONFIG_LTO=y -DCMAKE_BUILD_TYPE=RELEASE -DANDROID_ABI=armeabi-v7a -DCMAKE_TOOLCHAIN_FILE=%ANDROID_NDK%/build/cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=arm-linux-androideabi-clang -DANDROID_NATIVE_API_LEVEL=android-16 -G "NMake Makefiles" %BASE_PATH%
cd %BUILD_PATH%
cmake --build Android_v7a_release --config Release
mkdir ..\..\prebuilt\Plugins\Android\libs\armeabi-v7a\
xcopy /Y .\Android_v7a_release\libquickjs.so ..\..\prebuilt\Plugins\Android\libs\armeabi-v7a\

rd /s /q %BUILD_PATH%\Android_v8a_release 2>nul
echo building v8a release
mkdir %BUILD_PATH%\Android_v8a_release
cd %BUILD_PATH%\Android_v8a_release
cmake -DQJS_VERSION=2021-03-27 -DCONFIG_LTO=y -DCMAKE_BUILD_TYPE=RELEASE -DANDROID_ABI=arm64-v8a -DCMAKE_TOOLCHAIN_FILE=%ANDROID_NDK%/build/cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=arm-linux-androideabi-clang -DANDROID_NATIVE_API_LEVEL=android-16 -G "NMake Makefiles" %BASE_PATH%
cd %BUILD_PATH%
cmake --build Android_v8a_release --config Release
mkdir ..\..\prebuilt\Plugins\Android\libs\arm64-v8a\
xcopy /Y .\Android_v8a_release\libquickjs.so ..\..\prebuilt\Plugins\Android\libs\arm64-v8a\

rd /s /q %BUILD_PATH%\Android_x86_release 2>nul
echo building x86 release
mkdir %BUILD_PATH%\Android_x86_release
cd %BUILD_PATH%\Android_x86_release
cmake -DQJS_VERSION=2021-03-27 -DCONFIG_LTO=y -DCMAKE_BUILD_TYPE=RELEASE -DANDROID_ABI=x86 -DCMAKE_TOOLCHAIN_FILE=%ANDROID_NDK%/build/cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=x86-clang -DANDROID_NATIVE_API_LEVEL=android-16 -G "NMake Makefiles" %BASE_PATH%
cd %BUILD_PATH%
cmake --build Android_x86_release --config Release
mkdir ..\..\prebuilt\Plugins\Android\libs\x86\
xcopy /Y .\Android_x86_release\libquickjs.so ..\..\prebuilt\Plugins\Android\libs\x86\

cd %BASE_PATH%
