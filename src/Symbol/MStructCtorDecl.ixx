export module Citron.MSymbol:MStructCtorDecl;

import <memory>;
import <vector>;

import :MDecl;
import :MDeclVisitor;
import :MBodyDeclOuter;
import :MFuncDecl;
import :MAccessor;
import :MCommonFuncDeclComponent;

namespace Citron
{

export class MStructDecl;
export class MFuncParameter;

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