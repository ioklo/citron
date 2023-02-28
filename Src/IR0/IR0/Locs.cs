using Citron.Symbol;
using Pretune;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.IR0
{
    public abstract record class Loc : INode
    {   
    }

    // 임시 value를 만들어서 Exp를 실행해서 대입해주는 역할, ExpInfo 대신 쓴다    
    public record class TempLoc(Exp Exp) : Loc;
    public record class LocalVarLoc(Name Name) : Loc;

    // only this member allowed, so no need this
    public record class LambdaMemberVarLoc(LambdaMemberVarSymbol MemberVar) : Loc;

    // l[b], l is list    
    public record class ListIndexerLoc(Loc List, Exp Index) : Loc;

    // Instance가 null이면 static
    public record class StructMemberLoc(Loc? Instance, StructMemberVarSymbol MemberVar) : Loc;
    public record ClassMemberLoc(Loc? Instance, ClassMemberVarSymbol MemberVar) : Loc;

    public record class EnumElemMemberLoc(Loc Instance, EnumElemMemberVarSymbol MemberVar) : Loc;
    public record class ThisLoc() : Loc;
    
    // 두가지 버전
    public record class DerefLocLoc(Loc Loc) : Loc;
    public record class DerefExpLoc(Exp Exp) : Loc;

    // nullable value에서 value를 가져온다
    public record class NullableValueLoc(Loc Loc) : Loc;
}
