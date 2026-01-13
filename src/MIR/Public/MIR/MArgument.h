#pragma once

#include <variant>

namespace Citron {

class MExp;
class MLoc;

struct MArgument_Exp
{
    MExp* exp;
};

struct MArgument_Ref
{
    MLoc* loc;
};

struct MArgument_Move // 실제 move가 일어날때만. [move] 인자를 다른 [move] 파라미터로 보내는건 move를 명시하긴 하지만 move는 아니다
{
    MLoc* loc;
};

struct MArgument_Params
{
    MExp* exp;
    int elemCount;
};

using MArgument = std::variant<MArgument_Exp, MArgument_Ref, MArgument_Move, MArgument_Params>;

}