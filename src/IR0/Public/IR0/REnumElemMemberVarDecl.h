#pragma once
#include "IR0Config.h"

#include <memory>
#include <optional>

#include "RDecl.h"
#include "RNames.h"
#include "RType.h"

namespace Citron
{

class REnumElemDecl;

class REnumElemMemberVarDecl
    : public RDecl
{   
    std::weak_ptr<REnumElemDecl> outer;
    RName name;

    RTypePtr declType; // lazy-init

public:
    IR0_API REnumElemMemberVarDecl(std::weak_ptr<REnumElemDecl> outer, RName name);
    IR0_API void InitDeclType(RTypePtr&& declType);

    IR0_API RTypePtr GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory);
    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this); }
};


}