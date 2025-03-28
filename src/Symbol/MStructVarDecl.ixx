export module Citron.MSymbol:MStructVarDecl;

import <memory>;

import :ForwardDecls;
import :MDecl;
import :MType;

import Citron.MAccessor;
import Citron.MNames;

namespace Citron
{
export class MStructVarDecl
    : public MDecl
{
    std::weak_ptr<MStructDecl> _struct;

    MAccessor accessor;
    bool bStatic;
    MTypePtr declType;
    MName name;

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
};



}