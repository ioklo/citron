using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.CompileTime
{
    public partial class ScriptModuleInfo
    {
        public abstract class ItemMember
        {
            public abstract ItemPathEntry GetLocalId();
            public abstract ItemInfo? GetItemInfo();            
            
            public class Type : ItemMember<TypeInfo> { public Type(TypeInfo info) : base(info) { } }
            public class Func : ItemMember<FuncInfo> { public Func(FuncInfo info) : base(info) { } }
            public class Var : ItemMember<VarInfo> { public Var(VarInfo info) : base(info) { } }
        }

        public abstract class ItemMember<TItemInfo> : ItemMember where TItemInfo : ItemInfo
        {
            public TItemInfo Info { get; }
            public ItemMember(TItemInfo info) { Info = info; }
            public override ItemPathEntry GetLocalId() => Info.GetLocalId();
            public override ItemInfo? GetItemInfo() => Info;
        }
    }
}
