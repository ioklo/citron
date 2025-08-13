#include "NClassCtorDecl.h"

#include <cassert>
#include "NClassDecl.h"

using namespace std;

namespace Citron {

NClassCtorDecl::NClassCtorDecl(NClassDecl* _class, RAccessor accessor, bool bTrivial, vector<string>&& typeParams, vector<RFuncParameter> parameters, bool bLastParamVariadic)
    : NCommonFuncDeclComponent(/*bStatic*/ false, /*bSeqFunc*/ false, move(typeParams))
    , _class(_class), accessor(accessor), bTrivial(bTrivial)
{
    NCommonFuncDeclComponent::InitFuncReturnAndParams(RFuncReturn_ForCtor(), move(parameters), bLastParamVariadic);
}

NDecl* NClassCtorDecl::GetNOuter()
{
    return _class;
}

RDecl* NClassCtorDecl::GetROuter()
{
    return _class;
}

RIdentifier NClassCtorDecl::GetIdentifier()
{
    return RIdentifier { RName_Reserved("Ctor"), 0, NCommonFuncDeclComponent::GetParamIds() };
}

optional<RMember> NClassCtorDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

std::optional<RMember> NClassCtorDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, IR0Factory& factory)
{
    auto baseTypeParamCount = _class->GetAllTypeParamCount();
    if (auto oMember = NCommonFuncDeclComponent::ResolveIdentifier(baseTypeParamCount, name, explicitTypeParamsExceptOuterCount, factory))
        return oMember;


    return _class->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount, factory);
}

RClassDecl* NClassCtorDecl::GetClassDecl()
{
    return _class;
}

} // namespace Citron