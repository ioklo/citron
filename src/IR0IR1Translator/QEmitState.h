#pragma once
#include <optional>

namespace Citron {

struct QEmitState_Ready {};
struct QEmitState_Done {};

template<typename T>
struct QEmitState
{
    std::optional<T> o_value;

    template<typename TT>
    QEmitState(TT&& value) { o_value.emplace(std::forward<TT>(value)); }
    QEmitState(QEmitState_Done) : o_value{std::nullopt} {}

    operator bool() { return o_value.has_value(); }
    T* operator->() { return &*o_value; }
    T& operator*() { return *o_value; }
};

template<>
struct QEmitState<void>
{
    bool bReady;
    operator bool() const { return bReady; }

    QEmitState(QEmitState_Ready) : bReady{true} {}
    QEmitState(QEmitState_Done) : bReady{false} {}
};


} // namespace Citron

#define RETURN_ON_ERROR_OR_DONE(e) \
    RETURN_ON_ERROR(e); \
    do { if (!*e) return QEmitState_Done{}; } while(0)