#pragma once

#if defined(_MSC_VER)
    #if defined(SYNTAXIR0TRANSLATOR_EXPORT)
        #define SYNTAXIR0TRANSLATOR_API __declspec(dllexport)
    #else
        #define SYNTAXIR0TRANSLATOR_API __declspec(dllimport)
    #endif
#elif defined(__GNUC__) || defined(__clang__)
    #if defined(SYNTAXIR0TRANSLATOR_EXPORT)
        #define SYNTAXIR0TRANSLATOR_API __attribute__((visibility ("default")))
    #else
        #define SYNTAXIR0TRANSLATOR_API
    #endif
#endif