#pragma once
#include <memory>
#include "SmDeclContext.h"

namespace Citron {

class RClassDecl;


class SmDeclContext_ClassDecl : public SmDeclContext
{
    std::shared_ptr<SmDeclContext> outer;
    RClassDecl* decl;
    RTypeArguments* typeArgs;

public:
    RClassDecl* GetRClassDecl() { return decl; }

protected: // from SmDeclContext
    SmDeclContext* GetOuter() override;
    RDecl* GetDecl() override;

    std::optional<SmTypeRes> ResolveInheritedTypeMember(InRef<RName> name) override;
    std::optional<SmDeclRes> ResolveInheritedMember(InRef<RName> name) override;
};

} // namespace Citron