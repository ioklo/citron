export module Citron.MSymbol:MStructFuncDecl;

import <memory>;
import <vector>;

import :ForwardDecls;
import :MDecl;
import :MBodyDeclOuter;
import :MFuncDecl;
import :MCommonFuncDeclComponent;

import Citron.MAccessor;
import Citron.MNames;

namespace Citron
{

export class MStructFuncDecl
    : public MDecl
    , public MBodyDeclOuter
    , public MFuncDecl
    , private MCommonFuncDeclComponent
{
    std::weak_ptr<MStructDecl> _struct;
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