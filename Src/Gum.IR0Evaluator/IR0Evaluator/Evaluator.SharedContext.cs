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
            Dictionary<(R.Name, R.ParamHash), RuntimeItem> runtimeItems;
            
            Dictionary<R.Name, R.LambdaDecl> lambdaDecls;
            Dictionary<R.Name, R.CapturedStatementDecl> capturedStatementDecls;
            Dictionary<R.Name, R.EnumElement> enumElems;

            public ItemContainer()
            {
                containers = new Dictionary<(R.Name, R.ParamHash), ItemContainer>();

                runtimeItems = new Dictionary<(R.Name, R.ParamHash), RuntimeItem>();

                lambdaDecls = new Dictionary<R.Name, R.LambdaDecl>();
                capturedStatementDecls = new Dictionary<R.Name, R.CapturedStatementDecl>();
                enumElems = new Dictionary<R.Name, R.EnumElement>();
            }

            public ItemContainer GetContainer(R.Name name, R.ParamHash paramHash)
            {
                return containers[(name, paramHash)];
            }

            public TRuntimeItem GetRuntimeItem<TRuntimeItem>(R.Name name, R.ParamHash paramHash)
                where TRuntimeItem : RuntimeItem
            {
                return (TRuntimeItem)runtimeItems[(name, paramHash)];
            }
            
            public R.CapturedStatementDecl GetCapturedStatementDecl(R.Name name)
            {
                return capturedStatementDecls[name];
            }

            public R.LambdaDecl GetLambdaDecl(R.Name name)
            {
                return lambdaDecls[name];
            }

            public void AddLambdaDecl(R.LambdaDecl lambdaDecl)
            {
                lambdaDecls.Add(lambdaDecl.Name, lambdaDecl);
            }

            public void AddItemContainer(R.Name name, R.ParamHash paramHash, ItemContainer itemContainer)
            {
                containers.Add((name, paramHash), itemContainer);
            }
            
            public void AddRuntimeItem(RuntimeItem runtimeItem)
            {
                runtimeItems.Add((runtimeItem.Name, runtimeItem.ParamHash), runtimeItem);
            }
            
            public void AddCapturedStmtDecl(R.CapturedStatementDecl capturedStmtDecl)
            {
                capturedStatementDecls.Add(capturedStmtDecl.Name, capturedStmtDecl);
            }

            public void AddEnumElem(R.EnumElement enumElem)
            {
                enumElems.Add(enumElem.Name, enumElem);
            }

            public R.EnumElement GetEnumElem(R.Name name)
            {
                return enumElems[name];
            }
        }

        class SharedContext
        {
            Dictionary<R.ModuleName, ItemContainer> rootContainers;

            public R.EnumElement GetEnumElem(R.Path.Nested enumElem)
            {
                var outer = GetContainer(enumElem.Outer);
                return outer.GetEnumElem(enumElem.Name);
            }

            public Dictionary<string, Value> PrivateGlobalVars { get; }

            public SharedContext()
            {
                rootContainers = new Dictionary<R.ModuleName, ItemContainer>();
                PrivateGlobalVars = new Dictionary<string, Value>();
            }

            // 여기서 만들어 내면 됩니다
            public FuncRuntimeItem GetFuncInvoker(R.Path.Nested normalPath)
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

            public TRuntimeItem GetRuntimeItem<TRuntimeItem>(R.Path.Nested path)
                where TRuntimeItem : RuntimeItem
            {
                var outer = GetContainer(path.Outer);
                return outer.GetRuntimeItem<TRuntimeItem>(path.Name, path.ParamHash);
            }

            public R.LambdaDecl GetLambdaDecl(R.Path.Nested lambda)
            {
                var outer = GetContainer(lambda.Outer);
                return outer.GetLambdaDecl(lambda.Name);
            }

            public R.CapturedStatementDecl GetCapturedStatementDecl(R.Path.Nested path)
            {
                var outer = GetContainer(path.Outer);
                return outer.GetCapturedStatementDecl(path.Name);
            }

            // 
            public void AddRootItemContainer(R.ModuleName moduleName, ItemContainer container)
            {
                rootContainers.Add(moduleName, container);
            }
        }
    }    
}
