#pragma once

#ifdef TEXT_ANALYSIS_EXPORT
    #define TEXT_ANALYSIS_API __declspec(dllexport)
#else
    #ifdef _DLL
        #define TEXT_ANALYSIS_API __declspec(dllimport)
    #else
        #define SYNTAX_API
    #endif
#endif