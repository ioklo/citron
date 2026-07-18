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
    return outer;
}

RIdentifier RNamespaceDecl::GetIdentifier()
{
    return RIdentifier{name, {}};
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
