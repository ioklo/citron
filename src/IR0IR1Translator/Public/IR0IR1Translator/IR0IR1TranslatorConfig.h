#pragma once

#if defined(_MSC_VER)
    #if defined(IR0IR1TRANSLATOR_EXPORT)
        #define IR0IR1TRANSLATOR_API __declspec(dllexport)
    #else
        #define IR0IR1TRANSLATOR_API __declspec(dllimport)
    #endif
#elif defined(__GNUC__) || defined(__clang__)
    #if defined(IR0IR1TRANSLATOR_EXPORT)
        #define IR0IR1TRANSLATOR_API __attribute__((visibility ("default")))
    #else
        #define IR0IR1TRANSLATOR_API
    #endif
#endif