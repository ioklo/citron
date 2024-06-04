#pragma once

#define DECLARE_DEFAULTS(className) \
    className(const className&) = delete; \
    className(className&&) = default; \
    className& operator=(const className&) = delete; \
    className& operator=(className&&) = default; \
    ~className(); \
    className Copy() const;
