#pragma once
#include <concepts>
#include "SmDeclContext.h"

namespace Citron {

class RDecl;
class RClassDecl;
class RTypeArguments;

template<typename TRDecl> 
    requires std::derived_from<TRDecl, RDecl> && (!std::same_as<TRDecl, RClassDecl>)
class SmDeclContext_Decl : public SmDeclContext
{
    std::shared_ptr<SmDeclContext> outer;
    TRDecl* decl;
    RTypeArguments* typeArgs;

protected:
    RDecl* GetDecl() override { return decl; }

};

} // namespace Citron
