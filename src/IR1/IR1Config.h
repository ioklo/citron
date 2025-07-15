#pragma once

#if defined(_MSC_VER)
    #if defined(IR1_EXPORT)
        #define IR1_API __declspec(dllexport)
    #else
        #define IR1_API __declspec(dllimport)
    #endif
#elif defined(__GNUC__) || defined(__clang__)
    #if defined(IR1_EXPORT)
        #define IR1_API __attribute__((visibility ("default")))
    #else
        #define IR1_API
    #endif
#endif