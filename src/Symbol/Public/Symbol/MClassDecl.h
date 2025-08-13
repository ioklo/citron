#pragma once
#include "SymbolConfig.h"

#include <vector>
#include <optional>
#include <variant>
#include <string>
#include "Infra/Hash.h"

#include "MClassCtorDecl.h"
#include "MClassFuncDecl.h"
#include "MClassVarDecl.h"

#include "MDecl.h"
#include "MTypeDecl.h"
#include "MTypeDeclOuter.h"
#include "MTypeDeclContainerComponent.h"
#include "MFuncDeclContainerComponent.h"
#include "MTypeDeclOuter.h"

#include "MAccessor.h"
#include "MNames.h"

namespace Citron
{

class MType;

class MClassDecl
    : public MDecl
    , public MTypeDecl
    , public MTypeDeclOuter
    , private MTypeDeclContainerComponent
    , private MFuncDeclContainerComponent<MClassFuncDecl>
{
    struct BaseTypes
    {
        MType* baseClass;
        std::vector<MType*> interfaces;
    };

    MTypeDeclOuter* outer;
    MAccessor accessor;

    MName name;
    std::vector<std::string> typeParams;

    std::vector<MClassCtorDecl*> ctors;
    int trivialCtorIndex; // can be -1

    std::vector<MClassVarDecl*> vars;

    std::optional<BaseTypes> oBaseTypes;

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(this); }
    void Accept(MTypeDeclVisitor& visitor) override { visitor.Visit(this); }
    void Accept(MTypeDeclOuterVisitor& visitor) override { visitor.Visit(this); }
};

}