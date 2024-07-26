#pragma once

#if defined(_MSC_VER)
    #if defined(SYMBOL_EXPORT)
        #define SYMBOL_API __declspec(dllexport)
    #else
        #define SYMBOL_API __declspec(dllimport)
    #endif
#elif defined(__GNUC__) || defined(__clang__)
    #if defined(SYMBOL_EXPORT)
        #define SYMBOL_API __attribute__((visibility ("default")))
    #else
        #define SYMBOL_API
    #endif
#endif