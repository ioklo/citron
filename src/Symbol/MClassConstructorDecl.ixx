module;
#include "Macros.h"

export module Citron.Symbols:MClassConstructorDecl;

import "std.h";

import :MAccessor;
import :MCommonFuncDeclComponent;

namespace Citron
{

export class MClassDecl;
export class MFuncParameter;

export class MClassConstructorDecl : private MCommonFuncDeclComponent
{
    std::weak_ptr<MClassDecl> _class;
    MAccessor accessor;
    std::vector<MFuncParameter> parameters;
    bool bTrivial;
};


}