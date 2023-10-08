using Citron.Collections;
using Citron.Symbol;

namespace Citron.IR2;

// FuncInfo
public struct FuncInfo
{
    // local변수들의 타입. 미리 숫자로 계산할 수도 있지만, 라이브러리가 동적으로 로딩될 경우,
    // 컴파일 시점과 크기가 다를 수도 있는 상황을 지원하기 위해서 typeId만 갖고 동적으로 계산한다
    ImmutableArray<TypeId> localInfos;

    // Basic blocks
    ImmutableArray<BasicBlock> basicBlocks;
}