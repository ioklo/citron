﻿using Gum.Collections;
using M = Gum.CompileTime;
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
