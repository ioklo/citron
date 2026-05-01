#pragma once
#include "MqBodyContext.h"
namespace Citron {

struct MqScopeGuard
{
    MqBodyContext& bodyContext;
    bool needCleanUp;

    MqScopeGuard(std::optional<size_t> o_labelId, MqBodyContext& bodyContext)
        : bodyContext{bodyContext}
        , needCleanUp{true}
    {
        bodyContext.PushScope(o_labelId);
    }

    void SetDontNeedCleanUp() { needCleanUp = false; }

    ~MqScopeGuard()
    {   
        if (needCleanUp)
            bodyContext.CleanUpScope();
        
        bodyContext.PopScope();
    }
};

} // namespace Citron
