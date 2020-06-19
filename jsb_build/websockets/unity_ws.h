
#include "libwebsockets.h"

#if defined(WIN32) || defined(_WIN32)
    #define ULWS_EXTERNAL_DECL extern __declspec(dllexport)
    #define ULWS_EXTERNAL __declspec(dllexport)
#endif

typedef int ulws_ssl_type;

ULWS_EXTERNAL_DECL int ulws_pre();

ULWS_EXTERNAL_DECL struct lws_context * ulws_create(const char *name, lws_callback_function callback, size_t rx_buffer_size, size_t tx_packet_size);

ULWS_EXTERNAL_DECL struct lws *ulws_connect(const struct lws_context *context, 
                                    const char *protocol_names, 
                                    ulws_ssl_type ssl_type, 
                                    const char *host, const char *address, const char *path, int port);
