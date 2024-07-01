#pragma once

#if defined(_MSC_VER)
    #if defined(TEXT_ANALYSIS_EXPORT)
        #define TEXT_ANALYSIS_API __declspec(dllexport)
    #else
        #if defined(_DLL)
            #define TEXT_ANALYSIS_API __declspec(dllimport)
        #else
            #define TEXT_ANALYSIS_API
        #endif
    #endif
#elif defined(__GNUC__) || defined(__clang__)
    #if defined(TEXT_ANALYSIS_EXPORT)
        #define TEXT_ANALYSIS_API __attribute__((visibility ("default")))
    #else
        #define TEXT_ANALYSIS_API
    #endif
#endif