cmake_minimum_required(VERSION 3.7.2)
project(sqlite3)

set(TOP_DIR ${CMAKE_CURRENT_SOURCE_DIR})
set(OUT_DIR ${TOP_DIR}/build)

set(LIBSQLITE3_DIR ${TOP_DIR}/sqlite-amalgamation-3320300)

include_directories(${LIBSQLITE3_DIR})
include_directories(${TOP_DIR})

set(LIBSQLITE3_SRCS
    ${TOP_DIR}/unity_sqlite.c
)

if (MSVC)
	# Turn off security warnings.
	add_definitions(-D_CRT_SECURE_NO_DEPRECATE -D_CRT_NONSTDC_NO_DEPRECATE)
endif (MSVC)

if (APPLE) 
    if (IOS)
        add_library(sqlite3 STATIC ${LIBSQLITE3_SRCS})
        set(CMAKE_OSX_ARCHITECTURES "$(ARCHS_STANDARD)")
        set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fembed-bitcode")
        set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -fembed-bitcode")
        set_xcode_property (sqlite3 IPHONEOS_DEPLOYMENT_TARGET "7.0" "all")
    else ()
        add_library(sqlite3 MODULE ${LIBSQLITE3_SRCS})
        set(CMAKE_OSX_ARCHITECTURES "$(ARCHS_STANDARD_64_BIT)")
        set_target_properties(sqlite3 PROPERTIES MACOSX_BUNDLE TRUE)
        set_target_properties(sqlite3 PROPERTIES BUNDLE TRUE)
    endif ()
else ()
    if (WIN32)
        add_library(sqlite3 SHARED ${LIBSQLITE3_SRCS})
    else ()
        add_library(sqlite3 SHARED ${LIBSQLITE3_SRCS})
    endif ()
endif ()

#target_link_libraries(sqlite3 librws_static.lib)
