using System;
using System.Collections.Generic;
using Gum.Collections;
using Gum.Infra;
using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    public partial class Evaluator
    {
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

            // 
            public void AddRootItemContainer(R.ModuleName moduleName, ItemContainer container)
            {
                rootContainers.Add(moduleName, container);
            }
        }
    }    
}
