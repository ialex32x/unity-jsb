cmake_minimum_required(VERSION 3.7.2)
project(unity-jsb)

set(TOP_DIR ${CMAKE_CURRENT_SOURCE_DIR})
set(OUT_DIR ${TOP_DIR}/build)
set(QJS_DIR ${TOP_DIR}/quickjs-2020-04-12)
set(QJS_LIBS -lm -static-libgcc -static-libstdc++ -Wl,-Bstatic -lstdc++ -lpthread -Wl,-Bdynamic)
if (CONFIG_WIN32)
else ()
    set(QJS_LIBS ${QJS_LIBS} -ldl)
endif ()

SET(CMAKE_C_COMPILER x86_64-w64-mingw32-gcc)
SET(CMAKE_STRIP x86_64-w64-mingw32-strip)
if (CONFIG_LTO)
    SET(CMAKE_AR x86_64-w64-mingw32-gcc-ar)
else ()
    SET(CMAKE_AR x86_64-w64-mingw32-ar)
endif ()

set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -m64 -fPIC -shared -g -Wall -Wno-array-bounds -Wno-format-truncation")
if (CONFIG_LTO)
    set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -flto")
    add_definitions(-DCONFIG_LTO)
endif ()

add_definitions(
    -DCONFIG_BIGNUM
    -DCONFIG_VERSION="2020-04-12"
)

include_directories(${QJS_DIR})
set(QJS_SRCS 
    ${QJS_DIR}/quickjs.c
    ${QJS_DIR}/libregexp.c
    ${QJS_DIR}/libunicode.c
    ${QJS_DIR}/cutils.c
    ${QJS_DIR}/libbf.c
    ${QJS_DIR}/unity_base.c
)

add_library(quickjs SHARED ${QJS_SRCS})
target_link_libraries(quickjs ${QJS_LIBS})
