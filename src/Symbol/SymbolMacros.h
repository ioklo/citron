#pragma once

#if defined(_MSC_VER)
    #if defined(SYMBOL_EXPORT)
        #define SYMBOL_API __declspec(dllexport)
    #else
        #if defined(SYMBOL_IMPORT)
            #define SYMBOL_API __declspec(dllimport)
        #else
            #define SYMBOL_API
        #endif
    #endif

    #elif defined(__GNUC__) || defined(__clang__)
    #if defined(SYMBOL_EXPORT)
        #define SYMBOL_API __attribute__((visibility ("default")))
    #else
        #define SYMBOL_API
    #endif
#endif


#define DECLARE_DEFAULTS(className) \
    className(const className&) = delete; \
    SYMBOL_API className(className&&); \
    className& operator=(const className&) = delete; \
    SYMBOL_API className& operator=(className&&); \
    SYMBOL_API ~className(); \
    SYMBOL_API className Copy() const;

#define IMPLEMENT_DEFAULTS(className) \
    className::className(className&&) = default; \
    className& className::operator=(className&&) = default; \
    className::~className() = default;
