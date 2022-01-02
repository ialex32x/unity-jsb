#pragma once

#include "QuickJSCompatible.h"

struct JSContext;
class JSInspectorChannel;

class JSDebugger
{
public:
	virtual void Open() = 0;
	virtual void Close() = 0;
	virtual void Update() = 0;
	virtual bool IsConnected() = 0;

	virtual void OnConnectionClosed() = 0;
	virtual void OnMessageReceived(const unsigned char* buf, size_t len) = 0;

	static JSDebugger* CreateDefaultDebugger(JSContext* ctx, JSDebuggerCallbacks callbacks);
};
