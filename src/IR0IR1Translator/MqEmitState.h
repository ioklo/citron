#pragma once
#include <optional>

namespace Citron {

struct MqEmitState_Ready {};
struct MqEmitState_Done {};

template<typename T>
struct MqEmitState
{
    std::optional<T> o_value;

    template<typename TT>
    MqEmitState(TT&& value) { o_value.emplace(std::forward<TT>(value)); }
    MqEmitState(MqEmitState_Done) : o_value{std::nullopt} {}

    operator bool() { return o_value.has_value(); }
    T* operator->() { return &*o_value; }
    T& operator*() { return *o_value; }
};

template<>
struct MqEmitState<void>
{
    bool bReady;
    operator bool() const { return bReady; }

    MqEmitState(MqEmitState_Ready) : bReady{true} {}
    MqEmitState(MqEmitState_Done) : bReady{false} {}
};


} // namespace Citron

#define RETURN_ON_ERROR_OR_DONE(e) \
    RETURN_ON_ERROR(e); \
    do { if (!*e) return MqEmitState_Done{}; } while(0)