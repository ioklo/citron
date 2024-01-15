#pragma once
#include "SyntaxConfig.h"

namespace Citron::Syntax {

enum class UnaryOpKind
{
    PostfixInc, PostfixDec,

    Minus, LogicalNot, PrefixInc, PrefixDec,

    Ref, Deref, // &, *, local���� box������ �м��� ���ؼ� �˾Ƴ��� �ȴ�
};

} // namespace Citron::Syntax
