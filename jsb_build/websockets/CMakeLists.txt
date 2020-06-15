cmake_minimum_required(VERSION 3.7.2)
project(websockets)

if (WIN32)
    add_definitions(-D_WINSOCKAPI_)
endif ()

set(TOP_DIR ${CMAKE_CURRENT_SOURCE_DIR})
set(OUT_DIR ${TOP_DIR}/build)

set(LIBWEBSOCKETS_DIR ${TOP_DIR}/libwebsockets)
set(MBEDTLS_DIR ${TOP_DIR}/mbedtls)
set(ZLIB_DIR ${TOP_DIR}/zlib)

include_directories(${MBEDTLS_DIR}/include)
aux_source_directory(${MBEDTLS_DIR}/library MBEDTLS_SRCS)

include_directories(${ZLIB_DIR})
set(ZLIB_SRCS
    ${ZLIB_DIR}/adler32.c
    ${ZLIB_DIR}/compress.c
    ${ZLIB_DIR}/crc32.c
    ${ZLIB_DIR}/deflate.c
    ${ZLIB_DIR}/infback.c
    ${ZLIB_DIR}/inffast.c
    ${ZLIB_DIR}/inflate.c
    ${ZLIB_DIR}/inftrees.c
    ${ZLIB_DIR}/trees.c
    ${ZLIB_DIR}/uncompr.c
    ${ZLIB_DIR}/zutil.c
)

include_directories(${LIBWEBSOCKETS_DIR})
include_directories(${LIBWEBSOCKETS_DIR}/win32helpers)
include_directories(${LIBWEBSOCKETS_DIR}/tls/mbedtls/wrapper/include)
include_directories(${LIBWEBSOCKETS_DIR}/tls/mbedtls/wrapper/include/internal)
include_directories(${LIBWEBSOCKETS_DIR}/tls/mbedtls/wrapper/include/openssl)
include_directories(${LIBWEBSOCKETS_DIR}/tls/mbedtls/wrapper/include/platform)

if (MSVC)
	# Turn off security warnings.
	add_definitions(-D_CRT_SECURE_NO_DEPRECATE -D_CRT_NONSTDC_NO_DEPRECATE)
endif(MSVC)

set(LIBWEBSOCKETS_SRCS
    ${LIBWEBSOCKETS_DIR}/core/alloc.c
    ${LIBWEBSOCKETS_DIR}/core/context.c
    ${LIBWEBSOCKETS_DIR}/core/libwebsockets.c
    ${LIBWEBSOCKETS_DIR}/core/output.c
    ${LIBWEBSOCKETS_DIR}/core/pollfd.c
    ${LIBWEBSOCKETS_DIR}/core/service.c

    ${LIBWEBSOCKETS_DIR}/event-libs/poll/poll.c

    ${LIBWEBSOCKETS_DIR}/misc/base64-decode.c
    ${LIBWEBSOCKETS_DIR}/misc/lejp.c
    ${LIBWEBSOCKETS_DIR}/misc/sha-1.c

    ${LIBWEBSOCKETS_DIR}/roles/h1/ops-h1.c
    ${LIBWEBSOCKETS_DIR}/roles/http/header.c
    ${LIBWEBSOCKETS_DIR}/roles/http/client/client.c
    ${LIBWEBSOCKETS_DIR}/roles/http/client/client-handshake.c
    ${LIBWEBSOCKETS_DIR}/roles/http/server/fops-zip.c
    ${LIBWEBSOCKETS_DIR}/roles/http/server/lejp-conf.c
    ${LIBWEBSOCKETS_DIR}/roles/http/server/parsers.c
    ${LIBWEBSOCKETS_DIR}/roles/http/server/server.c
    ${LIBWEBSOCKETS_DIR}/roles/listen/ops-listen.c
    ${LIBWEBSOCKETS_DIR}/roles/pipe/ops-pipe.c
    ${LIBWEBSOCKETS_DIR}/roles/raw/ops-raw.c

    ${LIBWEBSOCKETS_DIR}/roles/ws/client-ws.c
    ${LIBWEBSOCKETS_DIR}/roles/ws/client-parser-ws.c
    ${LIBWEBSOCKETS_DIR}/roles/ws/ops-ws.c
    ${LIBWEBSOCKETS_DIR}/roles/ws/server-ws.c

    ${LIBWEBSOCKETS_DIR}/tls/tls.c
    ${LIBWEBSOCKETS_DIR}/tls/tls-client.c
    ${LIBWEBSOCKETS_DIR}/tls/tls-server.c

    ${LIBWEBSOCKETS_DIR}/tls/mbedtls/wrapper/library/ssl_cert.c
    ${LIBWEBSOCKETS_DIR}/tls/mbedtls/wrapper/library/ssl_pkey.c
    ${LIBWEBSOCKETS_DIR}/tls/mbedtls/wrapper/library/ssl_stack.c
    ${LIBWEBSOCKETS_DIR}/tls/mbedtls/wrapper/library/ssl_methods.c
    ${LIBWEBSOCKETS_DIR}/tls/mbedtls/wrapper/library/ssl_lib.c
    ${LIBWEBSOCKETS_DIR}/tls/mbedtls/wrapper/library/ssl_x509.c
    ${LIBWEBSOCKETS_DIR}/tls/mbedtls/wrapper/platform/ssl_port.c
    ${LIBWEBSOCKETS_DIR}/tls/mbedtls/wrapper/platform/ssl_pm.c
    ${LIBWEBSOCKETS_DIR}/tls/mbedtls/lws-genhash.c
    ${LIBWEBSOCKETS_DIR}/tls/mbedtls/mbedtls-client.c
    ${LIBWEBSOCKETS_DIR}/tls/mbedtls/lws-genrsa.c
    ${LIBWEBSOCKETS_DIR}/tls/mbedtls/ssl.c
    ${LIBWEBSOCKETS_DIR}/tls/mbedtls/mbedtls-server.c
)

if (WIN32)
    add_definitions(-DWINVER=0x0601 -D_WIN32_WINNT=0x0601)
    add_definitions(-DWINDOWS_ENABLED)
    add_definitions(-DLWS_MINGW_SUPPORT)
    list(APPEND LIBWEBSOCKETS_SRCS
        ${LIBWEBSOCKETS_DIR}/plat/lws-plat-win.c
        ${LIBWEBSOCKETS_DIR}/win32helpers/getopt.c
        ${LIBWEBSOCKETS_DIR}/win32helpers/getopt_long.c
        ${LIBWEBSOCKETS_DIR}/win32helpers/gettimeofday.c
    )
else ()
    list(APPEND LIBWEBSOCKETS_SRCS ${LIBWEBSOCKETS_DIR}/plat/lws-plat-unix.c)
endif ()

if (ANDROID)
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fstrict-aliasing -fomit-frame-pointer")
    set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -fstrict-aliasing -fomit-frame-pointer")
    list(APPEND LIBWEBSOCKETS_SRCS
        ${LIBWEBSOCKETS_DIR}/misc/getifaddrs.c
    )
endif ()

if (APPLE) 
    if (IOS)
        add_library(websockets STATIC ${MBEDTLS_SRCS} ${LIBWEBSOCKETS_SRCS} ${ZLIB_SRCS})
        set(CMAKE_OSX_ARCHITECTURES "$(ARCHS_STANDARD)")
        set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fembed-bitcode")
        set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -fembed-bitcode")
        set_xcode_property (libwebsockets IPHONEOS_DEPLOYMENT_TARGET "7.0" "all")
    else ()
        add_library(websockets MODULE ${MBEDTLS_SRCS} ${LIBWEBSOCKETS_SRCS} ${ZLIB_SRCS})
        set(CMAKE_OSX_ARCHITECTURES "$(ARCHS_STANDARD_64_BIT)")
        set_target_properties(libwebsockets PROPERTIES MACOSX_BUNDLE TRUE)
        set_target_properties(libwebsockets PROPERTIES BUNDLE TRUE)
    endif ()
else ()
    if (WIN32)
        add_library(libwebsockets SHARED ${MBEDTLS_SRCS} ${LIBWEBSOCKETS_SRCS} ${ZLIB_SRCS})
        target_link_libraries(libwebsockets bcrypt)
    else ()
        add_library(websockets SHARED ${MBEDTLS_SRCS} ${LIBWEBSOCKETS_SRCS} ${ZLIB_SRCS})
    endif ()
endif ()

#target_link_libraries(websockets librws_static.lib)
