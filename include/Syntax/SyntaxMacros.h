#pragma once

#define DECLARE_DEFAULTS(ClassName) \
    SYNTAX_API ClassName(const ClassName&); \
    SYNTAX_API ClassName(ClassName&&) noexcept; \
    SYNTAX_API ~ClassName(); \
    SYNTAX_API ClassName& operator=(const ClassName&); \
    SYNTAX_API ClassName& operator=(ClassName&&) noexcept;

#define IMPLEMENT_DEFAULTS_PIMPL(ClassName) \
ClassName::ClassName(const ClassName& other) \
{ \
    impl = std::make_unique<Impl>(*other.impl); \
} \
\
ClassName::ClassName(ClassName&& other) noexcept \
{ \
    impl = std::move(other.impl); \
} \
ClassName::~ClassName() = default; \
ClassName& ClassName::operator=(const ClassName& other) \
{ \
    impl = std::make_unique<Impl>(*other.impl); \
    return *this; \
} \
ClassName& ClassName::operator=(ClassName&& other) noexcept \
{ \
    impl = std::move(other.impl); \
    return *this; \
}

#define IMPLEMENT_DEFAULTS_DEFAULT(ClassName) \
ClassName::ClassName(const ClassName& other) = default;\
ClassName::ClassName(ClassName&& other) noexcept = default;\
ClassName::~ClassName() = default; \
ClassName& ClassName::operator=(const ClassName& other) = default;\
ClassName& ClassName::operator=(ClassName&& other) noexcept = default;\
