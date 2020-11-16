using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Gum.CompileTime
{
    public interface IModuleInfo
    {
        ModuleName GetModuleName();

        ItemInfo? GetItem(NamespacePath namespacePath, ItemPathEntry itemIdEntry);            

        IEnumerable<FuncInfo> GetFuncs(NamespacePath namespacePath, Name funcName);
        VarInfo? GetVar(NamespacePath namespacePath, Name funcName);
    }
}
