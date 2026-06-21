#include "NClassFuncDecl.h"

#include <cassert>
#include "Infra/Exceptions.h"

#include "NClassDecl.h"

using namespace std;

namespace Citron {

NClassFuncDecl::NClassFuncDecl(NClassDecl* _class, RAccessor accessor, RName&& name, bool bStatic, bool bSeqFunc)
    : _class{_class}, accessor{accessor}, name{move(name)}, _static{bStatic}
    , NCommonFuncDeclComponent{bSeqFunc}
{
}

void NClassFuncDecl::Init(RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic)
{
    NCommonFuncDeclComponent::InitFuncReturnAndParams(
        move(funcReturn), 
        _static ? (RThisKind)RThisKind_Static{} : RThisKind_Handle{_class->GetOpenType()}, 
        move(funcParameters), 
        bLastParameterVariadic);
}

NDecl* NClassFuncDecl::GetNOuter()
{
    return _class;
}

RDecl* NClassFuncDecl::GetROuter()
{
    return _class;
}

RIdentifier NClassFuncDecl::GetIdentifier()
{
    return RIdentifier{ name, NGenericsComponent::GetTypeParamCount(), NCommonFuncDeclComponent::GetParamIds() };
}

RTypeDecl* NClassFuncDecl::GetTypeMember(const RName& name, size_t typeParamCount)
{
    return NGenericsComponent::GetTypeMember(name, typeParamCount);
}

optional<RDeclRes> NClassFuncDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

std::optional<RDeclRes> NClassFuncDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    if (auto o_member = NGenericsComponent::ResolveIdentifier(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    if (auto o_member = NCommonFuncDeclComponent::ResolveIdentifier(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    return _class->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);
}

} // namespace Citron