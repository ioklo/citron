#pragma once

#ifdef INFRA_EXPORT
    #define INFRA_API __declspec(dllexport)
#else
    #ifdef _DLL
        #define INFRA_API __declspec(dllimport)
#else
        #define INFRA_API
    #endif
#endif