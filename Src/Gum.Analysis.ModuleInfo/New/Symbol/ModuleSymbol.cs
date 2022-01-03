using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gum.Collections;
using Pretune;

using M = Gum.CompileTime;
using R = Gum.IR0;

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

        public ImmutableArray<ITypeSymbolNode> GetTypeArgs()
        {
            return default;
        }

        public TypeEnv GetTypeEnv()
        {
            return TypeEnv.Empty;
        }

        public M.NormalTypeId MakeChildTypeId(M.Name childName, ImmutableArray<M.TypeId> childTypeArgs)
        {
            var name = decl.GetName();
            return new M.RootTypeId(name, null, childName, childTypeArgs);
        }

        public R.Path.Normal MakeRPath()
        {
            var name = decl.GetName();
            var rmoduleName = RItemFactory.MakeModuleName(name);
            return new R.Path.Root(rmoduleName);
        }

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
    }
}
