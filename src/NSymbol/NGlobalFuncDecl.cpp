#include "NGlobalFuncDecl.h"
#include <cassert>

#include "NNamespaceDecl.h"
#include "Infra/Exceptions.h"

using namespace std;

namespace Citron {

NGlobalFuncDecl::NGlobalFuncDecl(NNamespaceDecl* outer, RAccessor accessor, bool bSeqFunc, RName&& rName)
    : outer{outer}
    , accessor{accessor}
    , name{move(rName)}
    , NCommonFuncDeclComponent{RThisKind::None, bSeqFunc}
{   
}

NDecl* NGlobalFuncDecl::GetNOuter()
{
    return outer;
}

NFuncDeclOuter* NGlobalFuncDecl::GetNFuncDeclOuter()
{
    return outer;
}

RDecl* NGlobalFuncDecl::GetROuter()
{
    return outer;
}

RIdentifier NGlobalFuncDecl::GetIdentifier()
{
    return RIdentifier{name, NGenericsComponent::GetTypeParamCount(), GetParamIds()};
}

RTypeDecl* NGlobalFuncDecl::GetTypeMember(const RName& name, size_t typeParamCount)
{
    if (auto* typeDecl = NGenericsComponent::GetTypeMember(name, typeParamCount))
        return typeDecl;
    return nullptr;
}

optional<RMember> NGlobalFuncDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    // 람다는 검색시키지 않는다
    // 현재 함수에서 Declaration을 할 수 없기 때문에 
    return nullopt;
}

optional<RMember> NGlobalFuncDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    if (auto o_member = NGenericsComponent::ResolveIdentifier(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    if (auto o_member = NCommonFuncDeclComponent::ResolveIdentifier(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    return outer->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);
}

}