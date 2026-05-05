#include "NClassDecl.h"

#include <cassert>
#include <ranges>

#include "Infra/Exceptions.h"
#include "RSymbol/RFactory.h"
#include "NClassFuncDecl.h"
#include "NTypeParamDecl.h"

using namespace std;

namespace Citron {

NDecl* NClassDecl::GetNOuter()
{
    return outer->GetNDecl();
}

RDecl* NClassDecl::GetROuter()
{
    return outer->GetNDecl()->GetRDecl();
}

RIdentifier NClassDecl::GetIdentifier()
{
    return RIdentifier{name, NGenericsComponent::GetTypeParamCount(), {}};
}

RTypeDecl* NClassDecl::GetTypeMember(const RName& name, size_t typeParamCount)
{
    if (auto* typeDecl = NGenericsComponent::GetTypeMember(name, typeParamCount))
        return typeDecl;
    
    return NTypeDeclContainerComponent::GetTypeMember(name, typeParamCount);
}

RDeclRes NClassDecl::ToRDeclRes(RTypeArguments* typeArgs)
{   
    return RDeclRes_Class(typeArgs, this);
}

optional<RDeclRes> NClassDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    vector<RDeclRes> candidates;

    // type
    if (auto o_type = NTypeDeclContainerComponent::GetMemberType(typeArgs, name, explicitTypeParamsExceptOuterCount))
        candidates.push_back(move(*o_type));

    // class member func
    if (auto o_func = NFuncDeclContainerComponent<NClassFuncDecl>::GetMemberFunc(typeArgs, name, explicitTypeParamsExceptOuterCount))
        candidates.push_back(move(*o_func));

    if (explicitTypeParamsExceptOuterCount == 0)
        if (auto o_var = GetVar(typeArgs, name))
            candidates.push_back(move(*o_var));

    if (candidates.empty()) return nullopt;

    if (1 < candidates.size())
    {
        // TODO: 여러 candidate가 있다고 로깅하고 FatalException던지기
        throw NotImplementedException();
    }

    return move(candidates[1]);
}

optional<RDeclRes> NClassDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    if (auto o_member = NGenericsComponent::ResolveIdentifier(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    auto* typeArgs = MakeOpenTypeArgs(*rFactory);
    if (auto o_member = GetMember(typeArgs, name, explicitTypeParamsExceptOuterCount))
        return o_member;

    // TODO: [37] class base에서도 검색하기

    return outer->GetNDecl()->GetRDecl()->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);
}

RType* NClassDecl::GetOpenType()
{
    return rFactory->MakeClassType(this, MakeOpenTypeArgs(*rFactory));
}

optional<RDeclRes_ClassVar> NClassDecl::GetVar(RTypeArguments* typeArgs, const RName& name)
{
    auto i = varsMap.find(name);
    if (i == varsMap.end()) return nullopt;

    return RDeclRes_ClassVar(i->second, typeArgs);
}

} // namespace Citron