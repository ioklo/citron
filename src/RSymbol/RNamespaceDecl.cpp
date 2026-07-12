#include "RNamespaceDecl.h"
#include <cassert>
#include "Infra/Exceptions.h"
#include "RTypeArguments.h"
#include "RFactory.h"

using namespace std;

namespace Citron {

optional<RDeclRes> RNamespaceDecl::ResolveMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    assert(typeArgs->GetCount() == 0);

    vector<RDeclRes> candidates;

    // namespace 
    if (auto o_namespace = namespaceDeclContainerComp.ResolveNamespaceMember(name, explicitTypeParamsExceptOuterCount))
        candidates.push_back(move(*o_namespace));

    // type
    if (auto o_type = typeDeclContainerComp.ResolveTypeMember(typeArgs, name, explicitTypeParamsExceptOuterCount))
        candidates.push_back(move(*o_type));

    // func
    if (auto o_func = funcDeclContainerComp.GetMemberFunc(typeArgs, name, explicitTypeParamsExceptOuterCount))
        candidates.push_back(move(*o_func));

    if (candidates.empty()) return nullopt;

    if (1 < candidates.size())
    {
        // TODO: 여러 candidate가 있다고 로깅하고 FatalException던지기
        throw NotImplementedException();
    }

    return move(candidates[0]);
}

optional<RDeclRes> RNamespaceDecl::ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    auto typeArgs = rFactory->MakeEmptyTypeArguments();
    if (auto o_member = ResolveMember(typeArgs, name, explicitTypeParamsExceptOuterCount))
        return o_member;

    if (outer)
        return outer->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);

    return nullopt;
}

RDecl* RNamespaceDecl::GetOuter()
{
    return outer;
}

RIdentifier RNamespaceDecl::GetIdentifier()
{
    return RIdentifier{name, 0, {}};
}

size_t RNamespaceDecl::GetTypeParamCount()
{
    return 0;
}

RTypeParamDecl* RNamespaceDecl::GetTypeParam(size_t index)
{
    return nullptr;
}

RTypeDecl* RNamespaceDecl::GetTypeMember(InRef<RName> name, size_t typeParamCount)
{
    return typeDeclContainerComp.GetTypeMember(name, typeParamCount);
}

} // namespace Citron
