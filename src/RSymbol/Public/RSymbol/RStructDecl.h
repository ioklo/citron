#pragma once
#include "RSymbolConfig.h"

#include "Infra/Views.h"

#include "RDecl.h"
#include "RTypeDecl.h"
#include "RTypeDeclOuter.h"

namespace Citron {

class EStructDecl;
class RType_Struct;
class RTypeArguments;

class RStructDecl
    : public RDecl
    , public RTypeDecl
    , public RTypeDeclOuter
{
public:
    virtual RType_Struct* GetUnboundBaseStruct() = 0;
    virtual View<RStructVarDecl*> GetRVars() = 0;
    virtual std::optional<RDeclRes_StructVar> GetVar(RTypeArguments* typeArgs, const RName& name) = 0;
    virtual std::vector<RStructCtorDecl*> GetUnboundCtors() = 0;
    virtual RStructCtorDecl* GetUnboundCopyCtor() = 0;
    virtual RStructCtorDecl* GetUnboundTrivialCtor_RStructCtorDecl() = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
    RSYMBOL_API void Accept(RTypeDeclVisitor& visitor) final;
    void Accept(RTypeDeclOuterVisitor& visitor) final { visitor.Visit(this); }
};

class REStructDecl : public RStructDecl
{
    EStructDecl* decl;
};


} // namespace Citron

