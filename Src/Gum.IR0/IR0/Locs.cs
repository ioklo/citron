using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.IR0
{   
    public abstract class Loc
    {
    }

    // l[b], l is list
    [AutoConstructor, ImplementIEquatable]
    public partial class ListIndexerLoc : Loc
    {
        public Loc List { get; }
        public Loc Index { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class StaticMemberLoc : Loc
    {
        public Path Type { get; }
        public string MemberName { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class StructMemberLoc : Loc
    {
        public Loc Instance { get; }
        public string MemberName { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class ClassMemberLoc : Loc
    {
        public Loc Instance { get; }
        public string MemberName { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class EnumMemberLoc : Loc
    {
        public Loc Instance { get; }
        public string MemberName { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class GlobalVarLoc : Loc
    {
        public string Name { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class LocalVarLoc : Loc
    {
        public string Name { get; } // LocalVarId;
    }

    // 임시 value를 만들어서 Exp를 실행해서 대입해주는 역할, ExpInfo 대신 쓴다
    [AutoConstructor, ImplementIEquatable]
    public partial class TempLoc : Loc 
    {
        public Exp Exp { get; }
        public Path Type { get; }
    }
}
