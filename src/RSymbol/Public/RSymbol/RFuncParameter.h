#pragma once
#include "RSymbolConfig.h"
#include <string>

#include "RNames.h"

namespace Citron {

class RType;
class RTypeArguments;
class RFactory;

enum class RFuncParameterKind
{
    Normal,  // 일반
    Ref,     // T&
    In,      // [in] T&
    Move,    // [move] T& 
    Forward, // [forword] T&
    Out,     // [out] T&
    Params,  // [params] T
    Init,    // memberwise ctor용
};

struct RFuncParameter
{
    RFuncParameterKind kind;
    RType* type; // 람다의 경우 지정이 안될 수 있다
    RName name;

    bool IsRef() {
        return kind == RFuncParameterKind::Ref
            || kind == RFuncParameterKind::In
            || kind == RFuncParameterKind::Move
            || kind == RFuncParameterKind::Forward
            || kind == RFuncParameterKind::Out; // Init은 뺀다        
    }       

    RSYMBOL_API RFuncParameter Apply(RTypeArguments& typeArgs);
};


}
