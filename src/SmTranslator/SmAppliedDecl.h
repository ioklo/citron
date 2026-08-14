#pragma once
#include <vector>

namespace Citron {

class SmType;

template<typename TRDecl>
struct SmAppliedDecl // 반드시 계산시에는 TypeEnv를 동반해야 한다.
{
    TRDecl* decl;
    std::vector<SmType*> typeArgs;
};

} // namespace Citron
