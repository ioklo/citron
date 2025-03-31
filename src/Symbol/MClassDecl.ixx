export module Citron.MDecls:MClassDecl;

import :MClassCtorDecl;
import :MClassFuncDecl;
import :MClassVarDecl;

import :MDecl;
import :MTypeDecl;
import :MTypeDeclOuter;
import :MTypeDeclContainerComponent;
import :MFuncDeclContainerComponent;
import :MTypeDeclOuter;

import Citron.MAccessor;
import Citron.MNames;

namespace Citron
{

export class MType;
export using MTypePtr = std::shared_ptr<MType>;

export class MClassDecl
    : public MDecl
    , public MTypeDecl
    , public MTypeDeclOuter
    , private MTypeDeclContainerComponent
    , private MFuncDeclContainerComponent<std::shared_ptr<MClassFuncDecl>>
{
    struct BaseTypes
    {
        MTypePtr baseClass;
        std::vector<MTypePtr> interfaces;
    };

    MTypeDeclOuterWPtr outer;
    MAccessor accessor;

    MName name;
    std::vector<std::string> typeParams;

    std::vector<std::shared_ptr<MClassCtorDecl>> ctors;
    int trivialCtorIndex; // can be -1

    std::vector<std::shared_ptr<MClassVarDecl>> vars;

    std::optional<BaseTypes> oBaseTypes;

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTypeDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
};

}