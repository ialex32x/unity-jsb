cmake_minimum_required(VERSION 3.7.2)

# before project definition

if (APPLE) 
    if (NOT IOS)
        # -DCMAKE_OSX_DEPLOYMENT_TARGET:STRING=10.13
        set(CMAKE_OSX_DEPLOYMENT_TARGET "10.13" CACHE STRING "Minimum OS X deployment version")
    endif ()
endif ()

if (CONFIG_WIN32)
    if (CONFIG_WIN32_64)
        set(CMAKE_C_COMPILER x86_64-w64-mingw32-gcc)
        set(CMAKE_STRIP x86_64-w64-mingw32-strip)
        if (CONFIG_LTO)
            set(CMAKE_AR x86_64-w64-mingw32-gcc-ar)
        else ()
            set(CMAKE_AR x86_64-w64-mingw32-ar)
        endif ()
    else ()
        set(CMAKE_C_COMPILER i686-w64-mingw32-gcc)
        set(CMAKE_STRIP i686-w64-mingw32-strip)
        if (CONFIG_LTO)
            set(CMAKE_AR i686-w64-mingw32-gcc-ar)
        else ()
            set(CMAKE_AR i686-w64-mingw32-ar)
        endif ()
    endif ()
endif ()

project(unity-jsb)

set(TOP_DIR ${CMAKE_CURRENT_SOURCE_DIR})
set(OUT_DIR ${TOP_DIR}/build)
set(QJS_DIR ${TOP_DIR}/quickjs-${QJS_VERSION})

if (CONFIG_WIN32)
    if (CONFIG_WIN32_64)
        set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -m64")
    else ()
        set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -m32")
    endif ()
    set(QJS_LIBS -lm -static-libgcc -static-libstdc++ -Wl,-Bstatic -lstdc++ -lpthread -Wl,-Bdynamic)
else ()
    set(QJS_LIBS -lm -ldl)
endif ()

if (CONFIG_WSA)
    # set(QJS_LIBS ${QJS_LIBS} -lwinstorecompat)
endif ()

set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -fPIC -shared -g -Wall -Wno-array-bounds -Wno-format-truncation")
if (CONFIG_LTO)
    add_definitions(-DCONFIG_LTO)
endif ()

add_definitions(
    -DCONFIG_DISABLE_STACK_CHECK
    -DCONFIG_BIGNUM
    -DCONFIG_VERSION="${QJS_VERSION}"
    # -D_GNU_SOURCE # ok?
)

include_directories(${QJS_DIR})
set(QJS_SRCS 
    ${QJS_DIR}/quickjs.c
    ${QJS_DIR}/libregexp.c
    ${QJS_DIR}/libunicode.c
    ${QJS_DIR}/cutils.c
    ${QJS_DIR}/libbf.c
    ${QJS_DIR}/../unity_qjs.c
)

if (JSB_DEF_LOG2)
    add_definitions(-DJSB_DEF_LOG2)
endif ()

if (APPLE) 
    if (IOS)
        set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -fembed-bitcode")
        add_library(quickjs STATIC ${QJS_SRCS})
        set_xcode_property (quickjs IPHONEOS_DEPLOYMENT_TARGET "8.0" "all")
    else ()
        add_library(quickjs MODULE ${QJS_SRCS})
        set(CMAKE_OSX_ARCHITECTURES "$(ARCHS_STANDARD_64_BIT)")
        set_target_properties(quickjs PROPERTIES MACOSX_BUNDLE TRUE)
        set_target_properties(quickjs PROPERTIES BUNDLE TRUE)
    endif ()
elseif (ANDROID)
    add_library(quickjs SHARED ${QJS_SRCS})
else () # WINDOWS
    add_library(quickjs SHARED ${QJS_SRCS})
    set_target_properties(quickjs PROPERTIES SUFFIX ".dll")
endif ()

target_link_libraries(quickjs ${QJS_LIBS})

if (CONFIG_LTO)
#    set_target_properties(quickjs PROPERTIES INTERPROCEDURAL_OPTIMIZATION TRUE)
endif ()
