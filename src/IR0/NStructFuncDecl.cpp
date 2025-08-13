#include "NStructFuncDecl.h"
#include <cassert>
#include "NStructDecl.h"

using namespace std;

namespace Citron
{

NStructFuncDecl::NStructFuncDecl(NStructDecl* _struct, RAccessor accessor, std::string name, std::vector<std::string>&& typeParams, bool bStatic)
    : NCommonFuncDeclComponent(/*bStatic*/ false, /*bSeqFunc*/ false, move(typeParams))
    , _struct(_struct)
    , accessor(accessor)
    , name(move(name))
    , bStatic(bStatic)
{
}

void NStructFuncDecl::InitFuncReturnAndParams(RType* funcReturn, std::vector<RFuncParameter> funcParameters, bool bLastParameterVariadic)
{
    NCommonFuncDeclComponent::InitFuncReturnAndParams(RFuncReturn_Set(funcReturn), move(funcParameters), bLastParameterVariadic);
}

NDecl* NStructFuncDecl::GetNOuter()
{
    return _struct;
}

RDecl* NStructFuncDecl::GetROuter()
{
    return _struct;
}

RIdentifier NStructFuncDecl::GetIdentifier()
{
    return RIdentifier { RName_Normal(name), NCommonFuncDeclComponent::GetTypeParamCount(), NCommonFuncDeclComponent::GetParamIds() };
}

optional<RMember> NStructFuncDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

optional<RMember> NStructFuncDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, IR0Factory& factory)
{
    size_t baseTypeParamCount = _struct->GetAllTypeParamCount();
    if (auto oMember = NCommonFuncDeclComponent::ResolveIdentifier(baseTypeParamCount, name, explicitTypeParamsExceptOuterCount, factory))
        return oMember;

    return _struct->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount, factory);
}

}