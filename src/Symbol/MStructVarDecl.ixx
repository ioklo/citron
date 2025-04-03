export module Citron.MDecls:MStructVarDecl;

import <memory>;

import :MDecl;
import :MType;

import :MAccessor;
import :MNames;

namespace Citron
{

export class MType;
export using MTypePtr = std::shared_ptr<MType>;

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