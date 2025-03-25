export module Citron.MSymbol:MClassVarDecl;

import <memory>;

import :MDecl;
import :MDeclVisitor;
import :MAccessor;
import :MType;
import :MNames;

namespace Citron
{

export class MClassDecl;

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