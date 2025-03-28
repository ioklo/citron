export module Citron.MSymbol:MClassVarDecl;

import <memory>;

import :ForwardDecls;
import :MDecl;
import :MType;

import Citron.MAccessor;
import Citron.MNames;

namespace Citron
{

export class MClassVarDecl
    : public MDecl
{
    std::weak_ptr<MClassDecl> _class;

    MAccessor accessor;
    bool bStatic;
    MTypePtr declType;
    MName name;

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}