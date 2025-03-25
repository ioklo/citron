export module Citron.MSymbol:MEnumElemVarDecl;

import <memory>;
import <optional>;

import :MDecl;
import :MDeclVisitor;
import :MNames;
import :MType;

namespace Citron
{

export class MEnumElemDecl;

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