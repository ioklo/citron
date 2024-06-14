#pragma once

#define DECLARE_DEFAULTS(className) \
    className(const className&) = delete; \
    className(className&&); \
    className& operator=(const className&) = delete; \
    className& operator=(className&&); \
    ~className(); \
    className Copy() const;

#define IMPLEMENT_DEFAULTS(className) \
    className::className(className&&) = default; \
    className& className::operator=(className&&) = default; \
    className::~className() = default;
