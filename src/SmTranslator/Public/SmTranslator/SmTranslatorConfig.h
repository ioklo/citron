#pragma once

#if defined(_MSC_VER)
    #if defined(SMTRANSLATOR_EXPORTS)
        #define SMTRANSLATOR_API __declspec(dllexport)
    #else
        #define SMTRANSLATOR_API __declspec(dllimport)
    #endif
#elif defined(__GNUC__) || defined(__clang__)
    #if defined(SMTRANSLATOR_EXPORTS)
        #define SMTRANSLATOR_API __attribute__((visibility ("default")))
    #else
        #define SMTRANSLATOR_API
    #endif
#endif