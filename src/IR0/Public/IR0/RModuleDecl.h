#pragma once

#include <memory>

#include "RDecl.h"
#include "RTopLevelDeclOuter.h"
#include "RFuncDeclOuter.h"
#include "RTypeDeclOuter.h"

namespace Citron {

class MModuleDecl;

class RModuleDecl 
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

class RMModuleDecl : public RModuleDecl
{
    std::shared_ptr<MModuleDecl> decl;
    // std::optional<RMember> GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
};


} // namespace Citron