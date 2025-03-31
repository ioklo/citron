module Citron.NDecls:NStructFuncDecl;
import <cassert>;
import :NStructDecl;

using namespace std;

namespace Citron
{

NStructFuncDecl::NStructFuncDecl(std::weak_ptr<NStructDecl> _struct, RAccessor accessor, std::string name, std::vector<std::string>&& typeParams, bool bStatic)
    : NCommonFuncDeclComponent(/*bStatic*/ false, /*bSeqFunc*/ false, std::move(typeParams))
    , _struct(std::move(_struct))
    , accessor(accessor)
    , name(std::move(name))
    , typeParams(std::move(typeParams))
    , bStatic(bStatic)
{
}

void NStructFuncDecl::InitFuncReturnAndParams(RTypePtr funcReturn, std::vector<RFuncParameter> funcParameters, bool bLastParameterVariadic)
{
    NCommonFuncDeclComponent::InitFuncReturnAndParams(RFuncReturn_Set(std::move(funcReturn)), std::move(funcParameters), bLastParameterVariadic);
}

NDecl* NStructFuncDecl::GetNOuter()
{
    return _struct.lock().get();
}

RDecl* NStructFuncDecl::GetROuter()
{
    return _struct.lock().get();
}

RIdentifier NStructFuncDecl::GetIdentifier()
{
    return RIdentifier { RName_Normal(name), typeParams.size(), NCommonFuncDeclComponent::GetParamIds() };
}

optional<RMember> NStructFuncDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

optional<RMember> NStructFuncDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory)
{
    auto sharedStruct = _struct.lock();
    assert(sharedStruct);

    size_t baseTypeParamCount = sharedStruct->GetAllTypeParamCount();
    if (auto oMember = NCommonFuncDeclComponent::ResolveIdentifier(baseTypeParamCount, name, explicitTypeParamsExceptOuterCount, factory))
        return oMember;

    return sharedStruct->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount, factory);
}

}