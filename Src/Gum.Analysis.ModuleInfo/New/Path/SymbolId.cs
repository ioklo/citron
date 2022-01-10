using Gum.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using M = Gum.CompileTime;

namespace Gum.Analysis
{
    // Module, SymbolPath
    public record SymbolId;
    
    public record ModuleSymbolId(M.Name ModuleName, SymbolPath? Path) : SymbolId;   

    public record TypeVarSymbolId(DeclSymbolId DeclSymbolId) : SymbolId;
    
    public record NullableSymbolId(SymbolId InnerTypeId) : SymbolId;

    public record VoidSymbolId : SymbolId;    

    public static class SymbolIdExtensions
    {
        public static ModuleSymbolId Child(this ModuleSymbolId id, M.Name name, ImmutableArray<SymbolId> typeArgs = default, M.ParamTypes paramTypes = default)
        {
            return new ModuleSymbolId(id.ModuleName, id.Path.Child(name, typeArgs, paramTypes));
        }
    }
}
