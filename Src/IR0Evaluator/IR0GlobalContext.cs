using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Citron.Collections;
using Citron.Infra;
using R = Citron.IR0;
using Citron.CompileTime;
using Citron.Analysis;
using System.Diagnostics;

namespace Citron
{
    public class IR0GlobalContext
    {
        Evaluator evaluator;
        SymbolLoader symbolLoader;
        Name internalModuleName;
        IIR0CommandProvider commandProvider;
        Dictionary<R.ModuleName, IItemContainer> rootContainers;
        Dictionary<string, Value> globalVars;

        public IR0GlobalContext(Evaluator evaluator, SymbolLoader symbolLoader, Name internalModuleName, IIR0CommandProvider commandProvider)
        {
            this.evaluator = evaluator;
            this.internalModuleName = internalModuleName;
            this.commandProvider = commandProvider;
            this.rootContainers = new Dictionary<R.ModuleName, IItemContainer>();
            this.globalVars = new Dictionary<string, Value>();
        }

        public TSymbol LoadSymbol<TSymbol>(SymbolPath symbolPath)
            where TSymbol : class, ISymbolNode
        {
            return (TSymbol)symbolLoader.Load(new ModuleSymbolId(internalModuleName, symbolPath));
        }

        public Value GetGlobalValue(string name)
        {
            return globalVars[name];
        }

        public IR0EvalContext NewEvalContext(SymbolPath path, Value? thisValue, Value retValue)
        {
            var typeContext = TypeContext.Make(path);
            return new IR0EvalContext(evaluator, typeContext, IR0EvalFlowControl.None, thisValue, retValue);
        }

        public IR0EvalContext NewEvalContext()
        {
            return new IR0EvalContext(evaluator, TypeContext.Empty, IR0EvalFlowControl.None, null, VoidValue.Instance);
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

        public Value GetStructStaticMemberValue(SymbolId memberVarId)
        {
            return evaluator.GetStructStaticMemberValue(memberVarId);
        }

        public Value GetStructMemberValue(StructValue structValue, SymbolId memberVarId)
        {
            return evaluator.GetStructMemberValue(structValue, memberVarId);
        }

        public TRuntimeItem GetRuntimeItem<TRuntimeItem>(R.Path.Nested path)
            where TRuntimeItem : RuntimeItem
        {
            var outer = GetContainer(path.Outer);
            return outer.GetRuntimeItem<TRuntimeItem>(path.Name, path.ParamHash);
        }

        public Value GetClassStaticMemberValue(SymbolId memberVarId)
        {
            return evaluator.GetClassStaticMemberValue(memberVarId);
        }

        public Value GetClassMemberValue(ClassValue classValue, SymbolId memberVarId)
        {
            return evaluator.GetClassMemberValue(classValue, memberVarId);
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

        public R.Stmt GetBodyStmt(SymbolPath path)
        {
            throw new NotImplementedException();
        }

        public Name GetInternalModuleName()
        {
            return internalModuleName;
        }

        public ITypeSymbol? GetListItemType(ITypeSymbol listType)
        {
            var listTypeId = listType.GetSymbolId() as ModuleSymbolId;
            Debug.Assert(listTypeId != null);
            if (listTypeId.IsList(out var itemId))
                return symbolLoader.Load(itemId) as ITypeSymbol;

            return null;
        }
    }
}
