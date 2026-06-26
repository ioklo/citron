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

RDecl* RFuncDecl::GetRDecl()
{
    return visit([](auto* funcDecl) -> RDecl* { return funcDecl; }, v);
}

RThisKind RFuncDecl::GetThisKind()
{
    return visit([](auto* funcDecl) -> RThisKind { return funcDecl->GetThisKind(); }, v);
}

size_t RFuncDecl::GetTypeParamCount()
{
    return visit([](auto* funcDecl) -> size_t { return funcDecl->GetTypeParamCount(); }, v);
}

RTypeParamDecl* RFuncDecl::GetTypeParam(size_t index)
{
    return visit([index](auto* funcDecl) -> RTypeParamDecl* { return funcDecl->GetTypeParam(index); }, v);
}

size_t RFuncDecl::GetParamCount()
{
    return visit([](auto* funcDecl) -> size_t { return funcDecl->GetParamCount(); }, v);
}

RType* RFuncDecl::GetReturnType(RTypeArguments* typeArgs)
{
    return visit([typeArgs](auto* funcDecl) -> RType* { return funcDecl->GetReturnType(typeArgs); }, v);
}

RFuncReturn RFuncDecl::GetFuncReturn(RTypeArguments* typeArgs)
{
    return visit([typeArgs](auto* funcDecl) -> RFuncReturn { return funcDecl->GetFuncReturn(typeArgs); }, v);
}

RFuncParameter RFuncDecl::GetFuncParam(RTypeArguments* typeArgs, size_t index)
{
    return visit([typeArgs, index](auto* funcDecl) -> RFuncParameter { return funcDecl->GetFuncParam(typeArgs, index); }, v);
}

RFuncReturn RFuncDecl::GetUnboundFuncReturn()
{
    return visit([](auto* funcDecl) -> RFuncReturn { return funcDecl->GetUnboundFuncReturn(); }, v);
}

std::span<RFuncParameter> RFuncDecl::GetUnboundFuncParams()
{
    return visit([](auto* funcDecl) -> std::span<RFuncParameter> { return funcDecl->GetUnboundFuncParams(); }, v);
}

} // namespace Citron