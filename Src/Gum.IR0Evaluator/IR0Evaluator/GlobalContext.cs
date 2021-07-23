using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gum.Collections;
using Gum.Infra;
using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    public class GlobalContext
    {
        ICommandProvider commandProvider;

        Dictionary<R.ModuleName, IItemContainer> rootContainers;
        Dictionary<string, Value> globalVars;

        public GlobalContext(ICommandProvider commandProvider)
        {
            this.commandProvider = commandProvider;

            this.rootContainers = new Dictionary<R.ModuleName, IItemContainer>();
            this.globalVars = new Dictionary<string, Value>();
        }

        public Value GetGlobalValue(string name)
        {
            return globalVars[name];
        }

        public void AddGlobalVar(string name, Value value)
        {
            globalVars.Add(name, value);
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

        public Task ExecuteCommandAsync(string cmdText)
        {
            return commandProvider.ExecuteAsync(cmdText);
        }

        public TValue AllocValue<TValue>(R.Path type)
        where TValue : Value
        {
            return (TValue)AllocValue(type);
        }

        public RefValue AllocRefValue()
        {
            return new RefValue();
        }

        // type은 ir0 syntax의 일부분이다
        public Value AllocValue(R.Path typePath)
        {
            if (typePath.Equals(R.Path.Bool))
            {
                return new BoolValue();
            }
            else if (typePath.Equals(R.Path.Int))
            {
                return new IntValue();
            }
            else if (typePath.Equals(R.Path.String))
            {
                return new StringValue();
            }
            else if (typePath.Equals(R.Path.VoidType.Instance))
            {
                return VoidValue.Instance;
            }
            else if (R.PathExtensions.IsTypeInstOfList(typePath))
            {
                return new ListValue();
            }
            else if (R.PathExtensions.IsTypeInstOfListIter(typePath))
            {
                return new SeqValue();
            }

            if (typePath is R.Path.Nested nestedTypePath)
            {
                var runtimeItem = GetRuntimeItem<AllocatableRuntimeItem>(nestedTypePath);
                var typeContext = TypeContext.Make(nestedTypePath);

                return runtimeItem.Alloc(typeContext);
            }

            throw new NotImplementedException();
        }
    }
}
