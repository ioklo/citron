using Gum.Collections;
using M = Gum.CompileTime;
using R = Gum.IR0;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pretune;

namespace Gum.Analysis
{
    [AutoConstructor]
    public partial class NamespaceSymbol : ITopLevelSymbolNode
    {
        ITopLevelSymbolNode outer;
        NamespaceDeclSymbol decl;

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
            return outer;
        }

        public (M.Name Module, M.NamespacePath? NamespacePath) GetRootPath()
        {
            var (module, outerNamespacePath) = outer.GetRootPath();
            var name = decl.GetName();

            return (module, new M.NamespacePath(outerNamespacePath, name));
        }

        public ImmutableArray<ITypeSymbolNode> GetTypeArgs()
        {
            return default;
        }

        public TypeEnv GetTypeEnv()
        {
            return TypeEnv.Empty;
        }

        public M.NormalTypeId MakeChildTypeId(M.Name childTypeName, ImmutableArray<M.TypeId> typeArgs)
        {
            var (module, nsPath) = GetRootPath();
            return new M.RootTypeId(module, nsPath, childTypeName, typeArgs);
        }

        public R.Path.Normal MakeRPath()
        {
            var name = decl.GetName();
            var rname = RItemFactory.MakeName(name);

            var outerPath = outer.MakeRPath();
            return new R.Path.Nested(outerPath, rname, R.ParamHash.None, default);
        }

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
    }
}
