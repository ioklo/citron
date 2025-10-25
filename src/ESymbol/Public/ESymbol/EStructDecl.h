#pragma once
#include "ESymbolConfig.h"

#include <vector>
#include <optional>
#include <string>

#include "EStructCtorDecl.h"
#include "EStructFuncDecl.h"
#include "EStructVarDecl.h"

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

class EStructDecl
    : public EDecl
    , public ETypeDecl
    , public ETypeDeclOuter
    , private ETypeDeclContainerComponent
    , private EFuncDeclContainerComponent<EStructFuncDecl>
{
    struct BaseTypes
    {
        EType* baseStruct;
        std::vector<EType*> interfaces;
    };

    ETypeDeclOuter* outer;
    EAccessor accessor;

    EName name;
    std::vector<std::string> typeParams;

    std::vector<EStructCtorDecl*> ctors;
    int trivialCtorIndex; // can be -1

    std::vector<EStructVarDecl*> vars;

    std::optional<BaseTypes> oBaseTypes;

public:
    void Accept(EDeclVisitor& visitor) override { visitor.Visit(this); }
    void Accept(ETypeDeclVisitor& visitor) override { visitor.Visit(this); }
    void Accept(ETypeDeclOuterVisitor& visitor) override { visitor.Visit(this); }
};

}