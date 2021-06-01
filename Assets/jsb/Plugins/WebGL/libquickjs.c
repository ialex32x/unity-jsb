
#define UNITY_WEBGL
#define EMSCRIPTEN

// #define DUMP_MEM
// #define CONFIG_BIGNUM

#define CONFIG_VERSION "quickjs-latest"

#define compute_stack_size compute_stack_size_regexp
#define is_digit is_digit_regexp
#include "../../../../jsb_build/quickjs/quickjs-latest/libregexp.c"
#undef is_digit
#undef compute_stack_size

#include "../../../../jsb_build/quickjs/quickjs-latest/cutils.c"
#include "../../../../jsb_build/quickjs/quickjs-latest/libunicode.c"

#ifdef CONFIG_BIGNUM
#define floor_div floor_div_bbf
#define to_digit to_digit_bbf
#include "../../../../jsb_build/quickjs/quickjs-latest/libbf.c"
#undef floor_div
#undef to_digit
#undef malloc
#undef free
#undef realloc
#endif

#include "../../../../jsb_build/quickjs/quickjs-latest/quickjs.c"

#define DEF(name, str) \
    JSAtom JSB_ATOM_##name() { return JS_ATOM_##name; }
#include "../../../../jsb_build/quickjs/quickjs-latest/quickjs-atom.h"
#undef DEF

#include "../../../../jsb_build/quickjs/unity_qjs.c"
