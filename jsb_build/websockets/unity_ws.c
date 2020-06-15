
#include "libwebsockets.h"

#define _UNITY_TRUE 1
#define _UNITY_FALSE 0

#define UNITY_LOCAL static

#define LWS_BUF_SIZE 65536
#define LWS_PACKET_SIZE 65536
#define LWS_PAYLOAD_SIZE 4096

typedef int unity_bool_t;
typedef size_t unity_size_t;

// struct unity_websocket_payload_t {
//     unity_bool_t is_binary;
//     void *buf;
//     unity_size_t len;
//     struct unity_websocket_payload_t *next;
// };

// struct unity_websocket_t {
//     duk_context *ctx;
//     void *heapptr;

//     struct unity_websocket_payload_t *pending_head;
//     struct unity_websocket_payload_t *pending_tail;
//     struct unity_websocket_payload_t *freelist;
    
//     unity_bool_t is_binary;
//     void *buf;
//     unity_size_t len;

//     struct lws_protocols *protocols;
//     duk_uarridx_t protocols_size;
//     const char *protocol_names;

//     struct lws_context *context;
//     struct lws *wsi;

//     unity_bool_t is_closing;
//     unity_bool_t is_servicing;
//     unity_bool_t is_polling;
//     unity_bool_t is_context_destroying;
//     unity_bool_t is_context_destroyed;
// };

// UNITY_LOCAL void _delete_payload(struct unity_websocket_t *websocket, struct unity_websocket_payload_t *payload) {
//     payload->next = websocket->freelist;
//     websocket->freelist = payload;
// }

// UNITY_LOCAL struct unity_websocket_payload_t *_new_payload(struct unity_websocket_t *websocket) {
//     struct unity_websocket_payload_t *payload = websocket->freelist;
//     if (payload) {
//         websocket->freelist = payload->next;
//     } else {
//         payload = (struct unity_websocket_payload_t *)duk_alloc(websocket->ctx, sizeof(struct unity_websocket_payload_t));
//         duk_memzero(payload, sizeof(struct unity_websocket_payload_t));
//         payload->buf = duk_alloc(websocket->ctx, LWS_PAYLOAD_SIZE + LWS_PRE);
//         duk_memzero(payload->buf, LWS_PAYLOAD_SIZE + LWS_PRE);
//     }
//     payload->next = NULL;
//     payload->len = 0;
//     return payload;
// }

// UNITY_LOCAL void _duk_lws_destroy(struct unity_websocket_t *websocket) {
//     if (websocket == NULL || websocket->is_context_destroyed) {
//         return;
//     }
//     if (websocket->is_polling) {
//         websocket->is_context_destroying = _UNITY_TRUE;
//         return;
//     }
//     websocket->is_context_destroyed = _UNITY_TRUE;
//     if (websocket->context != NULL) {
//         lws_context_destroy(websocket->context);
//         websocket->context = NULL;
//     }
//     struct unity_websocket_payload_t *payload = websocket->pending_head;
//     while (payload) {
//         websocket->pending_head = payload->next;
//         payload->next = websocket->freelist;
//         websocket->freelist = payload;
//         payload = websocket->pending_head;
//     }
//     websocket->pending_tail = NULL;
// }

// UNITY_LOCAL void _on_connect(struct unity_websocket_t *websocket, const char *protocol) {
//     duk_context *ctx = websocket->ctx;
//     duk_push_heapptr(ctx, websocket->heapptr);
//     duk_push_literal(ctx, "dispatch");
//     duk_push_literal(ctx, "open");
//     duk_push_string(ctx, protocol);
//     if (duk_pcall_prop(ctx, -4, 2) != DUK_EXEC_SUCCESS) {
//         lwsl_warn("unable to dispatch: %s", duk_to_string(ctx, -1));
//     }
//     duk_pop_2(ctx);
// }

// UNITY_LOCAL void _on_error(struct unity_websocket_t *websocket) {
//     duk_context *ctx = websocket->ctx;
//     duk_push_heapptr(ctx, websocket->heapptr);
//     duk_push_literal(ctx, "dispatch");
//     duk_push_literal(ctx, "error");
//     if (duk_pcall_prop(ctx, -3, 1) != DUK_EXEC_SUCCESS) {
//         lwsl_warn("unable to dispatch: %s", duk_to_string(ctx, -1));
//     }
//     duk_pop_2(ctx);
// }

// UNITY_LOCAL void _on_disconnect(struct unity_websocket_t *websocket) {
//     duk_context *ctx = websocket->ctx;
//     duk_push_heapptr(ctx, websocket->heapptr);
//     duk_push_literal(ctx, "dispatch");
//     duk_push_literal(ctx, "close");
//     if (duk_pcall_prop(ctx, -3, 1) != DUK_EXEC_SUCCESS) {
//     }
//     duk_pop_2(ctx);
// }

// UNITY_LOCAL void _on_close_request(struct unity_websocket_t *websocket, int code, const char *reason) {
//     duk_context *ctx = websocket->ctx;
//     duk_push_heapptr(ctx, websocket->heapptr);
//     duk_push_literal(ctx, "dispatch");
//     duk_push_literal(ctx, "close_request");
//     duk_push_int(ctx, code);
//     if (reason) {
//         duk_push_string(ctx, reason);
//     } else {
//         duk_push_null(ctx);
//     }
//     if (duk_pcall_prop(ctx, -5, 3) != DUK_EXEC_SUCCESS) {
//         lwsl_warn("unable to dispatch: %s", duk_to_string(ctx, -1));
//     }
//     duk_pop_2(ctx);
// }

// UNITY_LOCAL void _on_received(struct unity_websocket_t *websocket) {
//     duk_context *ctx = websocket->ctx;
//     duk_push_heapptr(ctx, websocket->heapptr);
//     duk_push_literal(ctx, "dispatch");
//     duk_push_literal(ctx, "data");
//     if (websocket->is_binary) {
//         duk_push_fixed_buffer(ctx, websocket->len);
//         void *buffer = duk_get_buffer_data(ctx, -1, NULL);
//         duk_memcpy(buffer, websocket->buf, websocket->len);
//     } else {
//         duk_push_lstring(ctx, (const char *)(websocket->buf), websocket->len);
//     }
//     if (duk_pcall_prop(ctx, -4, 2) != DUK_EXEC_SUCCESS) {
//         lwsl_warn("unable to dispatch: %s", duk_to_string(ctx, -1));
//     }
//     duk_pop_2(ctx);
// }

// UNITY_LOCAL int _lws_receive(struct unity_websocket_t *websocket, struct lws *wsi, void *in, size_t len) {
//     if (lws_is_first_fragment(wsi)) {
//         websocket->len = 0;
//     }
//     if (websocket->len + len > LWS_PAYLOAD_SIZE) {
//         lwsl_debug("receiving payload is too large");
//         return -1;
//     }
//     duk_memcpy(&(((char *)(websocket->buf))[websocket->len]), in, len);
//     websocket->len += len;
//     if (lws_is_final_fragment(wsi)) {
//         websocket->is_binary = lws_frame_is_binary(wsi);
//         _on_received(websocket);
//     }
//     return 0;
// }

// UNITY_LOCAL void _lws_send(struct unity_websocket_t *websocket, struct lws *wsi) {
//     struct unity_websocket_payload_t *payload = websocket->pending_head;
//     if (payload) {
//         websocket->pending_head = payload->next;
//         if (websocket->pending_head == NULL) {
//             websocket->pending_tail = NULL;
//         }
//         payload->next = NULL;
//         enum lws_write_protocol protocol = payload->is_binary ? LWS_WRITE_BINARY : LWS_WRITE_TEXT;
//         lws_write(wsi, &(((char *)(payload->buf))[LWS_PRE]), payload->len, protocol);
//         _delete_payload(websocket, payload);
//         if (websocket->pending_head) {
//             lws_callback_on_writable(websocket->wsi);
//         }
//     }
// }

// UNITY_LOCAL void _duk_lws_close(struct unity_websocket_t *websocket) {
//     if (websocket->wsi) {
//         websocket->is_closing = _UNITY_TRUE;
//         lws_callback_on_writable(websocket->wsi);
//         websocket->wsi = NULL;
//     }
// }

// UNITY_LOCAL int _lws_callback_function(struct lws *wsi, 
//                                     enum lws_callback_reasons reason,
// 		                            void *user, 
//                                     void *in, 
//                                     size_t len) {
//     struct unity_websocket_t *websocket = (struct unity_websocket_t *)lws_context_user(lws_get_context(wsi));

//     websocket->is_servicing = _UNITY_TRUE;
// 	switch (reason) {
//         case LWS_CALLBACK_OPENSSL_LOAD_EXTRA_CLIENT_VERIFY_CERTS: {
//             return 0;
//         } 
//         case LWS_CALLBACK_CLIENT_ESTABLISHED: {
//             websocket->wsi = wsi;
// 			_on_connect(websocket, lws_get_protocol(wsi)->name);
//             return 0;
//         } 
//         case LWS_CALLBACK_CLIENT_CONNECTION_ERROR: {
//             _on_error(websocket);
// 			_duk_lws_destroy(websocket);
// 			return -1;
//         } 
//         case LWS_CALLBACK_WS_PEER_INITIATED_CLOSE: {
//             const uint8_t *b = (const uint8_t *)in;
//             int code = b[0] << 8 | b[1];
//             const char *utf8 = NULL;
//             if (len > 2) {
//                 utf8 = (const char *)&b[2];
//             }
//             _on_close_request(websocket, code, utf8);
// 			return 0;
//         } 
//         case LWS_CALLBACK_CLIENT_CLOSED: {
// 			_duk_lws_close(websocket);
// 			_duk_lws_destroy(websocket);
// 			_on_disconnect(websocket);
//             return 0;
//         } 
//         case LWS_CALLBACK_CLIENT_RECEIVE: {
//             return _lws_receive(websocket, wsi, in, len);
//         } 
//         case LWS_CALLBACK_CLIENT_WRITEABLE: {
//             if (websocket->is_closing) {
//                 lws_close_reason(wsi, LWS_CLOSE_STATUS_NORMAL, "", 0);
//                 return -1;
//             }
//             _lws_send(websocket, wsi);
//             return 0;
//         } 
//         default:  {
//             return 0;
//         }
//     }
// }

// struct IP_Address {
// 	union {
// 		uint8_t field8[16];
// 		uint16_t field16[8];
// 		uint32_t field32[4];
// 	};
// 	unity_bool_t valid;
// 	unity_bool_t wildcard;
// };

// UNITY_LOCAL void _IP_Address_clear(struct IP_Address *ip) {
// 	memset(&(ip->field8[0]), 0, sizeof(ip->field8));
// 	ip->valid = _UNITY_FALSE;
// 	ip->wildcard = _UNITY_FALSE;
// }

// UNITY_LOCAL void _IP_Address_set_ipv4(struct IP_Address *ip, const uint8_t *p_ip) {
//     _IP_Address_clear(ip);
//     ip->valid = _UNITY_TRUE;
// 	ip->field16[5] = 0xffff;
// 	ip->field32[3] = *((const uint32_t *)p_ip);
// }

// UNITY_LOCAL void _IP_Address_set_ipv6(struct IP_Address *ip, const uint8_t *p_ip) {
// 	_IP_Address_clear(ip);
// 	ip->valid = _UNITY_TRUE;
// 	for (int i = 0; i < 16; i++) {
// 		ip->field8[i] = p_ip[i];
//     }
// }

// UNITY_LOCAL unity_bool_t _resolve_hostname(const char *p_hostname, int p_type, struct IP_Address *ip) {
//     if (!ip) {
//         return _UNITY_FALSE;
//     }
// 	struct addrinfo hints;
// 	struct addrinfo *result;

// 	duk_memzero(&hints, sizeof(struct addrinfo));
// 	if (p_type == AF_INET) {
// 		hints.ai_family = AF_INET;
// 	} else if (p_type == AF_INET6) {
// 		hints.ai_family = AF_INET6;
// 		hints.ai_flags = 0;
// 	} else {
// 		hints.ai_family = AF_UNSPEC;
// 		hints.ai_flags = AI_ADDRCONFIG;
// 	};
// 	hints.ai_flags &= ~AI_NUMERICHOST;

// 	int s = getaddrinfo(p_hostname, NULL, &hints, &result);
// 	if (s != 0) {
// 		return _UNITY_FALSE;
// 	};

// 	if (result == NULL || result->ai_addr == NULL) {
// 		if (result) {
//             freeaddrinfo(result);
//         }
// 		return _UNITY_FALSE;
// 	};

// 	if (result->ai_addr->sa_family == AF_INET) {
// 		struct sockaddr_in *addr = (struct sockaddr_in *)result->ai_addr;
// 		_IP_Address_set_ipv4(ip, (uint8_t *)&(addr->sin_addr));
// 	} else if (result->ai_addr->sa_family == AF_INET6) {
// 		struct sockaddr_in6 *addr6 = (struct sockaddr_in6 *)result->ai_addr;
// 		_IP_Address_set_ipv6(ip, addr6->sin6_addr.s6_addr);
// 	};

// 	freeaddrinfo(result);
// 	return _UNITY_TRUE;
// }

// UNITY_LOCAL struct unity_websocket_t *duk_get_websocket(duk_context *ctx, duk_idx_t idx) {
//     struct unity_websocket_t *websocket = NULL;
//     if (duk_get_prop_literal(ctx, idx, DUK_HIDDEN_SYMBOL("websocket"))) {
//         websocket = (struct unity_websocket_t *)duk_get_pointer(ctx, -1);
//     }
//     duk_pop(ctx);
//     return websocket;
// }

// UNITY_LOCAL duk_ret_t duk_WebSocket_constructor(duk_context *ctx) {
//     duk_idx_t top = duk_get_top(ctx);
//     if (top >= 1) {
//         if (!duk_is_array(ctx, 0)) {
//             return duk_generic_error(ctx, "invalid arg #0 (protocols)");
//         }
//     }
//     unity_size_t protocols_size = duk_get_length(ctx, 0);
//     struct unity_websocket_t *websocket = (struct unity_websocket_t *)duk_alloc(ctx, sizeof(struct unity_websocket_t));
//     if (websocket == 0) {
//         return duk_generic_error(ctx, "unable to alloc websocket");
//     }
//     duk_memset(websocket, 0, sizeof(struct unity_websocket_t));
//     duk_push_this(ctx);
//     duk_push_object(ctx);
//     duk_put_prop_string(ctx, -2, "events");
//     duk_push_pointer(ctx, websocket);
//     duk_put_prop_literal(ctx, -2, DUK_HIDDEN_SYMBOL("websocket"));
//     websocket->buf = duk_alloc(ctx, LWS_PAYLOAD_SIZE);
//     websocket->len = 0;
//     duk_memzero(websocket->buf, LWS_PAYLOAD_SIZE);
//     websocket->heapptr = duk_get_heapptr(ctx, -1);
//     duk_pop(ctx);
//     websocket->ctx = ctx;
//     websocket->wsi = NULL;
//     websocket->protocols = (struct lws_protocols *)duk_alloc(ctx, sizeof(struct lws_protocols) * (protocols_size + 2));
//     if (websocket->protocols == NULL) {
//         return duk_generic_error(ctx, "unable to alloc websocket protocols");
//     }
//     duk_memset(websocket->protocols, 0, sizeof(struct lws_protocols) * (protocols_size + 2));
//     websocket->protocols_size = 0;
// 	websocket->protocols[websocket->protocols_size].name = "default";
// 	websocket->protocols[websocket->protocols_size].callback = _lws_callback_function;
// 	websocket->protocols[websocket->protocols_size].per_session_data_size = 0;
// 	websocket->protocols[websocket->protocols_size].rx_buffer_size = LWS_BUF_SIZE;
// 	websocket->protocols[websocket->protocols_size].tx_packet_size = LWS_PACKET_SIZE;
//     websocket->protocols_size = 1;
//     for (unity_size_t i = 0; i < protocols_size; i++) {
//         duk_get_prop_index(ctx, 0, i);
//         unity_size_t protocol_name_length;
//         const char *protocol_name_ptr = duk_get_lstring(ctx, -1, &protocol_name_length);
//         if (protocol_name_ptr != NULL) {
//             char *protocol_name = duk_alloc(ctx, protocol_name_length + 1);
//             if (protocol_name != NULL) {
//                 duk_memcpy(protocol_name, protocol_name_ptr, protocol_name_length);
//                 protocol_name[protocol_name_length] = '\0';
//                 websocket->protocols[websocket->protocols_size].name = protocol_name;
//                 websocket->protocols[websocket->protocols_size].callback = _lws_callback_function;
//                 websocket->protocols[websocket->protocols_size].per_session_data_size = 0;
//                 websocket->protocols[websocket->protocols_size].rx_buffer_size = LWS_BUF_SIZE;
//                 websocket->protocols[websocket->protocols_size].tx_packet_size = LWS_PACKET_SIZE;
//                 websocket->protocols_size++;
//             }
//         }
//         duk_pop(ctx);
//     }
//     duk_push_literal(ctx, ",");
//     for (unity_size_t i = 0; i < websocket->protocols_size; i++) {
//         duk_push_string(ctx, websocket->protocols[i].name);
//     }
//     duk_join(ctx, websocket->protocols_size);
//     unity_size_t protocol_names_length;
//     const char *protocol_names_ptr = duk_get_lstring(ctx, -1, &protocol_names_length);
//     char *protocol_names = (char *)duk_alloc(ctx, protocol_names_length + 1);
//     duk_memcpy(protocol_names, protocol_names_ptr, protocol_names_length);
//     protocol_names[protocol_names_length] = '\0';
//     websocket->protocol_names = protocol_names;
//     duk_pop(ctx); // pop join result
// 	websocket->protocols[websocket->protocols_size].name = NULL;
// 	websocket->protocols[websocket->protocols_size].callback = NULL;
// 	websocket->protocols[websocket->protocols_size].per_session_data_size = 0;
// 	websocket->protocols[websocket->protocols_size].rx_buffer_size = 0;
//     return 0;
// }

// UNITY_LOCAL duk_ret_t duk_WebSocket_finalizer(duk_context *ctx) {
//     struct unity_websocket_t *websocket = duk_get_websocket(ctx, 0);
//     if (websocket != 0) {
//         _duk_lws_destroy(websocket);
//         for (int i = 1; i < websocket->protocols_size; i++) {
//             struct lws_protocols *p = &(websocket->protocols[i]);
//             if (p && p->name) {
//                 duk_free(ctx, p->name);
//                 p->name = NULL;
//             }
//         }
//         duk_free(ctx, websocket->protocols);
//         websocket->protocols = NULL;
//         duk_free(ctx, websocket->protocol_names);
//         websocket->protocol_names = NULL;
//         struct unity_websocket_payload_t *payload = websocket->freelist;
//         while (payload) {
//             websocket->freelist = payload->next;
//             duk_free(ctx, payload->buf);
//             duk_free(ctx, payload);
//             payload = websocket->freelist;
//         }
//         duk_free(ctx, websocket);
//     }
//     return 0;
// }

// UNITY_LOCAL int _LWS_str_index_of(const char *str, size_t len, char c) {
// 	int i = 0;
// 	while (i < len) {
// 		if (str[i] == c) {
// 			return i;
// 		}
// 		++i;
// 	}
// 	return -1;
// }

// UNITY_LOCAL int _LWS_str_to_int(const char *str, size_t len) {
// 	if (len == 0) {
// 		return 0;
// 	}
// 	int integer = 0;
// 	int sign = 1;
// 	for (int i = 0; i < len; i++) {
// 		char c = str[i];
// 		if (c >= '0' && c <= '9') {
// 			integer *= 10;
// 			integer += c - '0';
// 		}
// 		else if (integer == 0 && c == '-') {
// 			sign = -sign;
// 		}
// 		else if (c == '.') {
// 			break;
// 		}
// 	}
// 	return integer * sign;
// }

// UNITY_LOCAL unity_bool_t _parse_url(duk_context *ctx, const char *url, size_t len, const char **host, int *port, const char **path, unity_bool_t *ssl) {
// 	if (!url || len <= 5 || len > 255) {
// 		return 0;
// 	}
// 	char *buf = NULL;
// 	const char *str = url;
// 	size_t left = len;
// 	if (len > 5 && !duk_memcmp(url, "ws://", 5)) {
// 		*ssl = 0;
// 		str += 5;
// 		left -= 5;
// 	} else if (len > 6 && !duk_memcmp(url, "wss://", 6)) {
// 		*ssl = 1;
// 		str += 6;
// 		left -= 6;
// 	} else {
// 		return 0;
// 	}
// 	int path_idx = _LWS_str_index_of(str, left, '/');
// 	if (path_idx < 0) {
// 		path_idx = left;
// 	}
// 	int port_idx = _LWS_str_index_of(str, left, ':');
// 	if (port_idx > path_idx) {
// 		port_idx = -1;
// 	}
// 	int host_idx = port_idx < 0 || path_idx < port_idx ? path_idx : port_idx;
// 	if (host_idx <= 0) {
// 		return 0;
// 	}
// 	if (port_idx >= 0) {
// 		*port = _LWS_str_to_int(str + port_idx + 1, left - port_idx);
// 	} else {
// 		*port = *ssl ? 443 : 80;
// 	}
// 	if (path_idx < 0 || path_idx >= left) {
// 		buf = (char *)duk_alloc(ctx, 2);
// 		*path = buf;
// 		buf[0] = '/';
// 		buf[1] = '\0';
// 	} else {
// 		path_idx--;
// 		buf = (char *)duk_alloc(ctx, left - path_idx);
// 		*path = buf;
// 		duk_memcpy(*path, str + path_idx + 1, left - path_idx - 1);
// 		buf[left - path_idx - 1] = '\0';
// 	}
// 	buf = (char *)duk_alloc(ctx, host_idx + 1);
// 	*host = buf;
// 	duk_memcpy(*host, str, host_idx);
// 	buf[host_idx] = '\0';
// 	return 1;
// }

// UNITY_LOCAL duk_ret_t duk_WebSocket_connect(duk_context *ctx) {
//     size_t p_url_len;
//     char *p_url = duk_require_lstring(ctx, 0, &p_url_len);
//     unity_bool_t p_allow_self_signed = duk_get_boolean_default(ctx, 1, _UNITY_FALSE);
//     const char *x_host = NULL;
// 	int x_port;
// 	const char *x_path = NULL;
// 	unity_bool_t x_ssl;
// 	if (!_parse_url(ctx, p_url, p_url_len, &x_host, &x_port, &x_path, &x_ssl)) {
//         return duk_generic_error(ctx, "invalid url");
// 	}
//     duk_push_this(ctx);
//     struct unity_websocket_t *websocket = duk_get_websocket(ctx, -1);
//     duk_pop(ctx); // pop this
//     if (websocket == NULL) {
//         duk_free(ctx, x_host);
//         duk_free(ctx, x_path);
//         return duk_generic_error(ctx, "no websocket");
//     }
// 	struct lws_context_creation_info info;
// 	struct lws_client_connect_info i;
    
// 	memset(&i, 0, sizeof(i));
// 	memset(&info, 0, sizeof(info));

// 	info.port = CONTEXT_PORT_NO_LISTEN;
// 	info.protocols = websocket->protocols;
// 	info.gid = -1;
// 	info.uid = -1;
// 	//info.ws_ping_pong_interval = 5;
// 	info.user = websocket; 
// 	struct lws_context *context = lws_create_context(&info);
//     if (context == NULL) {
//         duk_free(ctx, x_host);
//         duk_free(ctx, x_path);
//         return duk_generic_error(ctx, "lws_create_context failed");
//     }
//     websocket->context = context;
// 	i.context = context;
//     i.protocol = websocket->protocol_names;
//     if (x_ssl) {
// 		i.ssl_connection = LCCSCF_USE_SSL;
// 		if (p_allow_self_signed) {
//             i.ssl_connection |= LCCSCF_ALLOW_SELFSIGNED;
//         }
// 	} else {
// 		i.ssl_connection = 0;
// 	}
// 	i.address = x_host;
// 	i.host = x_host;
// 	i.path = x_path;
// 	i.port = x_port;

// 	lws_client_connect_via_info(&i);
//     duk_free(ctx, x_host);
//     duk_free(ctx, x_path);
//     return 0;
// }

// UNITY_LOCAL duk_ret_t duk_WebSocket_send(duk_context *ctx) {
//     unity_size_t len = 0;
//     unity_bool_t is_binary = _UNITY_TRUE;
//     void *buf = NULL;
//     duk_idx_t top = duk_get_top(ctx);
//     if (top == 0) {
//         return 0;
//     }
//     if (duk_is_string(ctx, 0)) {
//         buf = duk_get_lstring(ctx, 0, &len);
//         is_binary = _UNITY_FALSE;
//     } else if (duk_is_buffer_data(ctx, 0)) {
//         buf = duk_get_buffer_data(ctx, 0, &len);
//     }
//     if (len == 0) {
//         lwsl_warn("invalid payload object");
//         return 0;
//     }
//     if (len > LWS_PAYLOAD_SIZE) {
//         return duk_generic_error(ctx, "payload is too large");
//     }

//     duk_push_this(ctx);
//     struct unity_websocket_t *websocket = duk_get_websocket(ctx, -1);
//     duk_pop(ctx); // pop this
    
//     if (websocket) {
//         if (websocket->is_context_destroyed || websocket->is_closing) {
//             lwsl_warn("unable to send, websocket is closing");
//             return 0;
//         }
//         struct unity_websocket_payload_t *payload = _new_payload(websocket);
//         duk_memzero(payload->buf, LWS_PRE);
//         duk_memcpy(&(((char *)(payload->buf))[LWS_PRE]), buf, len);
//         payload->is_binary = is_binary;
//         payload->len = len;
//         struct unity_websocket_payload_t *tail = websocket->pending_tail;
//         if (tail) {
//             tail->next = payload;
//             websocket->pending_tail = payload;
//         } else {
//             websocket->pending_head = payload;
//             websocket->pending_tail = payload;
//         }
//         lws_callback_on_writable(websocket->wsi);
//     } 
//     return 0;
// }

// UNITY_LOCAL duk_ret_t duk_WebSocket_close(duk_context *ctx) {
//     duk_push_this(ctx);
//     struct unity_websocket_t *websocket = duk_get_websocket(ctx, -1);
//     duk_pop(ctx); // pop this
//     _duk_lws_close(websocket);
//     return 0;
// }

// UNITY_LOCAL duk_ret_t duk_WebSocket_poll(duk_context *ctx) {
//     duk_push_this(ctx);
//     struct unity_websocket_t *websocket = duk_get_websocket(ctx, -1);
//     duk_pop(ctx); // pop this
//     if (websocket == NULL || websocket->context == NULL) {
//         return 0;
//     }
//     websocket->is_polling = _UNITY_TRUE;
//     do {
//         websocket->is_servicing = _UNITY_FALSE;
//         lws_service(websocket->context, 0);
//     } while (websocket->is_servicing);
//     websocket->is_polling = _UNITY_FALSE;

//     if (websocket->is_context_destroying) {
//         _duk_lws_destroy(websocket);
//     }
//     return 0;
// }
