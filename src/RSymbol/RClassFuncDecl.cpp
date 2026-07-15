#include "RClassFuncDecl.h"
#include "RClassDecl.h"
#include "RThisKind.h"

using namespace std;

namespace Citron {

RClassFuncDecl::RClassFuncDecl(RClassDecl* _class, RClassMemberAccessor accessor, bool bSeqFunc, TakeRef<RName> name)
    : _class{_class}
    , accessor{accessor}
    , name{name.Take()}
    , genericsComp{}
    , commonFuncDeclComp{bSeqFunc}
    , ImplRFuncDeclUsingCommonComponents{this, commonFuncDeclComp}
{
}

void RClassFuncDecl::InitFuncReturnAndParams(bool bStatic, RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic)
{
    commonFuncDeclComp.InitFuncReturnAndParams(
        move(funcReturn),
        bStatic ? (RThisKind)RThisKind_Static {} : RThisKind_Handle{_class->GetOpenType()},
        move(funcParameters),
        bLastParameterVariadic);
}

// from RDecl

RDecl* RClassFuncDecl::GetOuter()
{
    return _class;
}

RIdentifier RClassFuncDecl::GetIdentifier()
{
    return RIdentifier{name, commonFuncDeclComp.GetParamIds()};
}

size_t RClassFuncDecl::GetTypeParamCount()
{
    return genericsComp.GetTypeParamCount();
}

RTypeParamDecl* RClassFuncDecl::GetTypeParam(size_t index)
{
    return genericsComp.GetTypeParam(index);
}

RTypeDecl* RClassFuncDecl::GetTypeMember(InRef<RName> name)
{
    return genericsComp.GetTypeMember(name);
}

std::optional<RDeclRes> RClassFuncDecl::ResolveMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

std::optional<RDeclRes> RClassFuncDecl::ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    if (auto o_member = genericsComp.ResolveTypeParam(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    if (auto o_member = commonFuncDeclComp.ResolveFuncParam(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    return _class->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);
}

} // namespace Citron