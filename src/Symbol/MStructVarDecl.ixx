export module Citron.MSymbol:MStructVarDecl;

import <memory>;

import :MDecl;
import :MDeclVisitor;
import :MAccessor;
import :MType;
import :MNames;

namespace Citron
{

export class MStructDecl;

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