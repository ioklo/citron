using System;
using System.Collections.Generic;
using Gum.Collections;
using Gum.Infra;
using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    public partial class Evaluator
    {
        class ItemContainer
        {
            Dictionary<(R.Name, R.ParamHash), ItemContainer> containers;
            Dictionary<(R.Name, R.ParamHash), R.SequenceFuncDecl> seqFuncDecls;
            Dictionary<(R.Name, R.ParamHash), FuncInvoker> funcInvokers;

            public ItemContainer GetContainer(R.Name name, R.ParamHash paramHash)
            {
                return containers[(name, paramHash)];
            }

            public R.SequenceFuncDecl GetSequenceFuncDecl(R.Name name, R.ParamHash paramHash)
            {
                return seqFuncDecls[(name, paramHash)];
            }

            public FuncInvoker GetFuncInvoker(R.Name name, R.ParamHash paramHash)
            {
                return funcInvokers[(name, paramHash)];
            }
        }

        class SharedContext
        {
            public ImmutableArray<R.Decl> Decls { get; }

            Dictionary<(R.ModuleName, R.NamespacePath, R.Name, R.ParamHash), ItemContainer> rootContainers;

            public Dictionary<string, Value> PrivateGlobalVars { get; }

            public SharedContext(ImmutableArray<R.Decl> decls)
            {
                Decls = decls;
                PrivateGlobalVars = new Dictionary<string, Value>();
            }

            // 여기서 만들어 내면 됩니다
            public FuncInvoker GetFuncInvoker(R.Path.Normal normalPath)
            {
                var outer = GetOuterContainer(normalPath);
                return outer.GetFuncInvoker(normalPath.Name, normalPath.ParamHash);
            }

            ItemContainer GetOuterContainer(R.Path path)
            {
                if (path is R.Path.Root rootPath)
                {
                    return rootContainers[(rootPath.ModuleName, rootPath.NamespacePath, rootPath.Name, rootPath.ParamHash)];
                }
                else if (path is R.Path.Nested nestedPath)
                {
                    var outer = GetOuterContainer(nestedPath.Outer);
                    return outer.GetContainer(nestedPath.Name, nestedPath.ParamHash);
                }

                throw new UnreachableCodeException();
            }

            // X<>.Y<,>.F 가 나온다. TypeArgs정보는 따로 
            public R.SequenceFuncDecl GetSequenceFuncDecl(R.Path.Normal seqFunc)
            {
                var outer = GetOuterContainer(seqFunc);
                return outer.GetSequenceFuncDecl(seqFunc.Name, seqFunc.ParamHash);
            }
        }
    }    
}
