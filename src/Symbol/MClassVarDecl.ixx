module;
#include <memory>

export module Citron.MDecls:MClassVarDecl;

import :MDecl;
import :MType;

import :MAccessor;
import :MNames;

namespace Citron
{

export class MType;
export using MTypePtr = std::shared_ptr<MType>;

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