#pragma once

#if defined(_MSC_VER)
    #if defined(QIRLLVMTRANSLATOR_EXPORTS)
        #define QIRLLVMTRANSLATOR_API __declspec(dllexport)
    #else
        #define QIRLLVMTRANSLATOR_API __declspec(dllimport)
    #endif
#elif defined(__GNUC__) || defined(__clang__)
    #if defined(QIRLLVMTRANSLATOR_EXPORTS)
        #define QIRLLVMTRANSLATOR_API __attribute__((visibility ("default")))
    #else
        #define QIRLLVMTRANSLATOR_API
    #endif
#endif