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
#include "RCommonFuncDeclComponent.h"

namespace Citron {

class MGlobalFuncDecl;

// abstract
class RGlobalFuncDecl
    : public RDecl
    , public RBodyDeclOuter
    , public RFuncDecl
{
public:
    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RBodyDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RFuncDeclVisitor& visitor) override { visitor.Visit(*this); }

    virtual RTypePtr GetReturnType(RTypeArguments& typeArgs, RTypeFactory& factory) = 0;
};

class RExternalGlobalFuncDecl : public RGlobalFuncDecl
{
    std::shared_ptr<MGlobalFuncDecl> externalFuncDecl;
};

class RInternalGlobalFuncDecl 
    : public RGlobalFuncDecl
    , private RCommonFuncDeclComponent
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
};

}