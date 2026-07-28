#include "RNamespace.h"
#include <cassert>
#include "Infra/Exceptions.h"
#include "RTypeArguments.h"
#include "RFactory.h"
#include "RMember.h"
#include "RModule.h"

using namespace std;

namespace Citron {

RDeclKey& RNamespace::GetDeclKey()
{
    return key;
}

RDecl* RNamespace::GetOuter()
{
    return kind.Visit([](auto& kind) -> RDecl* {
        using T = remove_cvref_t<decltype(kind)>;

        if constexpr (same_as<T, RNamespaceKind_Root>)
        {
            return nullptr;
        }
        else if constexpr (same_as<T, RNamespaceKind_Normal>)
        {
            return kind.outer;
        }
        else static_assert(false);
    });
}

RName* RNamespace::TryGetName()
{
    return kind.Visit([](auto& kind) -> RName* {
        using T = remove_cvref_t<decltype(kind)>;
        if constexpr (same_as<T, RNamespaceKind_Root>)
        {
            return nullptr;
        }
        else if constexpr (same_as<T, RNamespaceKind_Normal>)
        {
            return &kind.name;
        }
        else static_assert(false);
    });
}

size_t RNamespace::GetTypeParamCount()
{
    return 0;
}

RTypeParam* RNamespace::GetTypeParam(size_t index)
{
    return nullptr;
}

RTypeParam* RNamespace::GetTypeParam(InRef<RName> name)
{
    return nullptr;
}

RTypeDecl* RNamespace::GetTypeMember(InRef<RName> name)
{
    return typeDeclContainerComp.GetTypeMember(name);
}

optional<RMember> RNamespace::GetMember(InRef<RName> name)
{
    if (auto* _namespace = namespaceDeclContainerComp.GetNamespace(name))
        return RMember_Namespace{_namespace};

    if (auto* typeDecl = typeDeclContainerComp.GetTypeMember(name))
        return ToRMember(typeDecl);

    if (auto o_funcMember = funcDeclContainerComp.GetFuncs(name))
        return o_funcMember;

    return nullopt;
}

bool RNamespace::FillIdentifier(std::string& buffer)
{
    // RModule이나 RNamespace이나 FillIdentifier가 있으므로 구분하지 않고 호출한다
    return kind.Visit([this, &buffer](auto& kind) -> bool {

        using T = remove_cvref_t<decltype(kind)>;

        if constexpr (same_as<T, RNamespaceKind_Root>)
        {
            kind._module->FillIdentifier(buffer);
            return true;
        }
        else if constexpr (same_as<T, RNamespaceKind_Normal>)
        {
            if (kind.outer->FillIdentifier(buffer))
                buffer.append("::");
            else
                buffer.append(".");
                
            buffer.append(key.GetValue());
            return false;
        }
    });
}

} // namespace Citron
