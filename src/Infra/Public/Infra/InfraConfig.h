#pragma once

#if defined(_MSC_VER)
    #if defined(INFRA_EXPORT)
        #define INFRA_API __declspec(dllexport)
    #else
        #define INFRA_API __declspec(dllimport)
    #endif
#elif defined(__GNUC__) || defined(__clang__)
    #if defined(INFRA_EXPORT)
        #define INFRA_API __attribute__((visibility ("default")))
    #else
        #define INFRA_API
    #endif
#endif