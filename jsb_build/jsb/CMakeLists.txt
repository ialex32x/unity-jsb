cmake_minimum_required(VERSION 3.7.2)
project(jsb)

set(TOP_DIR ${CMAKE_CURRENT_SOURCE_DIR}/..)
set(QUICKJS_DIR ${TOP_DIR}/quickjs-2019-10-27)
set(JSB_DIR ${CMAKE_CURRENT_SOURCE_DIR})

include_directories(${QUICKJS_DIR})

set(QUICKJS_SRCS 
    ${QUICKJS_DIR}/quickjs.c
    ${QUICKJS_DIR}/cutils.c
    ${QUICKJS_DIR}/libbf.c
    ${QUICKJS_DIR}/libregexp.c
    ${QUICKJS_DIR}/libunicode.c
)
set(JSB_SRCS ${JSB_DIR}/jsb.c)

add_definitions(-DJSB_COMPILING)
add_definitions(-DCONFIG_BIGNUM)

if (WIN32)
    set(POLYFILLS_DIR ${TOP_DIR}/polyfills)
    include_directories((${POLYFILLS_DIR}))
    set(POLYFILLS_SRCS
        ${POLYFILLS_DIR}/sys/times.c
    )

    add_definitions(-DJSB_F_WINDOWS)
    add_definitions(-D_WINSOCKAPI_)

    add_library(jsb SHARED ${JSB_SRCS} ${QUICKJS_SRCS} ${POLYFILLS_SRCS})
endif ()

if (ANDROID)
    add_definitions(-DJSB_F_ANDROID)

    add_library(jsb SHARED ${JSB_SRCS} ${QUICKJS_SRCS})
endif ()

if (APPLE) 
    add_definitions(-DJSB_F_APPLE)
    if (IOS)
        add_definitions(-DJSB_F_APPLE_IOS)

        add_library(jsb STATIC ${JSB_SRCS} ${QUICKJS_SRCS})

        set(CMAKE_OSX_ARCHITECTURES "$(ARCHS_STANDARD)")
        set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fembed-bitcode")
        set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -fembed-bitcode")
		set_xcode_property (jsb IPHONEOS_DEPLOYMENT_TARGET "7.0" "all")
    else ()
        add_definitions(-DJSB_F_APPLE_OSX)

        add_library(jsb MODULE ${JSB_SRCS} ${QUICKJS_SRCS})

        set(CMAKE_OSX_ARCHITECTURES "$(ARCHS_STANDARD_64_BIT)")
        set_target_properties(jsb PROPERTIES MACOSX_BUNDLE TRUE)
        set_target_properties(jsb PROPERTIES BUNDLE TRUE)
    endif ()
endif ()
