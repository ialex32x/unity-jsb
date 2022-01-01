#include <v8-inspector.h>
#include <v8.h>

#include "JSContext.h"
#include "JSInspectorClient.h"

#include <codecvt>
#include <cuchar>
#include <locale>
#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

class JSInspectorClient : public JSDebugger, public v8_inspector::V8InspectorClient
{
	class JSInspectorChannel : public v8_inspector::V8Inspector::Channel
	{
	public:
		JSInspectorChannel(JSInspectorClient* client, int groupId);
		virtual ~JSInspectorChannel();

		void Initialize();
		void OnMessageReceived(const unsigned char* buf, size_t len);

		virtual void sendResponse(int callId, std::unique_ptr<v8_inspector::StringBuffer> message) override;
		virtual void sendNotification(std::unique_ptr<v8_inspector::StringBuffer> message) override;
		virtual void flushProtocolNotifications() override;
	private:
		void SendString(v8_inspector::StringView view);

		int _groupId;
		JSInspectorClient* _client;
		std::unique_ptr<v8_inspector::V8InspectorSession> _session;
	};

	enum EClientState
	{
		ECS_NONE,
		ECS_READY,
		ECS_PAUSED,
	};
public:
	JSInspectorClient(JSContext* ctx, JSDebuggerCallbacks callbacks);
	virtual ~JSInspectorClient();

	virtual void runMessageLoopOnPause(int contextGroupId) override;
	virtual void quitMessageLoopOnPause() override;
	virtual void runIfWaitingForDebugger(int contextGroupId) override;

	virtual void Open() override;
	virtual void Close() override;
	virtual void Update() override;
	virtual void OnConnectionClosed() override;
	virtual void OnMessageReceived(const unsigned char* buf, size_t len) override;
private:
	std::unique_ptr<v8_inspector::V8Inspector> _inspector;
	JSDebuggerCallbacks _callbacks;
	JSContext* _ctx;
	std::unique_ptr<JSInspectorChannel> _channel;
	EClientState _state;
};

JSDebugger* JSDebugger::CreateDefaultDebugger(JSContext* ctx, JSDebuggerCallbacks callbacks)
{
	return new JSInspectorClient(ctx, callbacks);
}

JSInspectorClient::JSInspectorChannel::JSInspectorChannel(JSInspectorClient* client, int groupId)
	:_client(client), _groupId(groupId)
{
	v8_inspector::StringView state;
	_session = client->_inspector->connect(groupId, this, state);
}

JSInspectorClient::JSInspectorChannel::~JSInspectorChannel()
{
	_session.reset();
}

void JSInspectorClient::JSInspectorChannel::Initialize()
{
	
}

void JSInspectorClient::JSInspectorChannel::OnMessageReceived(const unsigned char* buf, size_t len)
{
	if (_session)
	{
		v8::Isolate* isolate = _client->_ctx->GetIsolate();
		v8::Isolate::Scope isolateScope(isolate);
		v8::HandleScope handleScope(isolate);
		v8::TryCatch try_catch(isolate);

		v8_inspector::StringView message(buf, len);
		_session->dispatchProtocolMessage(message);
		if (try_catch.HasCaught())
		{
			//printf("inspector exception: %s\n", buf);
			return;
		}
		//printf("inspector dispatch: %d\n", (int)len);
	}
	else
	{
		//printf("invalid session\n");
	}
}

void JSInspectorClient::JSInspectorChannel::SendString(v8_inspector::StringView view)
{
	if (view.is8Bit())
	{
		//printf("inspector send 8: %d\n", (int)view.length());
		_client->_callbacks.send(_client->_ctx, 0, view.characters8(), view.length());
	}
	else
	{
		int cap = WideCharToMultiByte(CP_UTF8, 0, (LPCWCH)view.characters16(), view.length(), NULL, 0, NULL, NULL);
		std::string str(cap, 0);
		WideCharToMultiByte(CP_UTF8, 0, (LPCWCH)view.characters16(), view.length(), (LPSTR)str.c_str(), cap, NULL, NULL);
		//std::wstring_convert<std::codecvt_utf8_utf16<uint16_t>, uint16_t> Conv;
		//const uint16_t* Start = view.characters16();
		//std::string str = Conv.to_bytes(Start, Start + view.length());

		//printf("inspector send 16: %d\n", (int)str.length());
		_client->_callbacks.send(_client->_ctx, 0, reinterpret_cast<const unsigned char*>(str.c_str()), str.length());
	}
}

void JSInspectorClient::JSInspectorChannel::sendResponse(int callId, std::unique_ptr<v8_inspector::StringBuffer> message)
{
	SendString(message->string());
}

void JSInspectorClient::JSInspectorChannel::sendNotification(std::unique_ptr<v8_inspector::StringBuffer> message)
{
	SendString(message->string());
}

void JSInspectorClient::JSInspectorChannel::flushProtocolNotifications()
{
}

JSInspectorClient::JSInspectorClient(JSContext* ctx, JSDebuggerCallbacks callbacks)
	:_ctx(ctx), _callbacks(callbacks), _state(ECS_NONE), _channel(nullptr)
{
}

JSInspectorClient::~JSInspectorClient()
{
	Close();
}

void JSInspectorClient::runMessageLoopOnPause(int contextGroupId)
{
	if (_state == ECS_READY)
	{
		_state = ECS_PAUSED;
		while (_state == ECS_PAUSED)
		{
			Update();
		}
	}
}

void JSInspectorClient::quitMessageLoopOnPause()
{
	if (_state == ECS_PAUSED)
	{
		_state = ECS_READY;
	}
}

void JSInspectorClient::runIfWaitingForDebugger(int contextGroupId)
{
	//printf("runIfWaitingForDebugger %d\n", _state);
}

void JSInspectorClient::Open()
{
	if (_state == ECS_NONE)
	{
		v8::Isolate* isolate = _ctx->GetIsolate();
		v8::Isolate::Scope isolateScope(isolate);
		v8::HandleScope handleScope(isolate);

		int contextGroupId = 1;
		_inspector = v8_inspector::V8Inspector::create(isolate, this);
		const uint8_t p_name[] = "DefaultInsepctorContext";
		v8_inspector::StringView name(p_name, sizeof(p_name) - 1);
		_channel.reset(new JSInspectorChannel(this, contextGroupId));
		_inspector->contextCreated(v8_inspector::V8ContextInfo(_ctx->Get(), contextGroupId, name));
		_state = ECS_READY;
		//printf("open inspector\n");
	}
}

void JSInspectorClient::Close()
{
	if (_state != ECS_NONE)
	{
		_state = ECS_NONE;
		v8::Isolate* isolate = _ctx->GetIsolate();
		v8::Isolate::Scope isolateScope(isolate);
		v8::HandleScope handleScope(isolate);

		_channel.reset();
		_inspector->contextDestroyed(_ctx->Get());
		_inspector.reset();
		//printf("close inspector\n");
	}
}

void JSInspectorClient::OnMessageReceived(const unsigned char* buf, size_t len)
{
	if (_channel)
	{
		_channel->OnMessageReceived(buf, len);
	}
}

void JSInspectorClient::OnConnectionClosed()
{
	_channel.reset();
}

void JSInspectorClient::Update()
{
	_callbacks.update(_ctx);
}
