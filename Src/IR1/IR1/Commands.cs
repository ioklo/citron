using System.Collections.Generic;
using Citron.Collections;

namespace Citron.IR1; // TODO: namespace 정책 변경..

record struct BasicBlockId(int Value);

// 
class BasicBlock
{
    BasicBlockId Id;
    ImmutableArray<Command> Commands;
}

public abstract record class Command
{
    // dest = src;
    public record class Assign(RegId DestId, RegId SrcId) : Command;

    // *destPtr = src;
    public record class AssignPtr : Command
    {
        public RegId DestPtrId { get; }
        public RegId SrcId { get; }

        public AssignPtr(RegId destPtrId, RegId srcId)
        {
            DestPtrId = destPtrId;
            SrcId = srcId;
        }
    }

    // p = *i;
    public class Deref : Command
    {
        public RegId DestId { get; }
        public RegId SrcRefId { get; }

        public Deref(RegId destId, RegId srcRefId)
        {
            DestId = destId;
            SrcRefId = srcRefId;
        }
    }

    public class Call : Command
    {
        public RegId? ResultId { get; }
        public FuncId FuncId { get; }
        public ImmutableArray<RegId> ArgIds { get; }

        public Call(RegId? resultId, FuncId funcId, IEnumerable<RegId> argIds)
        {
            ResultId = resultId;
            FuncId = funcId;
            ArgIds = argIds.ToImmutableArray();
        }
    }

    public class ExternalCall : Command
    {
        public RegId? ResultId { get; }
        public ExternalFuncId FuncId { get; }
        public ImmutableArray<RegId> ArgIds { get; }

        public ExternalCall(RegId? resultId, ExternalFuncId funcId, IEnumerable<RegId> argIds)
        {
            ResultId = resultId;
            FuncId = funcId;
            ArgIds = argIds.ToImmutableArray();
        }
    }

    public class HeapAlloc : Command
    {
        public RegId ResultRefId { get; }
        public AllocInfoId AllocInfoId { get; }
        public HeapAlloc(RegId resultRefId, AllocInfoId allocInfoId)
        {
            ResultRefId = resultRefId;
            AllocInfoId = allocInfoId;
        }
    }

    // MakeInt, MakeString / ConcatString, MakeBool로 갈지 고심, 일단 RuntimeModule이 드러나지 않도록 한다
    public class MakeInt : Command
    {
        public RegId ResultId { get; }
        public int Value { get; }
        public MakeInt(RegId resultId, int value)
        {
            ResultId = resultId;
            Value = value;
        }
    }
    
    public class MakeString : Command
    {
        public RegId ResultId { get; }
        public string Value { get; }
        public MakeString(RegId resultId, string value)
        {
            ResultId = resultId;
            Value = value;
        }
    }

    public class MakeBool : Command
    {
        public RegId ResultId { get; }
        public bool Value { get; }
        public MakeBool(RegId resultId, bool value)
        {
            ResultId = resultId;
            Value = value;
        }
    }

    public class MakeEnumerator : Command
    {
        public RegId ResultId { get; }
        public FuncId FuncId { get; }
        public AllocInfoId YieldAllocId { get; }
        public ImmutableArray<RegId> ArgIds { get; }            

        public MakeEnumerator(RegId resultId, FuncId funcId, AllocInfoId yieldAllocId, IEnumerable<RegId> argIds)
        {
            ResultId = resultId;
            FuncId = funcId;
            YieldAllocId = yieldAllocId;
            ArgIds = argIds.ToImmutableArray();
        }
    }

    public class ConcatStrings : Command
    {
        public RegId ResultId { get; }
        public ImmutableArray<RegId> StringIds { get; }
        public ConcatStrings(RegId resultId, IEnumerable<RegId> stringIds)
        {
            ResultId = resultId;
            StringIds = stringIds.ToImmutableArray();
        }
    }

    public class If : Command
    {
        public RegId CondId { get; }
        public Command ThenCommand { get; }  // 참일때 선택
        public Command? ElseCommand { get; }
        public If(RegId condId, Command thenCommand, Command? elseCommand)
        {
            CondId = condId;
            ThenCommand = thenCommand;
            ElseCommand = elseCommand;
        }
    }

    public record class Jump(BasicBlockId Target) : Command;

    public class Break : Command
    {
        public ScopeId ScopeId { get; }
        public Break(ScopeId scopeId)
        {
            ScopeId = scopeId;
        }
    }

    public class Continue : Command
    {
        public ScopeId ScopeId { get; }
        public Continue(ScopeId scopeId)
        {
            ScopeId = scopeId;
        }
    }

    public class SetReturnValue : Command
    {
        public RegId ValueId { get; }
        public SetReturnValue(RegId valueId)
        {
            ValueId = valueId;
        }
    }

    public class EnumeratorMoveNext : Command
    {
        public RegId DestId { get; }
        public RegId EnumeratorRefId { get; }

        public EnumeratorMoveNext(RegId destId, RegId enumeratorRefId)
        {
            DestId = destId;
            EnumeratorRefId = enumeratorRefId;
        }
    }

    public class EnumeratorGetValue : Command
    {
        public RegId DestId { get; }
        public RegId EnumeratorRefId { get; }
        public EnumeratorGetValue(RegId destId, RegId enumeratorRefId)
        {
            DestId = destId;
            EnumeratorRefId = enumeratorRefId;
        }
    }

    public class Yield : Command
    {
        public RegId ValueId { get; }
        public Yield(RegId valueId) { ValueId = valueId; }
    }

    public class Task : Command
    {
        public FuncId FuncId { get; }
        public ImmutableArray<RegId> ArgIds { get; }
        public Task(FuncId funcId, IEnumerable<RegId> argIds)
        {
            FuncId = funcId;
            ArgIds = argIds.ToImmutableArray();
        }
    }

    public class Async : Command
    {
        public FuncId FuncId { get; }
        public ImmutableArray<RegId> ArgIds { get; }
        public Async(FuncId funcId, IEnumerable<RegId> argIds)
        {
            FuncId = funcId;
            ArgIds = argIds.ToImmutableArray();
        }
    }

    public class Await : Command
    {
        public Command Command { get; }
        public Await(Command command)
        {
            Command = command;
        }
    }
    
    public class GetMemberRef : Command
    {
        public RegId ResultId { get; }
        public RegId InstanceRefId { get; }
        public MemberVarId MemberVarId { get; }

        public GetMemberRef(RegId resultId, RegId instanceRefId, MemberVarId memberVarId)
        {
            ResultId = resultId;
            InstanceRefId = instanceRefId;
            MemberVarId = memberVarId;
        }
    }

    public class ExternalGetMemberRef : Command
    {
        public RegId ResultId { get; }
        public RegId InstanceRefId { get; }
        public ExternalMemberVarId MemberVarId { get; }
        public ExternalGetMemberRef(RegId resultId, RegId instanceRefId, ExternalMemberVarId memberVarId)
        {
            ResultId = resultId;
            InstanceRefId = instanceRefId;
            MemberVarId = memberVarId;
        }
    }

    // S s;
    // s.x.y = 1;
    // 
    // [0] LocalAlloc S         // S* s = alloca()
    // [1] GetMemberVar [0] "x" // X* x = &s->x
    // [2] GetMemberVar [1] "y" // int* y = &x->y;
    // [3] Const (1)            // int* p = &1;
    // Assign [0] [3]           // *y = *p;

    // S s;
    // int& i = &s.x.y;
    // int& y = i;

    // [0] LocalAlloc S         // S* t0 = alloca();
    // [1] GetMemberVar [0] "x" // X* t1 = &s->x
    // [2] GetMemberVar [1] "y" // int* t2 = &x->y;
    // [3] Ref [2]              // int** t3; *t3 = t2;
    // [4] LocalAlloc int*      // int** y;
    // Assign [4] [3]           // *y = *t3;

    // C c = new C();
    // c.x.y = 1;
    //
    // [0, ][1]
    // [0] HeapAlloc
    // [1] Call C.Constructor [0]  // C.C(c);
    // [2] GetMemberVar            // c->x
}
