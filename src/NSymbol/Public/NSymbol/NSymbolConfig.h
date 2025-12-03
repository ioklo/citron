#pragma once

#if defined(_MSC_VER)
    #if defined(NSYMBOL_EXPORTS)
        #define NSYMBOL_API __declspec(dllexport)
    #else
        #define NSYMBOL_API __declspec(dllimport)
    #endif
#elif defined(__GNUC__) || defined(__clang__)
    #if defined(NSYMBOL_EXPORTS)
        #define NSYMBOL_API __attribute__((visibility ("default")))
    #else
        #define NSYMBOL_API
    #endif
#endif