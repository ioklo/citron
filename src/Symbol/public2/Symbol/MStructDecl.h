#pragma once
#include "SymbolConfig.h"

#include <memory>
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
using MTypePtr = std::shared_ptr<MType>;

class MStructDecl
    : public MDecl
    , public MTypeDecl
    , public MTypeDeclOuter
    , private MTypeDeclContainerComponent
    , private MFuncDeclContainerComponent<std::shared_ptr<MStructFuncDecl>>
{
    struct BaseTypes
    {
        MTypePtr baseStruct;
        std::vector<MTypePtr> interfaces;
    };

    MTypeDeclOuterWPtr outer;
    MAccessor accessor;

    MName name;
    std::vector<std::string> typeParams;

    std::vector<std::shared_ptr<MStructCtorDecl>> ctors;
    int trivialCtorIndex; // can be -1

    std::vector<std::shared_ptr<MStructVarDecl>> vars;

    std::optional<BaseTypes> oBaseTypes;

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTypeDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
};

}