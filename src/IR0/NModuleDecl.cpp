#include "NModuleDecl.h"

#include <cassert>

#include <Infra/Exceptions.h>
#include "RTypeArguments.h"
#include "DeclWithOuterTypeArgs.h"
#include "RGlobalFuncDecl.h"

using namespace std;

namespace Citron {

NModuleDecl::NModuleDecl(string name)
    : name(std::move(name))
{
}

RDecl* NModuleDecl::GetROuter()
{
    return nullptr;
}

RIdentifier NModuleDecl::GetIdentifier()
{
    return RIdentifier { RName_Normal(name), 0, {} };
}

optional<RMember> NModuleDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
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

}