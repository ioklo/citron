using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gum.Collections;
using Pretune;

using M = Gum.CompileTime;

namespace Gum.Analysis
{
    [AutoConstructor]
    public partial class ModuleSymbol : ITopLevelSymbolNode
    {
        ModuleDeclSymbol decl;

        public ITopLevelSymbolNode Apply(TypeEnv typeEnv)
        {
            return this;
        }

        public IDeclSymbolNode GetDeclSymbolNode()
        {
            return decl;
        }

        public ISymbolNode? GetOuter()
        {
            return null;
        }

        public (M.Name Module, M.NamespacePath? NamespacePath) GetRootPath()
        {
            var name = decl.GetName();
            return (name, null);
        }

        public ImmutableArray<ITypeSymbol> GetTypeArgs()
        {
            return default;
        }

        public TypeEnv GetTypeEnv()
        {
            return TypeEnv.Empty;
        }

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
    }
}
