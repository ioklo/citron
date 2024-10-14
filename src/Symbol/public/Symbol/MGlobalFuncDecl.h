#pragma once

#include <vector>
#include <optional>

#include "MDecl.h"
#include "MBodyDeclOuter.h"
#include "MFuncDecl.h"
#include "MAccessor.h"
#include "MNames.h"
#include "MFuncReturn.h"
#include "MFuncParameter.h"
#include "MTopLevelDeclOuter.h"

namespace Citron {

class MGlobalFuncDecl
    : public MDecl
    , public MBodyDeclOuter
    , public MFuncDecl
{   
    struct FuncReturnAndParams
    {
        MFuncReturn funcReturn;
        std::vector<MFuncParameter> parameters;
    };

    MTopLevelDeclOuterPtr outer;
    MAccessor accessor;
    MName name;
    std::vector<MName> typeParams;

    std::optional<FuncReturnAndParams> funcReturnAndParams;

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MBodyDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MFuncDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}