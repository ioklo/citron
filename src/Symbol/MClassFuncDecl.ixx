export module Citron.MDecls:MClassFuncDecl;

import <vector>;
import <memory>;

import :MDecl;
import :MBodyDeclOuter;

import :MFuncDecl;
import :MCommonFuncDeclComponent;

import Citron.MAccessor;
import Citron.MNames;

namespace Citron
{
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