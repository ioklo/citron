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
            Dictionary<R.ModuleName, IItemContainer> rootContainers;

            public Dictionary<string, Value> PrivateGlobalVars { get; }

            public SharedContext()
            {
                rootContainers = new Dictionary<R.ModuleName, IItemContainer>();
                PrivateGlobalVars = new Dictionary<string, Value>();
            }            

            IItemContainer GetContainer(R.Path path)
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
            public void AddRootItemContainer(R.ModuleName moduleName, IItemContainer container)
            {
                rootContainers.Add(moduleName, container);
            }
        }
    }    
}
