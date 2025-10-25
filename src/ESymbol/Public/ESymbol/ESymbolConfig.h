#pragma once

#if defined(_MSC_VER)
    #if defined(ESYMBOL_EXPORTS)
        #define ESYMBOL_API __declspec(dllexport)
    #else
        #define ESYMBOL_API __declspec(dllimport)
    #endif
#elif defined(__GNUC__) || defined(__clang__)
    #if defined(ESYMBOL_EXPORTS)
        #define ESYMBOL_API __attribute__((visibility ("default")))
    #else
        #define ESYMBOL_API
    #endif
#endif