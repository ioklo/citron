#pragma once

#ifdef SYMBOL_EXPORT
    #define SYMBOL_API __declspec(dllexport)
#else
    #ifdef _DLL
        #define SYMBOL_API __declspec(dllimport)
    #else
        #define SYMBOL_API
    #endif
#endif