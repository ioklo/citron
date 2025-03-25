export module Citron.MSymbol:MEnumDecl;

import <vector>;
import <optional>;
import <memory>;

import :MDecl;
import :MTypeDecl;
import :MAccessor;
import :MNames;
import :MEnumElemDecl;
import :MTypeDeclOuter;

namespace Citron
{

export class MEnumDecl
    : public MDecl
    , public MTypeDecl
{
    MTypeDeclOuterWPtr outer;
    MAccessor accessor;

    MName name;
    std::vector<std::string> typeParams;

    std::optional<std::vector<std::shared_ptr<MEnumElemDecl>>> elems; // lazy initialization

    // std::unordered_map<std::string, int> elemsByName;

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}

