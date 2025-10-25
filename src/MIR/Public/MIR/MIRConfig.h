#pragma once

#if defined(_MSC_VER)
    #if defined(MIR_EXPORTS)
        #define MIR_API __declspec(dllexport)
    #else
        #define MIR_API __declspec(dllimport)
    #endif
#elif defined(__GNUC__) || defined(__clang__)
    #if defined(MIR_EXPORTS)
        #define MIR_API __attribute__((visibility ("default")))
    #else
        #define MIR_API
    #endif
#endif