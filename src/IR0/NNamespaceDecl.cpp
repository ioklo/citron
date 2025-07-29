#include "NNamespaceDecl.h"

#include <variant>
#include <cassert>

#include "Infra/Variants.h"
#include "Infra/Exceptions.h"
#include "Infra/Ptr.h"

#include "RTypeFactory.h"
#include "RNamespaceDeclGroup.h"
#include "RTypeArguments.h"

using namespace std;

namespace Citron {

shared_ptr<NNamespaceDecl> NNamespaceDecl::MakeRoot(RTypeFactory& factory)
{
    // root namespace면 
    auto group = factory.GetNamespaceDeclGroup({});
    shared_ptr<NNamespaceDecl> newDecl(new NNamespaceDecl(nullptr, "", group));
    group->Add(newDecl);
    return newDecl;
}

shared_ptr<NNamespaceDecl> NNamespaceDecl::MakeChild(const shared_ptr<NNamespaceDecl>& outer, const string& name, RTypeFactory& factory)
{   
    assert(outer && !name.empty());

    // root namespace면 
    std::vector<std::string> id;

    id.push_back(name);
    auto curNS = outer;

    while (curNS)
    {
        auto curOuter = curNS->outer.lock();
        
        if (!curOuter) 
        {              
            // root 라면 그만둔다
            assert(curNS->name.empty());
            break;
        }

        id.push_back(curNS->name);
        curNS = curOuter;
    }

    reverse(id.begin(), id.end());

    auto group = factory.GetNamespaceDeclGroup(id);
    shared_ptr<NNamespaceDecl> newDecl(new NNamespaceDecl(outer, name, group));
    group->Add(newDecl);

    return newDecl;
}

NNamespaceDecl::NNamespaceDecl(const shared_ptr<NNamespaceDecl>& outer, const std::string& name, const RNamespaceDeclGroupPtr& group)
    : outer(outer), name(name), group(group)
{
}

NDecl* NNamespaceDecl::GetNOuter()
{
    return outer.lock().get();
}

RDecl* NNamespaceDecl::GetROuter()
{
    return outer.lock().get();
}

RIdentifier NNamespaceDecl::GetIdentifier()
{
    return RIdentifier { RName_Normal(name), 0, {} };
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

optional<RMember> NNamespaceDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory)
{
    auto typeArgs = factory.MakeTypeArguments({});
    if (auto oMember = GetMember(typeArgs, name, explicitTypeParamsExceptOuterCount))
        return oMember;

    if (auto sharedOuter = outer.lock())
        return sharedOuter->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount, factory);

    return nullopt;
}

} // namespace Citron