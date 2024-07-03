module;
#include "Macros.h"

export module Citron.Symbols:MStructConstructorDecl;

import "std.h";

import :MAccessor;
import :MCommonFuncDeclComponent;

namespace Citron
{

export class MStructDecl;
export class MFuncParameter;

export class MStructConstructorDecl : private MCommonFuncDeclComponent
{
    std::weak_ptr<MStructDecl> _struct;
    MAccessor accessor;
    std::vector<MFuncParameter> parameters;
    bool bTrivial;

public:
    // DECLARE_DEFAULTS(MStructConstructorDecl)
};


}