#pragma once

#include <vector>
#include <optional>

#include "NDecl.h"
#include "NFuncDecl.h"
#include "NFuncDeclOuter.h"
#include "RAccessor.h"
#include "RNames.h"
#include "RFuncReturn.h"
#include "RFuncParameter.h"
#include "NTopLevelDeclOuter.h"
#include "NCommonFuncDeclComponent.h"
#include "RGlobalFuncDecl.h"

namespace Citron {

class MGlobalFuncDecl;

class NGlobalFuncDecl
    : public NDecl
    , public NFuncDecl
    , public NFuncDeclOuter
    , public RGlobalFuncDecl
    , private NCommonFuncDeclComponent
{   
    struct FuncReturnAndParams
    {
        RFuncReturn funcReturn;
        std::vector<RFuncParameter> parameters;
    };

    NTopLevelDeclOuterWPtr outer;
    RAccessor accessor;    
    RName name;
    std::vector<RName> typeParams;

    std::optional<FuncReturnAndParams> funcReturnAndParams;

public:
    // from NFuncDecl
    IR0_API NDecl* GetOuter() override;
    IR0_API RIdentifier GetIdentifier() override;

    // from NFuncDeclOuter
    IR0_API NDecl* GetDecl() override;

    // accestors
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NFuncDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NFuncDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
};

}