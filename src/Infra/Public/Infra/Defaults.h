#pragma once

#define DECLARE_DEFAULTS(linkage, className) \
    className(const className&) = delete; \
    linkage className(className&&); \
    className& operator=(const className&) = delete; \
    linkage className& operator=(className&&); \
    linkage ~className(); \
    linkage className Copy() const;

#define IMPLEMENT_DEFAULTS(className) \
    className::className(className&&) = default; \
    className& className::operator=(className&&) = default; \
    className::~className() = default;
