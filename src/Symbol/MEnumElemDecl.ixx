export module Citron.MSymbol:MEnumElemDecl;

import <optional>;
import <vector>;
import <memory>;
import <string>;

import :ForwardDecls;
import :MDecl;
import :MTypeDecl;

namespace Citron
{

export class MEnumElemDecl
    : public MDecl
    , public MTypeDecl
{
    std::weak_ptr<MEnumDecl> _enum;
    std::string name;
    std::optional<std::vector<MEnumElemVarDecl>> vars; // lazy-init

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}