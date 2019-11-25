cmake_minimum_required(VERSION 3.7.2)
project(jsb)

add_definitions(-DJSB_COMPILING)
add_definitions(-DCONFIG_BIGNUM)
add_definitions(-DCONFIG_VERSION="2019-10-27")

if (WIN32)
    add_definitions(-D_WINSOCKAPI_)
endif ()

set(TOP_DIR ${CMAKE_CURRENT_SOURCE_DIR}/..)
set(QUICKJS_DIR ${TOP_DIR}/quickjs-2019-10-27)
set(JSB_DIR ${CMAKE_CURRENT_SOURCE_DIR})

include_directories(${QUICKJS_DIR})
set(QUICKJS_SRCS 
    ${QUICKJS_DIR}/quickjs.c
)
set(JSB_SRCS ${JSB_DIR}/jsb.c)


if (APPLE) 
    if (IOS)
        add_library(jsb STATIC ${JSB_SRCS} ${QUICKJS_SRCS})
        set(CMAKE_OSX_ARCHITECTURES "$(ARCHS_STANDARD)")
        set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fembed-bitcode")
        set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -fembed-bitcode")
		set_xcode_property (jsb IPHONEOS_DEPLOYMENT_TARGET "7.0" "all")
    else ()
        add_library(jsb MODULE ${JSB_SRCS} ${QUICKJS_SRCS})
        set(CMAKE_OSX_ARCHITECTURES "$(ARCHS_STANDARD_64_BIT)")
        set_target_properties(jsb PROPERTIES MACOSX_BUNDLE TRUE)
        set_target_properties(jsb PROPERTIES BUNDLE TRUE)
    endif ()
else ()
    add_library(jsb SHARED ${JSB_SRCS} ${QUICKJS_SRCS})
endif ()

