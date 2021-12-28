cmake_minimum_required(VERSION 3.10)

# before project definition

if (APPLE) 
    if (NOT IOS)
        # -DCMAKE_OSX_DEPLOYMENT_TARGET:STRING=10.13
        set(CMAKE_OSX_DEPLOYMENT_TARGET "10.13" CACHE STRING "Minimum OS X deployment version")
        set(CMAKE_XCODE_ATTRIBUTE_ONLY_ACTIVE_ARCH "NO")
        # set(CMAKE_XCODE_ATTRIBUTE_ARCHS "x84_64")
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

if (CONFIG_WSA)
    # set(QJS_LIBS ${QJS_LIBS} -lwinstorecompat)
    set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} /wd\"4146\" /wd\"4703\"")

    add_definitions(
        -D_CRT_SECURE_NO_WARNINGS
    )

    set(QJS_DIR ${TOP_DIR}/quickjs-wsa)
    include_directories(${QJS_DIR})
    set(QJS_SRCS 
        ${QJS_DIR}/quickjs.c
        ${QJS_DIR}/libregexp.c
        ${QJS_DIR}/libunicode.c
        ${QJS_DIR}/cutils.c
        ${QJS_DIR}/../unity_qjs.c
    )
else ()
    if (CONFIG_WIN32)
        if (CONFIG_WIN32_64)
            set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -m64")
        else ()
            set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -m32")
        endif ()
        set(QJS_LIBS -lm -static-libgcc -static-libstdc++ -Wl,-Bstatic -lstdc++ -lpthread -Wl,-Bdynamic)
    else ()
        set(QJS_LIBS -lm -ldl)
        set(LDFLAGS "${LDFLAGS} -rdynamic")
    endif ()

    if (JSB_DEBUG)
        add_definitions(-DJSB_DEBUG)
        set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -g")
    endif ()

    add_definitions(
        -DCONFIG_BIGNUM
    )
    
    set(QJS_DIR ${TOP_DIR}/quickjs-latest)
    include_directories(${QJS_DIR})
    set(QJS_SRCS 
        ${QJS_DIR}/quickjs.c
        ${QJS_DIR}/libregexp.c
        ${QJS_DIR}/libunicode.c
        ${QJS_DIR}/cutils.c
        ${QJS_DIR}/libbf.c
        ${QJS_DIR}/../unity_qjs.c
    )
    
    if (APPLE)
        set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -fPIC -Wall -Wno-array-bounds -Wno-format-truncation")
    else ()
        set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -fPIC -shared -Wall -Wno-array-bounds -Wno-format-truncation")
    endif ()
endif ()

if (CONFIG_LTO)
    add_definitions(-DCONFIG_LTO)
    set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -O2 -flto")
    set(LDFLAGS "${LDFLAGS} ${CMAKE_C_FLAGS} -g")
else ()
    set(LDFLAGS "${LDFLAGS} ${CMAKE_C_FLAGS}")
endif ()

if (CONFIG_WIN32)
    add_definitions(
        -D__USE_MINGW_ANSI_STDIO
    )
endif ()

add_definitions(
    -DCONFIG_DISABLE_STACK_CHECK
    -DCONFIG_VERSION="${QJS_VERSION}"
    -D_GNU_SOURCE # ok?

    # do not throw error when set typed array out of bound index
    -DJSB_TYPED_ARRAY_NO_THROW
)

if (JSB_DEF_LOG2)
    add_definitions(-DJSB_DEF_LOG2)
endif ()

if (APPLE) 
    if (IOS)
        set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -fembed-bitcode")
        # set(CMAKE_STATIC_LINKER_FLAGS "${CMAKE_STATIC_LINKER_FLAGS} ${LDFLAGS}")
        set(IOS_PLATFORM OS64)
        set(ENABLE_BITCODE 1)
        add_library(quickjs STATIC ${QJS_SRCS})
        set_xcode_property(quickjs IPHONEOS_DEPLOYMENT_TARGET "9.0" "all")
    else ()
        set(CMAKE_OSX_ARCHITECTURES "$(ARCHS_STANDARD)")
        set(CMAKE_MODULE_LINKER_FLAGS "${CMAKE_MODULE_LINKER_FLAGS} ${LDFLAGS}")
        add_library(quickjs MODULE ${QJS_SRCS})
        set_target_properties(quickjs PROPERTIES MACOSX_BUNDLE TRUE)
        set_target_properties(quickjs PROPERTIES BUNDLE TRUE)
    endif ()
else () # WINDOWS ANDROID LINUX
    set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} ${LDFLAGS}")
    set(CMAKE_STATIC_LINKER_FLAGS "${CMAKE_STATIC_LINKER_FLAGS} ${LDFLAGS}")
    add_library(quickjs SHARED ${QJS_SRCS})
endif ()

if (CONFIG_WIN32) 
    set_target_properties(quickjs PROPERTIES SUFFIX ".dll")
endif()

target_link_libraries(quickjs ${QJS_LIBS})

if (CONFIG_LTO)
#    set_target_properties(quickjs PROPERTIES INTERPROCEDURAL_OPTIMIZATION TRUE)
endif ()
