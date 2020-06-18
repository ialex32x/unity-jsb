
#include "unity_ws.h"

#define _UNITY_TRUE 1
#define _UNITY_FALSE 0

// typedef int unity_bool_t;

// struct unity_websocket_payload_t {
//     unity_bool_t is_binary;
//     unity_bool_t is_done;
//     size_t len;
//     void *buf;
// };

// struct unity_websocket_t;

// typedef int lws_callback_function(struct lws *wsi, enum lws_callback_reasons reason, void *user, void *in, size_t len);

// struct unity_websocket_t {
//     void (*on_send)(struct lws *wsi);
    
//     struct lws_protocols *protocols;
//     uint32_t protocols_size;
//     const char *protocol_names;

//     struct lws_context *context;
//     struct lws *wsi;

//     unity_bool_t is_closing;
//     unity_bool_t is_servicing;
//     unity_bool_t is_polling;
//     unity_bool_t is_context_destroying;
//     unity_bool_t is_context_destroyed;
// };

#define ULWS_DEFAULT 0
#define ULWS_USE_SSL 1
#define ULWS_USE_SSL_ALLOW_SELFSIGNED 2

ULWS_EXTERNAL struct lws_context * ulws_create(const char *name, lws_callback_function callback, size_t rx_buffer_size, size_t tx_packet_size)
{
    struct lws_protocols protocols[2];
    
    protocols[0].name = name;
    protocols[0].callback = callback;
    protocols[0].per_session_data_size = 0;
    protocols[0].rx_buffer_size = rx_buffer_size;
    protocols[0].tx_packet_size = tx_packet_size;
	memset(&protocols[1], 0, sizeof(struct lws_protocols));

	struct lws_context_creation_info info;
	memset(&info, 0, sizeof(info));

	info.port = CONTEXT_PORT_NO_LISTEN;
	info.protocols = &protocols;
	info.gid = -1;
	info.uid = -1;
	//info.ws_ping_pong_interval = 5;
	info.user = 0; 
	struct lws_context *context = lws_create_context(&info);
    return context;
}

ULWS_EXTERNAL struct lws *ulws_connect(const struct lws_context *context, 
                                    const char *protocol_names, 
                                    ulws_ssl_type ssl_type, 
                                    const char *host, const char *address, const char *path, int port)
{
	struct lws_client_connect_info i;
    
	memset(&i, 0, sizeof(i));

	i.context = context;
    i.protocol = protocol_names;
    if (ssl_type == ULWS_DEFAULT) {
		i.ssl_connection = 0;
	} else {
		i.ssl_connection = LCCSCF_USE_SSL;
		if (ssl_type & ULWS_USE_SSL_ALLOW_SELFSIGNED) {
            i.ssl_connection |= LCCSCF_ALLOW_SELFSIGNED;
        }
	}
	i.address = address;
	i.host = host;
	i.path = path;
	i.port = port;

	return lws_client_connect_via_info(&i);
}

