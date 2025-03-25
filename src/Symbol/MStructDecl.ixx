export module Citron.MSymbol:MStructDecl;

import :MStructCtorDecl;
import :MStructFuncDecl;
import :MStructVarDecl;

import :MTypeDecl;
import :MTypeDeclOuter;
import :MNames;
import :MTypeDeclContainerComponent;
import :MFuncDeclContainerComponent;
import :MTypeDeclOuter;
import :MAccessor;

namespace Citron
{

export class MStructDecl
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