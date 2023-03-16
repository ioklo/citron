using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Linq;
using System.Text;

namespace Citron.IR1 // TODO: namespace 정책 변경..
{
    // 범위: 전역
    public struct ExternalDriverId
    {
        public string Value { get; }
        public ExternalDriverId(string value) { Value = value; }

        public override bool Equals(object? obj)
        {
            return obj is ExternalDriverId id &&
                   Value == id.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }
    }

    public struct AllocInfoId
    {
        public const int RefValue = -1;
        public const int BoolValue = -2;
        public const int IntValue = -3;

        public static AllocInfoId RefId { get; } = new AllocInfoId(RefValue);
        public static AllocInfoId BoolId { get; } = new AllocInfoId(BoolValue);
        public static AllocInfoId IntId { get; } = new AllocInfoId(IntValue);

        public int Value { get; }
        public AllocInfoId(int value) { Value = value; }
    }

    public struct FuncId
    {
        public int Value { get; }
        public FuncId(int value) { Value = value; }
    }

    public struct GlobalVarId
    {
        public int Value { get; }
        public GlobalVarId(int value) { Value = value; }
    }

    public struct MemberVarId
    {
        public int Value { get; }
        public MemberVarId(int value) { Value = value; }
    }

    public struct ExternalMemberVarId
    {
        public int Value { get; }
        public ExternalMemberVarId(int value) { Value = value; }
    }

    public struct ExternalFuncId
    {
        public int Value { get; }
        public ExternalFuncId(int value) { Value = value; }
    }

    public struct ExVarId
    {
        public int Value { get; }
        public ExVarId(int value ) { Value = value; }
    }

    public struct ScopeId
    {
        public int Value { get; }
        public ScopeId(int value) { Value = value; }

        public override bool Equals(object? obj)
        {
            return obj is ScopeId id &&
                   Value == id.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }

        public static bool operator ==(ScopeId left, ScopeId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ScopeId left, ScopeId right)
        {
            return !(left == right);
        }
    }

    // Func에 속하는 LocalId
    public struct RegId
    {
        public int Value { get; }
        public RegId(int value) { Value = value; }
    }    


    // 레지스터 정보
    public class Reg
    {
        public RegId Id { get; }
        public AllocInfoId AllocInfoId { get; }
        public Reg(RegId id, AllocInfoId allocInfoId)
        {
            Id = id;
            AllocInfoId = allocInfoId;
        }
    }

    public class CompAllocInfo
    {
        public AllocInfoId Id { get; }
        public ImmutableArray<AllocInfoId> MemberIds { get; }

        public CompAllocInfo(AllocInfoId id, IEnumerable<AllocInfoId> memberIds)
        {
            Id = id;
            MemberIds = memberIds.ToImmutableArray();
        }
    }

    public class Func
    {
        public FuncId Id { get; }
        public ImmutableArray<Reg> Regs { get; }
        public Command.Scope Body { get; }

        public Func(FuncId id, IEnumerable<Reg> regs, Command.Scope body)
        {
            Id = id;
            Regs = regs.ToImmutableArray();
            Body = body;
        }
    }

    public class GlobalVar
    {
        public GlobalVarId Id { get; }
        public AllocInfoId AllocInfoId { get; }
        public GlobalVar(GlobalVarId id, AllocInfoId allocInfoId)
        {
            Id = id;
            AllocInfoId = allocInfoId;
        }
    }

    public class MemberVar
    {
        public MemberVarId Id { get; }
    }

    public class ExternalFunc
    {
        // 모듈정보
        public ExternalFuncId Id { get; }
        public ExternalDriverId DriverId { get; }
        public ExternalDriverFuncId DriverFuncId { get; }
        public ImmutableArray<AllocInfoId> AllocInfoIds { get; }

        public ExternalFunc(ExternalFuncId id, ExternalDriverId driverId, ExternalDriverFuncId driverFuncId, IEnumerable<AllocInfoId> allocInfoIds)
        {
            Id = id;
            DriverId = driverId;
            DriverFuncId = driverFuncId;
            AllocInfoIds = allocInfoIds.ToImmutableArray();
        }
    }

    public class ExVar
    {
        public ExVarId Id { get; }
    }    

    // 
    public class Script
    {   
        public ImmutableArray<ExternalFunc> ExFuncs { get; }
        public ImmutableArray<GlobalVar> GlobalVars { get; }
        public ImmutableArray<Func> Funcs { get; }
        public FuncId EntryId { get; }
        public Script(IEnumerable<ExternalFunc> exFuncs, IEnumerable<GlobalVar> globalVars, IEnumerable<Func> funcs, FuncId entryId)
        {
            ExFuncs = exFuncs.ToImmutableArray();
            GlobalVars = globalVars.ToImmutableArray();
            Funcs = funcs.ToImmutableArray();
            EntryId = entryId;
        }
    }

    public abstract class Command
    {
        public class Scope : Command
        {
            public ScopeId Id { get; }
            public Command Command { get; }
            public Scope(ScopeId id, Command command)
            {
                Id = id;
                Command = command;
            }
        }

        public class Sequence : Command
        {
            public ImmutableArray<Command> Commands { get; }

            public Sequence(IEnumerable<Command> commands)
            {
                Commands = commands.ToImmutableArray();
            }
        }

        public class Assign : Command
        {
            public RegId DestId { get; }
            public RegId SrcId { get; }

            public Assign(RegId destId, RegId srcId)
            {
                DestId = destId;
                SrcId = srcId;
            }
        }

        // *p = i;
        public class AssignRef : Command
        {
            public RegId DestRefId { get; }
            public RegId SrcId { get; }

            public AssignRef(RegId destRefId, RegId srcId)
            {
                DestRefId = destRefId;
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

        public class GetGlobalRef : Command
        {
            public RegId ResultId { get; }
            public GlobalVarId GlobalVarId { get; }

            public GetGlobalRef(RegId resultId, GlobalVarId globalVarId)
            {
                ResultId = resultId;
                GlobalVarId = globalVarId;
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
}
