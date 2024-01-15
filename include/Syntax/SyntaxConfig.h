#pragma once

#ifdef SYNTAX_EXPORT
    #define SYNTAX_API __declspec(dllexport)
#else
    #ifdef _DLL
        #define SYNTAX_API __declspec(dllimport)
    #else
        #define SYNTAX_API
    #endif
#endif