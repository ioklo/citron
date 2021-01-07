using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Gum.CompileTime
{
    public interface IModuleInfo
    {
        ModuleName GetModuleName();

        ItemInfo? GetGlobalItem(NamespacePath namespacePath, ItemPathEntry itemIdEntry);

        IEnumerable<FuncInfo> GetGlobalFuncs(NamespacePath namespacePath, Name funcName);
        VarInfo? GetGlobalVar(NamespacePath namespacePath, Name funcName);
    }
}
