export module Citron.Symbols:MClassMemberVarDecl;

import "std.h";

import :MAccessor;
import :MType;
import :MNames;

namespace Citron
{

export class MClassDecl;

export class MClassMemberVarDecl
{
    std::weak_ptr<MClassDecl> _class;

    MAccessor accessor;
    bool bStatic;
    MType declType;
    MName name;
};

}