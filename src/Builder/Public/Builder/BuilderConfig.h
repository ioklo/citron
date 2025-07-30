#pragma once

#if defined(_MSC_VER)
    #if defined(BUILDER_EXPORT)
        #define BUILDER_API __declspec(dllexport)
    #else
        #define BUILDER_API __declspec(dllimport)
    #endif
#elif defined(__GNUC__) || defined(__clang__)
    #if defined(BUILDER_EXPORT)
        #define BUILDER_API __attribute__((visibility ("default")))
    #else
        #define BUILDER_API
    #endif
#endif