#include "RNamespaceDecl.h"
#include <cassert>
#include "Infra/Exceptions.h"
#include "RTypeArguments.h"
#include "RFactory.h"
#include "RMember.h"

using namespace std;

namespace Citron {

RDecl* RNamespaceDecl::GetOuter()
{
    return outer.Visit([](auto* outer) -> RDecl* {

        using T = remove_cvref_t<decltype(outer)>;

        if constexpr (same_as<T, RModule*>)
        {
            return nullptr;
        }
        else if constexpr (same_as<T, RNamespaceDecl*>)
        {
            return outer;
        }
        else static_assert(false);
    });
}

RIdentifier RNamespaceDecl::GetIdentifier()
{
    return RIdentifier{EncodeRName(name)};
}

size_t RNamespaceDecl::GetTypeParamCount()
{
    return 0;
}

RTypeParam* RNamespaceDecl::GetTypeParam(size_t index)
{
    return nullptr;
}

RTypeParam* RNamespaceDecl::GetTypeParam(InRef<RName> name)
{
    return nullptr;
}

RTypeDecl* RNamespaceDecl::GetTypeMember(InRef<RName> name)
{
    return typeDeclContainerComp.GetTypeMember(name);
}

optional<RMember> RNamespaceDecl::GetMember(InRef<RName> name)
{
    if (auto* namespaceDecl = namespaceDeclContainerComp.GetNamespace(name))
        return RMember_Namespace{namespaceDecl};

    if (auto* typeDecl = typeDeclContainerComp.GetTypeMember(name))
        return ToRMember(typeDecl);

    if (auto o_funcMember = funcDeclContainerComp.GetFuncs(name))
        return o_funcMember;

    return nullopt;
}

} // namespace Citron
