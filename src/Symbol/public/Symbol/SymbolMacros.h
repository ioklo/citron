#pragma once

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
