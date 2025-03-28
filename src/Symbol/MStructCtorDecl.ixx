export module Citron.MSymbol:MStructCtorDecl;

import <memory>;
import <vector>;

import :ForwardDecls;
import :MDecl;
import :MBodyDeclOuter;
import :MFuncDecl;
import :MCommonFuncDeclComponent;

import Citron.MAccessor;

namespace Citron
{

export class MStructCtorDecl
    : public MDecl
    , public MBodyDeclOuter
    , public MFuncDecl
    , private MCommonFuncDeclComponent
{
    std::weak_ptr<MStructDecl> _struct;
    MAccessor accessor;
    std::vector<MFuncParameter> parameters;
    bool bTrivial;

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MBodyDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MFuncDeclVisitor& visitor) override { visitor.Visit(*this); }
};


}