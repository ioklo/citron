using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Linq;
using System.Text;

namespace Citron.IR1; // TODO: namespace 정책 변경..

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
