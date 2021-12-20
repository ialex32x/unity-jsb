#pragma once

#include <v8.h>

namespace v8impl
{
	template<typename T>
	using Persistent = v8::Global<T>;

	template<typename T>
	using CopyPersistent = typename v8::CopyablePersistentTraits<T>::CopyablePersistent;
}

