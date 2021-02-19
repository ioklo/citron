using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gum.IR0Translator
{
    partial class ModuleInfoBuilder
    {
        // struct, class 류를 빌드하는데 사용
        class TypeBuilder
        {
            ItemPath typePath;
            List<TypeInfo> memberTypes;
            List<FuncInfo> memberFuncs;
            List<MemberVarInfo> memberVars;

            public TypeBuilder(ItemPath typePath)
            {
                this.typePath = typePath;
                memberTypes = new List<TypeInfo>();
                memberFuncs = new List<FuncInfo>();
                memberVars = new List<MemberVarInfo>();
            }

            public ItemPath GetTypePath()
            {
                return typePath;
            }

            public void AddTypeInfo(TypeInfo typeInfo)
            {
                memberTypes.Add(typeInfo);
            }

            public void AddFuncInfo(FuncInfo funcInfo)
            {
                memberFuncs.Add(funcInfo);
            }

            public void AddMemberVarInfo(MemberVarInfo varInfo)
            {
                memberVars.Add(varInfo);
            }

            public TTypeInfo MakeTypeInfo<TTypeInfo>(Func<ImmutableArray<TypeInfo>, ImmutableArray<FuncInfo>, ImmutableArray<MemberVarInfo>, TTypeInfo> constructor)
                where TTypeInfo : TypeInfo
            {
                return constructor.Invoke(memberTypes.ToImmutableArray(), memberFuncs.ToImmutableArray(), memberVars.ToImmutableArray());
            }
        }        
    }
}