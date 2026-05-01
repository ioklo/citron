#pragma once

#if defined(_MSC_VER)
    #if defined(QLTRANSLATOR_EXPORTS)
        #define QLTRANSLATOR_API __declspec(dllexport)
    #else
        #define QLTRANSLATOR_API __declspec(dllimport)
    #endif
#elif defined(__GNUC__) || defined(__clang__)
    #if defined(QLTRANSLATOR_EXPORTS)
        #define QLTRANSLATOR_API __attribute__((visibility ("default")))
    #else
        #define QLTRANSLATOR_API
    #endif
#endif