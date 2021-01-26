using Gum.CompileTime;
using System;
using System.Collections.Generic;

namespace Gum.IR0
{
    partial class ModuleInfoBuilder
    {
        // struct, class 류를 빌드하는데 사용
        class TypeBuilder
        {
            private ItemPath typePath;

            private List<TypeInfo> memberTypes;
            private List<FuncInfo> memberFuncs;
            private List<MemberVarInfo> memberVars;

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
        }        
    }
}