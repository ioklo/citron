#pragma once
#include <concepts>
#include "Infra/Ref.h"
#include "SmDeclContext.h"

namespace Citron {

class RDecl;
class RClassDecl;
class RTypeArguments;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

template<typename TRDecl> 
    requires std::derived_from<TRDecl, RDecl> && (!std::same_as<TRDecl, RClassDecl>)
class SmDeclContext_Decl : public SmDeclContext
{
    SmDeclContextPtr outer;
    TRDecl* decl;
    RTypeArguments* typeArgs;

public:
    SmDeclContext_Decl(TakeRef<SmDeclContextPtr> outer, TRDecl* decl, RTypeArguments* typeArgs)
        : outer{outer.Take()}, decl{decl}, typeArgs{typeArgs}
    {
    }

    TRDecl* GetRDecl() { return decl; }

protected:
    SmDeclContext* GetOuter() override { return outer.get(); }
    RDecl* GetDecl() override { return decl; }
    RTypeArguments* GetTypeArgs() override { return typeArgs; }
};

} // namespace Citron
