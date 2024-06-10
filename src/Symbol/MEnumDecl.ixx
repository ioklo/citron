export module Citron.Symbols:MEnumDecl;

import <vector>;
import <optional>;
import <memory>;
import <variant>;

import :MAccessor;
import :MNames;
import :MEnumElemDecl;
import :MTypeDeclOuter;

namespace Citron
{

export class MEnumDecl
{
    MTypeDeclOuter outer;
    MAccessor accessor;

    MName name;
    std::vector<std::string> typeParams;

    std::optional<std::vector<std::shared_ptr<MEnumElemDecl>>> elems; // lazy initialization

    // std::unordered_map<std::string, int> elemsByName;
};

}

