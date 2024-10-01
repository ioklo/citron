#pragma once

#if defined(_MSC_VER)
    #if defined(LOGGING_EXPORT)
        #define LOGGING_API __declspec(dllexport)
    #else
        #define LOGGING_API __declspec(dllimport)
    #endif

#elif defined(__GNUC__) || defined(__clang__)
    #if defined(LOGGING_EXPORT)
        #define LOGGING_API __attribute__((visibility ("default")))
    #else
        #define LOGGING_API
    #endif
#endif
