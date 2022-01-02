#include "WSServer.h"
#include "JSApi.h"
#include "JSInspectorClient.h"
#include <libwebsockets.h>
#include <string>
#include <cstdio>
#include <memory>
#include <queue>
#include <v8-version-string.h>

#include "JSContext.h"

static void custom_log(int level, const char* msg)
{
#if defined(JSB_EXEC_TEST)
	printf("%s", msg);
#endif
}

template<int N>
static bool _is_request(lws* wsi, lws_token_indexes token, const char (&expected)[N])
{
	static char buf[1024];
	int hlen = lws_hdr_total_length(wsi, token);
	if (!hlen || hlen != N - 1 || hlen > (int)sizeof(buf) - 1) {
		return false;
	}

	if (lws_hdr_copy(wsi, buf, sizeof(buf), token) < 0)
	{
		return false;
	}
	return memcmp(buf, expected, N - 1) == 0;
}

int _response_json(lws* wsi, http_status code, const char* content, int content_len)
{
	static unsigned char buf[4096];
	unsigned char* p = buf;
	unsigned char* end = p + sizeof(buf);
	if (lws_add_http_common_headers(wsi, HTTP_STATUS_OK, "application/json", content_len, &p, end))
	{
		return -1;
	}
	if (end - p - 1 - 2 < content_len)
	{
		return -1;
	}
	p[0] = '\r';
	p[1] = '\n';
	p += 2;
	memcpy(p, content, content_len);
	p[content_len] = '\0';
	if (lws_write_http(wsi, buf, (p - buf) + content_len))
	{
		return -1;
	}

	if (lws_http_transaction_completed(wsi))
	{
		return -1;
	}
	return 0;
}

struct WSBuffer
{
	size_t size;
	size_t capacity;
	unsigned char* buffer;
};

class WSServerImpl : public WSServer
{
public:
	void Open(int port);
	virtual void Update() override;
	virtual bool IsConnected() override;

	WSServerImpl(JSContext* ctx);
	virtual ~WSServerImpl();
private:
	void OnReceiveBegin();
	void OnReceive(unsigned char* buf, size_t len);
	void OnReceiveEnd(bool is_binary);
	void Start(lws* wsi, bool isDebugger = true);
	void Stop();
	void Send(unsigned char const* buf, size_t len);

	static void _JSDebuggerUpdate(JSContext* ctx);
	static void _JSDebuggerSend(JSContext* ctx, unsigned char type, const unsigned char* buf, size_t size);
	static int _v8_protocol_callback(struct lws* wsi, enum lws_callback_reasons reason, void* user, void* in, size_t len);
	static int _echo_callback(struct lws* wsi, enum lws_callback_reasons reason, void* user, void* in, size_t len);
	std::string _json_list;
	std::string _json_version;
	lws_protocols _protocols[3];
	lws_context* _ws_ctx;
	JSContext* _js_context;
	std::unique_ptr<JSDebugger> _debugger;

	lws* _wsi; // active wsi
	bool _is_polling;
	bool _is_servicing;
	bool _is_closing;

	size_t _recv_len;
	size_t _recv_cap;
	unsigned char* _recv_buffer;
	std::queue<WSBuffer*> _send_queue;
	std::vector<WSBuffer*> _free_queue;
};

WSServerImpl::WSServerImpl(JSContext* ctx)
	: _ws_ctx(nullptr), _js_context(ctx), _wsi(nullptr),
	_is_polling(false), _is_servicing(false), _is_closing(false),
	_recv_len(0), _recv_cap(0), _recv_buffer(nullptr)
{
}

void WSServerImpl::OnReceiveBegin()
{
	_recv_len = 0;
}

void WSServerImpl::OnReceive(unsigned char* buf, size_t len)
{
	size_t new_len = _recv_len + len;
	if (new_len > _recv_cap)
	{
		_recv_cap = max(_recv_cap + 4096, new_len);
		unsigned char* new_buffer = new unsigned char[_recv_cap];
		memcpy(new_buffer, _recv_buffer, _recv_len);
		delete[] _recv_buffer;
		_recv_buffer = new_buffer;
	}
	memcpy(&_recv_buffer[_recv_len], buf, len);
	_recv_len = new_len;
}

void WSServerImpl::OnReceiveEnd(bool is_binary)
{
	if (_debugger)
	{
		_debugger->OnMessageReceived(_recv_buffer, _recv_len);
		//lwsl_notice("receive message: %s %d %s", is_binary ? "binary" : "text", (int)_recv_len, _recv_buffer);
	}
}

void WSServerImpl::Send(unsigned char const* buf, size_t len)
{
	if (!_wsi)
	{
		lwsl_err("no connection to send");
		return;
	}

	WSBuffer* c = nullptr;
	size_t rlen = len + LWS_PRE;
	for (std::vector<WSBuffer*>::iterator i = _free_queue.begin(); i != _free_queue.end(); ++i)
	{
		if ((*i)->capacity >= rlen)
		{
			c = (*i);
			_free_queue.erase(i);
			break;
		}
	}

	if (!c)
	{
		c = new WSBuffer();
		c->capacity += max(rlen + 1, 512);
		c->buffer = new unsigned char[c->capacity];
	}

	//memset(c->buffer, 0, LWS_PRE);
	memcpy(&c->buffer[LWS_PRE], buf, len);
	c->size = len;
	c->buffer[LWS_PRE + len] = '\0';
	_send_queue.push(c);
	lws_callback_on_writable(_wsi);
	//printf("sending message %d (queue: %d): %s", (int)len, (int)_send_queue.size(), buf);
}

void WSServerImpl::Start(lws* wsi, bool isDebugger)
{
	_wsi = wsi;
	if (isDebugger)
	{
		_debugger.reset(JSDebugger::CreateDefaultDebugger(_js_context, { _JSDebuggerUpdate, _JSDebuggerSend }));
		_debugger->Open();
	}
}

void WSServerImpl::Stop()
{
	while (!_send_queue.empty()) 
	{
		WSBuffer* buffer = _send_queue.front();
		_send_queue.pop();
		_free_queue.push_back(buffer);
	}
	if (_debugger)
	{
		_debugger->Close();
		_debugger.reset();
	}
	_is_closing = true;
	_wsi = nullptr;
}

WSServerImpl::~WSServerImpl()
{
	while (!_send_queue.empty()) 
	{
		WSBuffer* f = _send_queue.front();
		_send_queue.pop();
		delete[] f->buffer;
		delete f;
	}
	for (WSBuffer* c : _free_queue)
	{
		delete[] c->buffer;
		delete c;
	}
	delete[] _recv_buffer;
	_debugger.reset();
	lws_context_destroy(_ws_ctx);
}

WSServer* WSServer::CreateDebugServer(JSContext* ctx, int port)
{
	WSServerImpl* s = new WSServerImpl(ctx);
	s->Open(port);
	return s;
}

void WSServerImpl::_JSDebuggerUpdate(JSContext* ctx)
{
	WSServerImpl* impl = (WSServerImpl*)ctx->_debugServer.get();
	if (impl)
	{
		impl->Update();
	}
}

void WSServerImpl::_JSDebuggerSend(JSContext* ctx, unsigned char type, const unsigned char* buf, size_t size)
{
	WSServerImpl* impl = (WSServerImpl*)ctx->_debugServer.get();
	if (impl)
	{
		impl->Send(buf, size);
	}
}

int WSServerImpl::_echo_callback(struct lws* wsi, enum lws_callback_reasons reason, void* user, void* in, size_t len)
{
	lws_context* ctx = lws_get_context(wsi);
	WSServerImpl* wss = (WSServerImpl*)lws_context_user(ctx);

	static int tick = 0;
	switch (reason)
	{
	case LWS_CALLBACK_ESTABLISHED:
		lwsl_notice("LWS_CALLBACK_CLIENT_ESTABLISHED");
		wss->Stop();
		wss->Start(wsi, false);
		break;
	case LWS_CALLBACK_CLOSED:
		wss->Stop();
		lwsl_notice("LWS_CALLBACK_CLIENT_CLOSED");
		return -1;
	case LWS_CALLBACK_RECEIVE:
		if (wss->_wsi != wsi)
		{
			lwsl_err("wrong connection");
			wss->Stop();
			return -1;
		}
		if (lws_is_first_fragment(wsi))
		{
			wss->OnReceiveBegin();
		}
		wss->OnReceive((unsigned char*)in, len);
		if (lws_is_final_fragment(wsi))
		{
			lwsl_notice("%p receive message", wsi);
			wss->OnReceiveEnd(lws_frame_is_binary(wsi) == 1);
			wss->Send((unsigned char const*)"hello1", 6);
			wss->Send((unsigned char const*)"hello2", 6);
			wss->Send((unsigned char const*)"hello3", 6);
			if (!wss->_send_queue.empty())
			{
				wss->_is_servicing = true;
				lws_callback_on_writable(wsi);
			}
		}
		break;
	case LWS_CALLBACK_CLIENT_WRITEABLE:
		lwsl_notice("LWS_CALLBACK_CLIENT_WRITEABLE");
		break;
	case LWS_CALLBACK_SERVER_WRITEABLE:
		if (!wss->_send_queue.empty())
		{
			if (wss->_wsi != wsi)
			{
				lwsl_err("wrong connection");
				return -1;
			}
			WSBuffer* buffer = wss->_send_queue.front();
			if (lws_write(wsi, &(buffer->buffer[LWS_PRE]), buffer->size, LWS_WRITE_TEXT) != (int)buffer->size)
			{
				lwsl_notice("connection write error");
				wss->Stop();
				return -1;
			}
			else
			{
				wss->_send_queue.pop();
				wss->_free_queue.push_back(buffer);
				lwsl_notice("LWS_CALLBACK_SERVER_WRITEABLE %p send message: [queue:%d] %d %s\n", wsi, (int)wss->_send_queue.size(), (int)buffer->size, &buffer->buffer[LWS_PRE]);
				if (!wss->_send_queue.empty())
				{
					lws_callback_on_writable(wsi);
				}
			}
		}
		break;
	default:
		break;
	}
	return 0;
}

int WSServerImpl::_v8_protocol_callback(struct lws* wsi, enum lws_callback_reasons reason, void* user, void* in, size_t len)
{
	lws_context* ctx = lws_get_context(wsi);
	WSServerImpl* wss = (WSServerImpl*)lws_context_user(ctx);

	//printf("servicing begin %d\n", reason);
	switch (reason)
	{
	case LWS_CALLBACK_ESTABLISHED:
		if (wss->_wsi)
		{
			lwsl_warn("supports only one connection");
			wss->Stop();
		}
		wss->Start(wsi);
		lwsl_debug("%p accept new incoming connection", wsi);
		break;
	case LWS_CALLBACK_RECEIVE:
		if (wss->_wsi != wsi)
		{
			lwsl_err("wrong connection");
			wss->Stop();
			return -1;
		}
		if (lws_is_first_fragment(wsi))
		{
			wss->OnReceiveBegin();
		}
		wss->OnReceive((unsigned char*)in, len);
		if (lws_is_final_fragment(wsi))
		{
			//lwsl_debug("%p receive message", wsi);
			wss->OnReceiveEnd(lws_frame_is_binary(wsi) == 1);
			if (!wss->_send_queue.empty())
			{
				wss->_is_servicing = true;
				lws_callback_on_writable(wsi);
			}
		}
		break;
	case LWS_CALLBACK_CLIENT_WRITEABLE:
	case LWS_CALLBACK_SERVER_WRITEABLE:
		if (wss->_is_closing)
		{
			lws_close_reason(wsi, LWS_CLOSE_STATUS_NORMAL, (unsigned char*)"", 0);
		}
		if (!wss->_send_queue.empty())
		{
			if (wss->_wsi != wsi)
			{
				lwsl_err("wrong connection");
				return -1;
			}
			WSBuffer* buffer = wss->_send_queue.front();
			if (lws_write(wsi, &(buffer->buffer[LWS_PRE]), buffer->size, LWS_WRITE_TEXT) != (int)buffer->size)
			{
				lwsl_err("connection write error");
				wss->Stop();
				return -1;
			}
			else 
			{
				wss->_send_queue.pop();
				wss->_free_queue.push_back(buffer);
				//lwsl_debug("%p send message: [queue:%d] %d %s\n", wsi, (int)wss->_send_queue.size(), (int)buffer->size, &buffer->buffer[LWS_PRE]);
				if (!wss->_send_queue.empty())
				{
					wss->_is_servicing = true;
					lws_callback_on_writable(wsi);
					//lws_rx_flow_control(wsi, 0);
				}
				else
				{
					//lws_rx_flow_control(wsi, 1);
				}
			}
		}
		break;
	case LWS_CALLBACK_CLOSED:
		if (wss->_wsi != wsi)
		{
			return -1;
		}
		wss->Stop();
		lwsl_debug("connection closed");
		break;
	case LWS_CALLBACK_CLIENT_CONNECTION_ERROR:
		if (wss->_wsi != wsi)
		{
			return -1;
		}
		wss->Stop();
		lwsl_debug("connection error");
		break;
	case LWS_CALLBACK_HTTP:
		if (_is_request(wsi, WSI_TOKEN_GET_URI, "/json") || _is_request(wsi, WSI_TOKEN_GET_URI, "/json/list"))
		{
			const char* content = wss->_json_list.c_str();
			int content_len = wss->_json_list.length();
			if (_response_json(wsi, HTTP_STATUS_OK, content, content_len))
			{
				return -1;
			}
		}
		else if (_is_request(wsi, WSI_TOKEN_GET_URI, "/json/version"))
		{
			const char* content = wss->_json_version.c_str();
			int content_len = wss->_json_version.length();
			if (_response_json(wsi, HTTP_STATUS_OK, content, content_len))
			{
				return -1;
			}
		}
		else 
		{
			if (lws_return_http_status(wsi, HTTP_STATUS_NOT_FOUND, nullptr))
			{
				return -1;
			}
		}
		break;
	case LWS_CALLBACK_HTTP_BODY_COMPLETION:
		//lwsl_notice("LWS_CALLBACK_HTTP_BODY_COMPLETION");
		if (lws_return_http_status(wsi, 200, nullptr))
		{
			return -1;
		}
		break;
	case LWS_CALLBACK_CLIENT_ESTABLISHED:
	case LWS_CALLBACK_CLIENT_CLOSED:
	case LWS_CALLBACK_CLIENT_RECEIVE:
		lwsl_warn("unexpected %d", reason);
		break;
	default:
		break;
	}
	//printf("servicing end\n");
	return 0;
}

// devtools://devtools/bundled/inspector.html?experiments=true&v8only=true&ws=127.0.0.1:9229/backend
void WSServerImpl::Open(int port)
{
	_json_version = \
		"{"
		"    \"Browser\": \"unity-jsb/1.7.1.0\","
		"    \"Protocol-Version\" : \"1.1\","
		"    \"User-Agent\" : \"unity-jsb/1.7\","
		"    \"V8-Version\" : \"" V8_VERSION_STRING "\",";
	_json_version += "	\"webSocketDebuggerUrl\" : \"ws://localhost:";
	_json_version += std::to_string(port);
	_json_version += "\"";
	_json_version += "}";

	_json_list = "[{";
	_json_list += "\"description\": \"unity-jsb (v8-bridge)\",";
	_json_list += "\"id\": \"0\",";
	_json_list += "\"title\": \"unity-jsb (v8-bridge)\",";
	_json_list += "\"type\": \"node\",";
	_json_list += "\"webSocketDebuggerUrl\" : \"ws://localhost:";
	_json_list += std::to_string(port);
	_json_list += "\"";
	_json_list += "}]";

#if defined(JSB_EXEC_TEST)
	lws_set_log_level(LLL_USER | LLL_DEBUG | LLL_NOTICE | LLL_ERR | LLL_WARN | LLL_INFO | LLL_CLIENT | LLL_THREAD, _js_context->_logCallback ? _js_context->_logCallback : custom_log);
#else 
	lws_set_log_level(LLL_USER | LLL_ERR | LLL_WARN, _js_context->_logCallback ? _js_context->_logCallback : custom_log);
#endif

	memset(_protocols, 0, sizeof(lws_protocols) * 3);
	_protocols[0].name = "binary";
	_protocols[0].callback = _v8_protocol_callback;
	_protocols[0].per_session_data_size = 0;
	_protocols[0].rx_buffer_size = (size_t)4 * 1024 * 1024;

	//_protocols[1].name = "echo";
	//_protocols[1].callback = _echo_callback;
	//_protocols[1].per_session_data_size = 0;
	//_protocols[1].rx_buffer_size = (size_t)4 * 1024 * 1024;

	lws_context_creation_info context_creation_info;
	memset(&context_creation_info, 0, sizeof(lws_context_creation_info));
	context_creation_info.port = port;
	context_creation_info.iface = nullptr;
	context_creation_info.protocols = _protocols;
	context_creation_info.extensions = nullptr;
	context_creation_info.gid = -1;
	context_creation_info.uid = -1;
	context_creation_info.options = 0;
	context_creation_info.user = this;
	//context_creation_info.options |= LWS_SERVER_OPTION_DO_SSL_GLOBAL_INIT;
	context_creation_info.options |= LWS_SERVER_OPTION_DISABLE_IPV6;

	_ws_ctx = lws_create_context(&context_creation_info);
	_debugger.reset(JSDebugger::CreateDefaultDebugger(_js_context, { _JSDebuggerUpdate, _JSDebuggerSend }));
	_debugger->Open();
}

bool WSServerImpl::IsConnected()
{
	return _debugger && _debugger->IsConnected();
}

void WSServerImpl::Update()
{
	_is_polling = true;
	//printf("loop begin\n");
	lws_service(_ws_ctx, 0);
	lws_callback_on_writable_all_protocol(_ws_ctx, _protocols);
	//printf("loop end\n");
	_is_polling = false;
}
