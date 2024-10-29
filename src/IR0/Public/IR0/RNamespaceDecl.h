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
    , public RFuncDeclOuter
    , public RTypeDeclOuter
{
public:
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RTopLevelDeclOuterVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RFuncDeclOuterVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RTypeDeclOuterVisitor& visitor) final { visitor.Visit(*this); }
};

class RMNamespaceDecl : public RNamespaceDecl
{
    std::shared_ptr<MNamespaceDecl> decl;
};


} // namespace Citron
