module;
#include "Macros.h"

export module Citron.Symbols:MStructConstructorDecl;

import <memory>;
import :MAccessor;
import :MCommonFuncDeclComponent;

namespace Citron
{

class MStructDecl;
class MFuncParameter;

class MStructConstructorDecl : private MCommonFuncDeclComponent
{
    std::weak_ptr<MStructDecl> _struct;
    MAccessor accessor;
    std::vector<MFuncParameter> parameters;
    bool bTrivial;

public:
    DECLARE_DEFAULTS(MStructConstructorDecl)
};


}