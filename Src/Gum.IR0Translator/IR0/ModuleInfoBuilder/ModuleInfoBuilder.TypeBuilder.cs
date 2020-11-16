using Gum.CompileTime;
using System;
using System.Collections.Generic;

namespace Gum.IR0
{
    partial class ModuleInfoBuilder
    {
        class TypeBuilder
        {
            private AppliedItemPath thisTypePath;

            private List<TypeInfo> typeInfos;
            private List<FuncInfo> funcInfos;
            private List<VarInfo> varInfos;

            public TypeBuilder(AppliedItemPath thisTypePath)
            {
                this.thisTypePath = thisTypePath;
                typeInfos = new List<TypeInfo>();
                funcInfos = new List<FuncInfo>();
                varInfos = new List<VarInfo>();
            }

            public AppliedItemPath GetThisTypeAppliedPath()
            {
                return thisTypePath;
            }

            internal void AddTypeInfo(TypeInfo typeInfo)
            {
                throw new NotImplementedException();
            }

            internal void AddFuncInfo(FuncInfo funcInfo)
            {
                throw new NotImplementedException();
            }

            internal void AddVarInfo(VarInfo varInfo)
            {
                throw new NotImplementedException();
            }
        }
    }
}