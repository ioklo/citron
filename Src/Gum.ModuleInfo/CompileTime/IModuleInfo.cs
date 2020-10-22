using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Gum.CompileTime
{
    public interface IModuleInfo
    {
        bool GetTypeInfo(ModuleItemId id, [NotNullWhen(true)] out ITypeInfo? typeInfo);
        bool GetFuncInfo(ModuleItemId id, [NotNullWhen(true)] out FuncInfo? funcInfo);
        bool GetVarInfo(ModuleItemId id, [NotNullWhen(true)] out VarInfo? varInfo);
    }
}
