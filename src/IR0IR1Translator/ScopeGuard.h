#pragma once
#include "QBodyContext.h"
namespace Citron {

struct ScopeGuard
{
    QBodyContext& bodyContext;
    ScopeGuard(QBodyContext& bodyContext)
        : bodyContext{bodyContext}
    {
        bodyContext.PushScope();
    }

    ~ScopeGuard()
    {
        // 이 스코프에서 리턴을 처리했다면 (다음으로 진행이 되지 않는다면)
        if (!bodyContext.IsReturnHandledOnCurScope())
            bodyContext.CleanUpScope();
        
        bodyContext.PopScope();
    }
};

} // namespace Citron
