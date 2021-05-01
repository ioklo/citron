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
            Dictionary<(R.Name, R.ParamHash), R.LambdaDecl> lambdaDecls;
            Dictionary<(R.Name, R.ParamHash), FuncInvoker> funcInvokers;

            public ItemContainer()
            {
                containers = new Dictionary<(R.Name, R.ParamHash), ItemContainer>();
                seqFuncDecls = new Dictionary<(R.Name, R.ParamHash), R.SequenceFuncDecl>();
                lambdaDecls = new Dictionary<(R.Name, R.ParamHash), R.LambdaDecl>();
                funcInvokers = new Dictionary<(R.Name, R.ParamHash), FuncInvoker>();
            }

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

            public R.LambdaDecl GetLambdaDecl(R.Name name)
            {
                return lambdaDecls[(name, R.ParamHash.None)];
            }

            public void AddLambdaDecl(R.LambdaDecl lambdaDecl)
            {
                lambdaDecls.Add((new R.Name.Lambda(lambdaDecl.Id), R.ParamHash.None), lambdaDecl);
            }

            public void AddItemContainer(R.Name name, R.ParamHash paramHash, ItemContainer itemContainer)
            {
                containers.Add((name, paramHash), itemContainer);
            }
        }

        class SharedContext
        {
            Dictionary<R.ModuleName, ItemContainer> rootContainers;

            public Dictionary<string, Value> PrivateGlobalVars { get; }

            public SharedContext()
            {
                rootContainers = new Dictionary<R.ModuleName, ItemContainer>();
                PrivateGlobalVars = new Dictionary<string, Value>();
            }

            // 여기서 만들어 내면 됩니다
            public FuncInvoker GetFuncInvoker(R.Path.Nested normalPath)
            {
                var outer = GetContainer(normalPath.Outer);
                return outer.GetFuncInvoker(normalPath.Name, normalPath.ParamHash);
            }

            ItemContainer GetContainer(R.Path path)
            {
                if (path is R.Path.Root rootPath)
                {
                    return rootContainers[rootPath.ModuleName];
                }
                else if (path is R.Path.Nested nestedPath)
                {
                    var outer = GetContainer(nestedPath.Outer);
                    return outer.GetContainer(nestedPath.Name, nestedPath.ParamHash);
                }

                throw new UnreachableCodeException();
            }

            // X<>.Y<,>.F 가 나온다. TypeArgs정보는 따로 
            public R.SequenceFuncDecl GetSequenceFuncDecl(R.Path.Nested seqFunc)
            {
                var outer = GetContainer(seqFunc.Outer);
                return outer.GetSequenceFuncDecl(seqFunc.Name, seqFunc.ParamHash);
            }

            public R.LambdaDecl GetLambdaDecl(R.Path.Nested lambda)
            {
                var outer = GetContainer(lambda.Outer);
                return outer.GetLambdaDecl(lambda.Name);
            }

            // 
            public void AddRootItemContainer(R.ModuleName moduleName, ItemContainer container)
            {
                rootContainers.Add(moduleName, container);
            }
        }
    }    
}
