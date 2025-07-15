#pragma once

#if defined(_MSC_VER)
    #if defined(COMPILER_EXPORT)
        #define COMPILER_API __declspec(dllexport)
    #else
        #define COMPILER_API __declspec(dllimport)
    #endif
#elif defined(__GNUC__) || defined(__clang__)
    #if defined(COMPILER_EXPORT)
        #define COMPILER_API __attribute__((visibility ("default")))
    #else
        #define COMPILER_API
    #endif
#endif