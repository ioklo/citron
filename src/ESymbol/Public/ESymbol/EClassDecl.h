#pragma once
#include "ESymbolConfig.h"

#include <vector>
#include <optional>
#include <variant>
#include <string>
#include "Infra/Hash.h"

#include "EClassCtorDecl.h"
#include "EClassFuncDecl.h"
#include "EClassVarDecl.h"

#include "EDecl.h"
#include "ETypeDecl.h"
#include "ETypeDeclOuter.h"
#include "ETypeDeclContainerComponent.h"
#include "EFuncDeclContainerComponent.h"
#include "ETypeDeclOuter.h"

#include "EAccessor.h"
#include "ENames.h"

namespace Citron
{

class EType;

class EClassDecl
    : public EDecl
    , public ETypeDecl
    , public ETypeDeclOuter
    , private ETypeDeclContainerComponent
    , private EFuncDeclContainerComponent<EClassFuncDecl>
{
    struct BaseTypes
    {
        EType* baseClass;
        std::vector<EType*> interfaces;
    };

    ETypeDeclOuter* outer;
    EAccessor accessor;

    EName name;
    std::vector<std::string> typeParams;

    std::vector<EClassCtorDecl*> ctors;
    int trivialCtorIndex; // can be -1

    std::vector<EClassVarDecl*> vars;

    std::optional<BaseTypes> o_baseTypes;

public:
    void Accept(EDeclVisitor& visitor) override { visitor.Visit(this); }
    void Accept(ETypeDeclVisitor& visitor) override { visitor.Visit(this); }
    void Accept(ETypeDeclOuterVisitor& visitor) override { visitor.Visit(this); }
};

}