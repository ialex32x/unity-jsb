
#include "unity_ws.h"
#include "core/private.h"

#define ULWS_DEFAULT 0
#define ULWS_USE_SSL 1
#define ULWS_USE_SSL_ALLOW_SELFSIGNED 2

ULWS_EXTERNAL struct lws_context *ulws_create(const char *name, size_t len, lws_callback_function callback, size_t rx_buffer_size, size_t tx_packet_size)
{
	// struct lws_protocols protocols[2];
	// memset(protocols, 0, sizeof(struct lws_protocols) * 2);
	struct lws_protocols *protocols = lws_zalloc(sizeof(struct lws_protocols) * 2, "lws_protocols");

	char *copyname = (char *)malloc(len + 1);
	memcpy(copyname, name, len);
	copyname[len] = '\0';

	protocols[0].name = copyname;
	protocols[0].callback = callback;
	protocols[0].per_session_data_size = 0;
	protocols[0].rx_buffer_size = rx_buffer_size;
	protocols[0].tx_packet_size = tx_packet_size;

	struct lws_context_creation_info info;
	memset(&info, 0, sizeof(struct lws_context_creation_info));

	info.port = CONTEXT_PORT_NO_LISTEN;
	info.protocols = protocols;
	info.gid = -1;
	info.uid = -1;
	// info.ws_ping_pong_interval = 5;
	info.user = protocols;
	struct lws_context *context = lws_create_context(&info);
	return context;
}

ULWS_EXTERNAL int ulws_pre()
{
	return LWS_PRE;
}

ULWS_EXTERNAL struct lws *ulws_connect(struct lws_context *context,
									   const char *protocol_names,
									   ulws_ssl_type ssl_type,
									   const char *host, const char *address, const char *path, int port)
{
	struct lws_client_connect_info i;

	memset(&i, 0, sizeof(struct lws_client_connect_info));

	i.context = context;
	i.protocol = protocol_names;
	if (ssl_type == ULWS_DEFAULT)
	{
		i.ssl_connection = 0;
	}
	else
	{
		i.ssl_connection = LCCSCF_USE_SSL;
		if (ssl_type & ULWS_USE_SSL_ALLOW_SELFSIGNED)
		{
			i.ssl_connection |= LCCSCF_ALLOW_SELFSIGNED;
		}
	}
	i.address = address;
	i.host = host;
	i.path = path;
	i.port = port;

	return lws_client_connect_via_info(&i);
}

ULWS_EXTERNAL void ulws_destroy(struct lws_context *context)
{
	struct lws_protocols *protocols = (struct lws_protocols *)lws_context_user(context);
	lws_context_destroy(context);
	if (protocols) {
		lws_free(protocols[0].name);
		lws_free(protocols);
	}
}
