export module Citron.Symbols:MClassMemberFuncDecl;

import "std.h";

import :MAccessor;
import :MNames;
import :MCommonFuncDeclComponent;

namespace Citron
{

export class MClassDecl;

export class MClassMemberFuncDecl : private MCommonFuncDeclComponent
{
    std::weak_ptr<MClassDecl> _class;
    MAccessor accessor;
    MName name;
    std::vector<std::string> typeParams;
    bool bStatic;
};

}