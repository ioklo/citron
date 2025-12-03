#pragma once

#if defined(_MSC_VER)
    #if defined(QIR_EXPORTS)
        #define QIR_API __declspec(dllexport)
    #else
        #define QIR_API __declspec(dllimport)
    #endif
#elif defined(__GNUC__) || defined(__clang__)
    #if defined(QIR_EXPORTS)
        #define QIR_API __attribute__((visibility ("default")))
    #else
        #define QIR_API
    #endif
#endif