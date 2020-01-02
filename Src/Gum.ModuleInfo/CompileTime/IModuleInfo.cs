using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Gum.CompileTime
{
    public interface IModuleInfo
    {
        string ModuleName { get; }
        
        bool GetTypeInfo(ModuleItemId id, [NotNullWhen(returnValue: true)] out ITypeInfo? typeInfo);
        bool GetFuncInfo(ModuleItemId id, [NotNullWhen(returnValue: true)] out FuncInfo? funcInfo);
        bool GetVarInfo(ModuleItemId id, [NotNullWhen(returnValue: true)] out VarInfo? varInfo);
    }
}
