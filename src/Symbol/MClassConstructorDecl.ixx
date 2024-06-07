module;
#include "Macros.h"

export module Citron.Symbols:MClassConstructorDecl;

import <memory>;
import :MAccessor;
import :MCommonFuncDeclComponent;

namespace Citron
{

class MClassDecl;
class MFuncParameter;

class MClassConstructorDecl : private MCommonFuncDeclComponent
{
    std::weak_ptr<MClassDecl> _class;
    MAccessor accessor;
    std::vector<MFuncParameter> parameters;
    bool bTrivial;

public:
    DECLARE_DEFAULTS(MClassConstructorDecl)
};


}