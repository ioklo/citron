#pragma once

#include <memory>
#include "RDecl.h"
#include "RTopLevelDeclOuter.h"
#include "RFuncDeclOuter.h"
#include "RTypeDeclOuter.h"

namespace Citron {

class MNamespaceDecl;

class RNamespaceDecl 
    : public RDecl
    , public RTopLevelDeclOuter
    , public RTypeDeclOuter
    , public RFuncDeclOuter
{
public:
    virtual void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
    virtual void Accept(RTopLevelDeclOuterVisitor& visitor) final { visitor.Visit(*this); }
    virtual void Accept(RFuncDeclOuterVisitor& visitor) final { visitor.Visit(*this); }
    virtual void Accept(RTypeDeclOuterVisitor& visitor) final { visitor.Visit(*this); }
};

class RMNamespaceDecl : public RNamespaceDecl
{
    std::shared_ptr<MNamespaceDecl> decl;
    // std::optional<RMember> GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
};


} // namespace Citron
