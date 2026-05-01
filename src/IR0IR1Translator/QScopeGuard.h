#pragma once
#include "QBodyContext.h"
namespace Citron {

struct QScopeGuard
{
    QBodyContext& bodyContext;
    bool needCleanUp;

    QScopeGuard(std::optional<size_t> o_labelId, QBodyContext& bodyContext)
        : bodyContext{bodyContext}
        , needCleanUp{true}
    {
        bodyContext.PushScope(o_labelId);
    }

    void SetDontNeedCleanUp() { needCleanUp = false; }

    ~QScopeGuard()
    {   
        if (needCleanUp)
            bodyContext.CleanUpScope();
        
        bodyContext.PopScope();
    }
};

} // namespace Citron
