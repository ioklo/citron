#include "NNamespaceDecl.h"

#include <variant>
#include <cassert>
#include <algorithm>

#include "Infra/Variants.h"
#include "Infra/Exceptions.h"
#include "Infra/Ptr.h"

#include "RSymbol/RFactory.h"
#include "RSymbol/RNamespaceDeclGroup.h"
#include "RSymbol/RTypeArguments.h"

using namespace std;

namespace Citron {

NNamespaceDecl::NNamespaceDecl(NNamespaceDecl* outer, const std::string& name, RNamespaceDeclGroup* group, const RFactoryPtr& rFactory)
    : outer{outer}, name{name}, group{group}, rFactory{rFactory}
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

RTypeDecl* NNamespaceDecl::GetTypeMember(const RName& name, size_t typeParamCount)
{
    return NTypeDeclContainerComponent::GetTypeMember(name, typeParamCount);
}

// NotFound, Valid는 리턴으로, Fatal은 exception으로
// Fatal을 처리해서 복구하고 싶으면 catch로
optional<RDeclRes> NNamespaceDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    
}

optional<RDeclRes> NNamespaceDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    auto typeArgs = rFactory->MakeEmptyTypeArguments();
    if (auto o_member = GetMember(typeArgs, name, explicitTypeParamsExceptOuterCount))
        return o_member;

    if (outer)
        return outer->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);

    return nullopt;
}

} // namespace Citron