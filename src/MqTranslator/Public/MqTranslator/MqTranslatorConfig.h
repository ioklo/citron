#pragma once

#if defined(_MSC_VER)
    #if defined(MQTRANSLATOR_EXPORTS)
        #define MQTRANSLATOR_API __declspec(dllexport)
    #else
        #define MQTRANSLATOR_API __declspec(dllimport)
    #endif
#elif defined(__GNUC__) || defined(__clang__)
    #if defined(MQTRANSLATOR_EXPORTS)
        #define MQTRANSLATOR_API __attribute__((visibility ("default")))
    #else
        #define MQTRANSLATOR_API
    #endif
#endif