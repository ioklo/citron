#pragma once

#include <vector>
#include <optional>

#include "RDecl.h"
#include "RBodyDeclOuter.h"
#include "RFuncDecl.h"
#include "RAccessor.h"
#include "RNames.h"
#include "RFuncReturn.h"
#include "RFuncParameter.h"
#include "RTopLevelDeclOuter.h"

namespace Citron {

class RGlobalFuncDecl
    : public RDecl
    , public RBodyDeclOuter
    , public RFuncDecl
{   
    struct FuncReturnAndParams
    {
        RFuncReturn funcReturn;
        std::vector<RFuncParameter> parameters;
    };

    RTopLevelDeclOuterPtr outer;
    RAccessor accessor;    
    RName name;
    std::vector<RName> typeParams;

    std::optional<FuncReturnAndParams> funcReturnAndParams;

public:
    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RBodyDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RFuncDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}