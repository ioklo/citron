using Citron.Analysis;
using Citron.CompileTime;
using Pretune;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.IR0
{   
    public abstract record Loc : INode;

    // 임시 value를 만들어서 Exp를 실행해서 대입해주는 역할, ExpInfo 대신 쓴다    
    public record TempLoc(Exp Exp) : Loc;
    public record GlobalVarLoc(string Name) : Loc;

    public record LocalVarLoc(Name Name) : Loc;    

    // only this member allowed, so no need this
    public record LambdaMemberVarLoc(LambdaMemberVarSymbol MemberVar) : Loc;

    // l[b], l is list    
    public record ListIndexerLoc(Loc List, Exp Index) : Loc;

    // Instance가 null이면 static
    public record StructMemberLoc(Loc? Instance, StructMemberVarSymbol MemberVar) : Loc;
    public record ClassMemberLoc(Loc? Instance, ClassMemberVarSymbol MemberVar) : Loc;

    public record EnumElemMemberLoc(Loc Instance, EnumElemMemberVarSymbol MemberVar) : Loc;
    public record ThisLoc() : Loc;
    
    // 두가지 버전
    public record DerefLocLoc(Loc Loc) : Loc;
    public record DerefExpLoc(Exp Exp) : Loc;

    // nullable value에서 value를 가져온다
    public record NullableValueLoc(Loc Loc) : Loc;
}
