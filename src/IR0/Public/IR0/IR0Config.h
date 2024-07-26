#pragma once

#if defined(_MSC_VER)
    #if defined(IR0_EXPORT)
        #define IR0_API __declspec(dllexport)
    #else
        #define IR0_API __declspec(dllimport)
    #endif
#elif defined(__GNUC__) || defined(__clang__)
    #if defined(IR0_EXPORT)
        #define IR0_API __attribute__((visibility ("default")))
    #else
        #define IR0_API
    #endif
#endif