#pragma once
#include "SymbolConfig.h"

#include <vector>
#include <optional>
#include <string>

#include "MStructCtorDecl.h"
#include "MStructFuncDecl.h"
#include "MStructVarDecl.h"

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

class MStructDecl
    : public MDecl
    , public MTypeDecl
    , public MTypeDeclOuter
    , private MTypeDeclContainerComponent
    , private MFuncDeclContainerComponent<MStructFuncDecl>
{
    struct BaseTypes
    {
        MType* baseStruct;
        std::vector<MType*> interfaces;
    };

    MTypeDeclOuter* outer;
    MAccessor accessor;

    MName name;
    std::vector<std::string> typeParams;

    std::vector<MStructCtorDecl*> ctors;
    int trivialCtorIndex; // can be -1

    std::vector<MStructVarDecl*> vars;

    std::optional<BaseTypes> oBaseTypes;

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(this); }
    void Accept(MTypeDeclVisitor& visitor) override { visitor.Visit(this); }
    void Accept(MTypeDeclOuterVisitor& visitor) override { visitor.Visit(this); }
};

}