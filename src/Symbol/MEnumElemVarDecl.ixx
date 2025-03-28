export module Citron.MSymbol:MEnumElemVarDecl;

import <memory>;
import <optional>;

import :ForwardDecls;
import :MDecl;
import :MType;

import Citron.MNames;

namespace Citron
{

export class MEnumElemVarDecl
    : public MDecl
{
    std::weak_ptr<MEnumElemDecl> outer;
    MName name;

    MTypePtr declType; // lazy-init

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
};


}