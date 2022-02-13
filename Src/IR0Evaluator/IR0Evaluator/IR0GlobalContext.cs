using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Citron.Collections;
using Citron.Infra;
using R = Citron.IR0;
using M = Citron.CompileTime;
using Citron.Analysis;
using Citron.CompileTime;

namespace Citron.IR0Evaluator
{
    public class IR0GlobalContext
    {
        Evaluator evaluator;
        IIR0CommandProvider commandProvider;
        Dictionary<R.ModuleName, IItemContainer> rootContainers;
        Dictionary<string, Value> globalVars;

        public IR0GlobalContext(Evaluator evaluator, IIR0CommandProvider commandProvider)
        {
            this.evaluator = evaluator;
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

        public RefValue AllocRefValue()
        {
            return evaluator.AllocRefValue();
        }

        public R.Stmt GetBodyStmt(SymbolId symbolId)
        {
            throw new NotImplementedException();
        }
    }
}
