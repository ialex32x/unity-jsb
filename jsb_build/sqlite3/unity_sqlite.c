
#define SQLITE_EXTERN

#if defined(WIN32) || defined(_WIN32)
    #define SQLITE_API extern __declspec(dllexport)
#else
    #define SQLITE_API extern
#endif

#include "sqlite3.c"
