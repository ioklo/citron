#pragma once

#include "RDecl.h"
#include "RFuncDeclOuter.h"
#include "RTypeDeclOuter.h"

namespace Citron {

class ENamespaceDecl;

class RNamespaceDecl
    : public RDecl
    , public RTypeDeclOuter
    , public RFuncDeclOuter
{
public:
    virtual void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
    virtual void Accept(RFuncDeclOuterVisitor& visitor) final { visitor.Visit(this); }
    virtual void Accept(RTypeDeclOuterVisitor& visitor) final { visitor.Visit(this); }
};

class RENamespaceDecl : public RNamespaceDecl
{
    ENamespaceDecl* decl;
    // std::optional<RDeclRes> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
};

} // namespace Citron
