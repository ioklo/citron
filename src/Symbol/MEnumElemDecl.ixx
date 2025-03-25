export module Citron.MSymbol:MEnumElemDecl;

import <optional>;
import <vector>;
import <memory>;
import <string>;

import :MDecl;
import :MTypeDecl;
import :MEnumElemVarDecl;

namespace Citron
{

export class MEnumDecl;

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