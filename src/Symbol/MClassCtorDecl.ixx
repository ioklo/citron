export module Citron.MSymbol:MClassCtorDecl;

import <vector>;
import <memory>;
import :ForwardDecls;

import :MDecl;
import :MBodyDeclOuter;
import :MFuncDecl;
import :MCommonFuncDeclComponent;

import Citron.MAccessor;

namespace Citron
{

export class MClassCtorDecl
    : public MDecl
    , public MBodyDeclOuter
    , public MFuncDecl
    , private MCommonFuncDeclComponent
{
    std::weak_ptr<MClassDecl> _class;
    MAccessor accessor;
    std::vector<MFuncParameter> parameters;
    bool bTrivial;

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MBodyDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MFuncDeclVisitor& visitor) override { visitor.Visit(*this); }
};


}