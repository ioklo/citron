using Gum.CompileTime;
using System;
using System.Collections.Generic;

namespace Gum.IR0
{
    partial class ModuleInfoBuilder
    {
        class TypeBuilder
        {
            private ItemPath typePath;

            private List<TypeInfo> typeInfos;
            private List<FuncInfo> funcInfos;
            private List<VarInfo> varInfos;

            public TypeBuilder(ItemPath typePath)
            {
                this.typePath = typePath;
                typeInfos = new List<TypeInfo>();
                funcInfos = new List<FuncInfo>();
                varInfos = new List<VarInfo>();
            }

            public ItemPath GetTypePath()
            {
                return typePath;
            }

            public void AddTypeInfo(TypeInfo typeInfo)
            {
                typeInfos.Add(typeInfo);
            }

            public void AddFuncInfo(FuncInfo funcInfo)
            {
                funcInfos.Add(funcInfo);
            }

            public void AddVarInfo(VarInfo varInfo)
            {
                varInfos.Add(varInfo);
            }
        }

        
    }
}