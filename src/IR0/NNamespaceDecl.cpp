#include "NNamespaceDecl.h"

#include <variant>
#include <cassert>
#include <algorithm>

#include "Infra/Variants.h"
#include "Infra/Exceptions.h"
#include "Infra/Ptr.h"

#include "IR0Factory.h"
#include "RNamespaceDeclGroup.h"
#include "RTypeArguments.h"

using namespace std;

namespace Citron {

NNamespaceDecl::NNamespaceDecl(NNamespaceDecl* outer, const std::string& name, RNamespaceDeclGroup* group)
    : outer(outer), name(name), group(group)
{
}

NDecl* NNamespaceDecl::GetNOuter()
{
    return outer;
}

RDecl* NNamespaceDecl::GetROuter()
{
    return outer;
}

RIdentifier NNamespaceDecl::GetIdentifier()
{
    return RIdentifier { RName_Normal(name), 0, {} };
}

// NotFound, Valid는 리턴으로, Fatal은 exception으로
// Fatal을 처리해서 복구하고 싶으면 catch로
optional<RMember> NNamespaceDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    assert(typeArgs->GetCount() == 0);

    vector<RMember> candidates;

    // namespace 
    if (auto oNamespace = NNamespaceDeclContainerComponent::GetMemberNamespace(name, explicitTypeParamsExceptOuterCount))
        candidates.push_back(*oNamespace);

    // type
    if (auto oType = NTypeDeclContainerComponent::GetMemberType(typeArgs, name, explicitTypeParamsExceptOuterCount))
        candidates.push_back(*oType);

    // func
    if (auto oFunc = NFuncDeclContainerComponent<NGlobalFuncDecl>::GetMemberFunc(typeArgs, name, explicitTypeParamsExceptOuterCount))
        candidates.push_back(*oFunc);

    if (candidates.empty()) return nullopt;

    if (1 < candidates.size())
    {
        // TODO: 여러 candidate가 있다고 로깅하고 FatalException던지기
        throw NotImplementedException();
    }

    return candidates[1];
}

optional<RMember> NNamespaceDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, IR0Factory& factory)
{
    auto typeArgs = factory.MakeTypeArguments({});
    if (auto oMember = GetMember(typeArgs, name, explicitTypeParamsExceptOuterCount))
        return oMember;
    
    return outer->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount, factory);
}

} // namespace Citron