using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.IR0
{   
    public abstract record Loc;

    // 임시 value를 만들어서 Exp를 실행해서 대입해주는 역할, ExpInfo 대신 쓴다    
    public record TempLoc(Exp Exp, Path Type) : Loc;

    public record GlobalVarLoc(string Name) : Loc;
    public record LocalVarLoc(string Name) : Loc;
    public record CapturedVarLoc(string Name) : Loc;

    // l[b], l is list    
    public record ListIndexerLoc(Loc List, Exp Index) : Loc;    
    public record StaticMemberLoc(Path Type, string MemberName) : Loc;    
    public record StructMemberLoc(Loc Instance, Path.Nested structMember) : Loc;
    public record ClassMemberLoc(Loc Instance, string MemberName) : Loc;    
    public record EnumElemMemberLoc(Loc Instance, Path.Nested EnumElemField) : Loc;

    public record ThisLoc : Loc
    {
        public static readonly ThisLoc Instance = new ThisLoc();
        ThisLoc() { }
    }

    public record DerefLoc(Loc Loc) : Loc;
    

}
