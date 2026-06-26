#pragma once
#include "RSymbolConfig.h"

#include "RDecl.h"
#include "RTypeDecl.h"
#include "RTypeDeclOuter.h"
#include "RNames.h"

namespace Citron {

class EClassDecl;

class RTypeArguments;

class RClassDecl
    : public RDecl
    , public RTypeDecl
    , public RTypeDeclOuter
{
public:
    virtual std::optional<RDeclRes_ClassVar> GetVar(RTypeArguments* typeArgs, const RName& name) = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
    RSYMBOL_API void Accept(RTypeDeclVisitor& visitor) final;
    void Accept(RTypeDeclOuterVisitor& visitor) final { visitor.Visit(this); }
};

class REClassDecl : public RClassDecl
{
    EClassDecl* decl;
};

} // namespace Citron