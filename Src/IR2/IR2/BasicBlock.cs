using Citron.Collections;

namespace Citron.IR2;

public struct BasicBlock
{
    // 커맨드, 마지막은 항상 jump형(return, if, ...) 로 끝나거나 never가 return값인 함수 호출로 끝난다
    ImmutableArray<Command> commands;
}

