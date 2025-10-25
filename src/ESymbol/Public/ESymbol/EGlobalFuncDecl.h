#pragma once

#include <vector>
#include <optional>

#include "EDecl.h"
#include "EBodyDeclOuter.h"
#include "EFuncDecl.h"
#include "EFuncReturn.h"
#include "EFuncParameter.h"

#include "EAccessor.h"
#include "ENames.h"

namespace Citron {

class EGlobalFuncDecl
    : public EDecl
    , public EBodyDeclOuter
    , public EFuncDecl
{
    struct FuncReturnAndParams
    {
        EFuncReturn funcReturn;
        std::vector<EFuncParameter> parameters;
    };

    ENamespaceDecl* outer;
    EAccessor accessor;
    EName name;
    std::vector<EName> typeParams;

    std::optional<FuncReturnAndParams> funcReturnAndParams;

public:
    void Accept(EDeclVisitor& visitor) override { visitor.Visit(this); }
    void Accept(EBodyDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(EFuncDeclVisitor& visitor) override { visitor.Visit(this); }
};

}