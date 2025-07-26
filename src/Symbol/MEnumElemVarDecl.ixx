module;
#include <memory>
#include <optional>

export module Citron.MDecls:MEnumElemVarDecl;

import :MDecl;
import :MNames;

namespace Citron {

export class MType;
export using MTypePtr = std::shared_ptr<MType>;

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