#pragma once

#if defined(_MSC_VER)
    #if defined(RSYMBOL_EXPORTS)
        #define RSYMBOL_API __declspec(dllexport)
    #else
        #define RSYMBOL_API __declspec(dllimport)
    #endif
#elif defined(__GNUC__) || defined(__clang__)
    #if defined(RSYMBOL_EXPORTS)
        #define RSYMBOL_API __attribute__((visibility ("default")))
    #else
        #define RSYMBOL_API
    #endif
#endif