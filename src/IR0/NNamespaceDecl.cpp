#include "NNamespaceDecl.h"

#include <variant>
#include <cassert>

#include <Infra/Variants.h>
#include <Infra/Exceptions.h>

#include "RTypeArguments.h"
#include "DeclWithOuterTypeArgs.h"
#include "RGlobalFuncDecl.h"

using namespace std;

namespace Citron {

NNamespaceDecl::NNamespaceDecl(NTopLevelDeclOuterWPtr outer, std::string name)
    : outer(outer), name(name)
{
}

RIdentifier NNamespaceDecl::GetIdentifier()
{
    return RIdentifier { RName_Normal(name), 0, {} };
}

RDecl* NNamespaceDecl::GetROuter()
{
    return outer.lock()->GetNDecl()->GetRDecl();
}

// NotFound, Valid는 리턴으로, Fatal은 exception으로
// Fatal을 처리해서 복구하고 싶으면 catch로
optional<RMember> NNamespaceDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
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

} // namespace Citron