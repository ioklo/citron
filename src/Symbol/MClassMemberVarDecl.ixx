export module Citron.Symbols:MClassMemberVarDecl;

import <memory>;
import :MAccessor;
import :MType;
import :MNames;

namespace Citron
{

class MClassDecl;

class MClassMemberVarDecl
{
    std::weak_ptr<MClassDecl> _class;

    MAccessor accessor;
    bool bStatic;
    MType declType;
    MName name;
};

}