export module Citron.MSymbol:MClassFuncDecl;

import <vector>;
import <memory>;

import :MDecl;
import :MDeclVisitor;
import :MBodyDeclOuter;

import :MFuncDecl;
import :MAccessor;
import :MNames;
import :MCommonFuncDeclComponent;

namespace Citron
{

export class MClassDecl;

export class MClassFuncDecl
    : public MDecl
    , public MBodyDeclOuter
    , public MFuncDecl
    , private MCommonFuncDeclComponent
{
    std::weak_ptr<MClassDecl> _class;
    MAccessor accessor;
    MName name;
    std::vector<std::string> typeParams;
    bool bStatic;

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MBodyDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MFuncDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}