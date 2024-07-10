#pragma once

#if defined(_MSC_VER)
    #if defined(SYNTAX_EXPORT)
        #define SYNTAX_API __declspec(dllexport)
    #else
        #if defined(SYNTAX_IMPORT)
            #define SYNTAX_API __declspec(dllimport)
        #else
            #define SYNTAX_API
        #endif
    #endif
#elif defined(__GNUC__) || defined(__clang__)
    #if defined(SYNTAX_EXPORT)
        #define SYNTAX_API __attribute__((visibility ("default")))
    #else
        #define SYNTAX_API
    #endif
#endif