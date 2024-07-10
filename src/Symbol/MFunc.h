#pragma once

#include "MGlobalFunc.h"

#include "MClassConstructor.h"
#include "MClassMemberFunc.h"

#include "MStructConstructor.h"
#include "MStructMemberFunc.h"

#include "MLambda.h"

namespace Citron
{

using MFunc = std::variant<
    MGlobalFunc,        // top-level decl space

    MClassConstructor,  // construct decl space
    MClassMemberFunc,   // construct decl space

    MStructConstructor, // struct decl space
    MStructMemberFunc,  // struct decl space
    
    MLambda             // body space
>;


}