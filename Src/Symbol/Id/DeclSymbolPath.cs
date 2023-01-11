using Pretune;
using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Linq;

using Citron.Infra;

using System.Diagnostics.CodeAnalysis;

namespace Citron.Symbol
{
    // Namespace / ... / Type / ... 
    public record class DeclSymbolPath(DeclSymbolPath? Outer, Name Name, int TypeParamCount = 0, ImmutableArray<FuncParamId> ParamIds = default);

    public static class DeclSymbolPathExtensions
    {
        public static DeclSymbolPath Child(this DeclSymbolPath? outer, string name, int typeParamCount = 0, ImmutableArray<FuncParamId> paramIds = default)
        {
            return new DeclSymbolPath(outer, new Name.Normal(name), typeParamCount, paramIds);
        }

        public static DeclSymbolPath Child(this DeclSymbolPath? outer, Name name, int typeParamCount = 0, ImmutableArray<FuncParamId> paramIds = default)
        {
            return new DeclSymbolPath(outer, name, typeParamCount, paramIds);
        }

        [return: NotNullIfNotNull("path")]
        public static DeclSymbolPath? GetDeclSymbolPath(this SymbolPath? path)
        {
            if (path == null) return null;

            var outerDeclPath = path.Outer.GetDeclSymbolPath();
            return new DeclSymbolPath(outerDeclPath, path.Name, path.TypeArgs.Length, path.ParamIds);
        }
    }
}