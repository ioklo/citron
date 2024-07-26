#pragma once

#if defined(_MSC_VER)
    #if defined(TEXTANALYSIS_EXPORT)
        #define TEXTANALYSIS_API __declspec(dllexport)
    #else
        #define TEXTANALYSIS_API __declspec(dllimport)
    #endif
#elif defined(__GNUC__) || defined(__clang__)
    #if defined(TEXTANALYSIS_EXPORT)
        #define TEXTANALYSIS_API __attribute__((visibility ("default")))
    #else
        #define TEXTANALYSIS_API
    #endif
#endif