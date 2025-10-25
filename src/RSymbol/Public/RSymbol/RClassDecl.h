#pragma once


#include "RDecl.h"
#include "RFuncDeclOuter.h"
#include "RTypeDecl.h"
#include "RTypeDeclOuter.h"
#include "RNames.h"

namespace Citron {

class EClassDecl;

class RTypeArguments;

class RClassDecl
    : public RDecl
    , public RFuncDeclOuter
    , public RTypeDecl
    , public RTypeDeclOuter
{
public:
    virtual std::optional<RMember_ClassVar> GetVar(RTypeArguments* typeArgs, const RName& name) = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
    void Accept(RFuncDeclOuterVisitor& visitor) final { visitor.Visit(this); }
    void Accept(RTypeDeclVisitor& visitor) final { visitor.Visit(this); }
    void Accept(RTypeDeclOuterVisitor& visitor) final { visitor.Visit(this); }
};

class REClassDecl : public RClassDecl
{
    EClassDecl* decl;
};


} // namespace Citron