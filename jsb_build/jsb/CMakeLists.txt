cmake_minimum_required(VERSION 3.7.2)
project(jsb)

add_definitions(-DJSB_COMPILING)
add_definitions(-DCONFIG_BIGNUM)

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
aux_source_directory(${JSB_DIR} JSB_SRCS)

add_library(jsb shared ${JSB_SRCS} ${QUICKJS_SRCS})
