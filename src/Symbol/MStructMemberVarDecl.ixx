export module Citron.Symbols:MStructMemberVarDecl;

import "std.h";

import :MAccessor;
import :MType;
import :MNames;

namespace Citron
{

export class MStructDecl;

export class MStructMemberVarDecl
{
    std::weak_ptr<MStructDecl> _struct;

    MAccessor accessor;
    bool bStatic;
    MType declType;
    MName name;
};

}