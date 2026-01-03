#include "NClassCtorDecl.h"

#include <cassert>
#include "NClassDecl.h"

using namespace std;

namespace Citron {

NClassCtorDecl::NClassCtorDecl(NClassDecl* _class, RAccessor accessor, bool bTrivial)
    : _class{_class}
    , accessor{accessor}
    , bTrivial{bTrivial}
    , NCommonFuncDeclComponent(/*bStatic*/false, /*bSeqFunc*/false)
{   
    NGenericsComponent::InitTypeParams({});
}

void NClassCtorDecl::Init(vector<RFuncParameter>&& parameters, bool bLastParamVariadic)
{   
    NCommonFuncDeclComponent::InitFuncReturnAndParams(RFuncReturn_ForCtor{}, move(parameters), bLastParamVariadic);
}


NDecl* NClassCtorDecl::GetNOuter()
{
    return _class;
}

NFuncDeclOuter* NClassCtorDecl::GetNFuncDeclOuter()
{
    return _class;
}

RDecl* NClassCtorDecl::GetROuter()
{
    return _class;
}

RIdentifier NClassCtorDecl::GetIdentifier()
{
    return RIdentifier{RName_Reserved("Ctor"), 0, NCommonFuncDeclComponent::GetParamIds()};
}

RTypeDecl* NClassCtorDecl::GetTypeMember(const RName& name, size_t typeParamCount)
{
    return NGenericsComponent::GetTypeMember(name, typeParamCount);
}

optional<RMember> NClassCtorDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

optional<RMember> NClassCtorDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{   
    if (auto o_member = NGenericsComponent::ResolveIdentifier(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    if (auto o_member = NCommonFuncDeclComponent::ResolveIdentifier(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    return _class->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);
}

RClassDecl* NClassCtorDecl::GetClassDecl()
{
    return _class;
}

} // namespace Citron