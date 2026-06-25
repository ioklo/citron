#include "RFuncDecl.h"

#include <variant>

#include "RGlobalFuncDecl.h"
#include "RClassCtorDecl.h"
#include "RClassFuncDecl.h"
#include "RStructCtorDecl.h"
#include "RStructDtorDecl.h"
#include "RStructFuncDecl.h"
#include "RLambdaDecl.h"

using namespace std;

namespace Citron {

RDecl* GetRDecl(RFuncDecl& funcDecl)
{
    return visit([](auto* funcDecl) -> RDecl* { return funcDecl; }, funcDecl);
}

RThisKind GetThisKind(RFuncDecl& funcDecl)
{
    return visit([](auto* funcDecl) -> RThisKind { return funcDecl->GetThisKind(); }, funcDecl);
}

size_t GetTypeParamCount(RFuncDecl& funcDecl)
{
    return visit([](auto* funcDecl) -> size_t { return funcDecl->GetTypeParamCount(); }, funcDecl);
}

RTypeParamDecl* GetTypeParam(RFuncDecl& funcDecl, size_t index)
{
    return visit([index](auto* funcDecl) -> RTypeParamDecl* { return funcDecl->GetTypeParam(index); }, funcDecl);
}

size_t GetParamCount(RFuncDecl& funcDecl)
{
    return visit([](auto* funcDecl) -> size_t { return funcDecl->GetParamCount(); }, funcDecl);
}

RType* GetReturnType(RFuncDecl& funcDecl, RTypeArguments* typeArgs)
{
    return visit([typeArgs](auto* funcDecl) -> RType* { return funcDecl->GetReturnType(typeArgs); }, funcDecl);
}

RFuncReturn GetFuncReturn(RFuncDecl& funcDecl, RTypeArguments* typeArgs)
{
    return visit([typeArgs](auto* funcDecl) -> RFuncReturn { return funcDecl->GetFuncReturn(typeArgs); }, funcDecl);
}

RFuncParameter GetFuncParam(RFuncDecl& funcDecl, RTypeArguments* typeArgs, size_t index)
{
    return visit([typeArgs, index](auto* funcDecl) -> RFuncParameter { return funcDecl->GetFuncParam(typeArgs, index); }, funcDecl);
}

RFuncReturn GetUnboundFuncReturn(RFuncDecl& funcDecl)
{
    return visit([](auto* funcDecl) -> RFuncReturn { return funcDecl->GetUnboundFuncReturn(); }, funcDecl);
}

std::span<RFuncParameter> GetUnboundFuncParams(RFuncDecl& funcDecl)
{
    return visit([](auto* funcDecl) -> std::span<RFuncParameter> { return funcDecl->GetUnboundFuncParams(); }, funcDecl);
}

} // namespace Citron