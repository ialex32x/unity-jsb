#pragma once

#include "QuickJSCompatible.h"

class WSServer
{
public:
	virtual bool IsConnected() = 0;
	virtual void Update(bool bDebugging) = 0;
	virtual ~WSServer() {}

	static WSServer* CreateDebugServer(JSContext* ctx, int port);
};