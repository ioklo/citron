using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Citron.Collections;
using Citron.Infra;
using Citron.CompileTime;
using Citron.Analysis;
using System.Diagnostics;
using Citron.IR0;

namespace Citron
{
    public class IR0GlobalContext
    {
        Evaluator evaluator;
        IR0Loader loader;
        Name internalModuleName;
        IIR0CommandProvider commandProvider;
        Dictionary<string, Value> globalVars;        

        public IR0GlobalContext(Evaluator evaluator, IR0Loader loader, Name internalModuleName, IIR0CommandProvider commandProvider)
        {
            this.evaluator = evaluator;
            this.loader = loader;
            this.internalModuleName = internalModuleName;
            this.commandProvider = commandProvider;
            this.globalVars = new Dictionary<string, Value>();
        }
        
        public TSymbol LoadSymbol<TSymbol>(SymbolId symbolId)
            where TSymbol : class, ISymbolNode
        {
            var moduleSymbolId = symbolId as ModuleSymbolId;
            Debug.Assert(moduleSymbolId != null);
            Debug.Assert(moduleSymbolId.Path != null);

            return loader.LoadSymbol<TSymbol>(moduleSymbolId.Path);
        }

        public Value GetGlobalValue(string name)
        {
            return globalVars[name];
        }

        public IR0EvalContext NewEvalContext(TypeContext typeContext, Value? thisValue, Value retValue)
        {   
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

        public Value GetStructStaticMemberValue(SymbolId memberVarId)
        {
            return evaluator.GetStructStaticMemberValue(memberVarId);
        }

        public Value GetStructMemberValue(StructValue structValue, SymbolId memberVarId)
        {
            return evaluator.GetStructMemberValue(structValue, memberVarId);
        }        

        public Value GetClassStaticMemberValue(SymbolId memberVarId)
        {
            return evaluator.GetClassStaticMemberValue(memberVarId);
        }

        public Value GetClassMemberValue(ClassValue classValue, SymbolId memberVarId)
        {
            return evaluator.GetClassMemberValue(classValue, memberVarId);
        }

        public Value GetEnumElemMemberValue(EnumElemValue enumElemValue, SymbolId memberVarId)
        {
            return evaluator.GetEnumElemMemberValue(enumElemValue, memberVarId);
        }

        public Task ExecuteCommandAsync(string cmdText)
        {
            return commandProvider.ExecuteAsync(cmdText);
        }

        public ImmutableArray<Stmt> GetBodyStmt(SymbolId symbolId)
        {
            var moduleSymbolId = symbolId as ModuleSymbolId;
            Debug.Assert(moduleSymbolId != null);

            var declSymbolId = moduleSymbolId.GetDeclSymbolId();
            Debug.Assert(declSymbolId.ModuleName.Equals(internalModuleName));
            Debug.Assert(declSymbolId.Path != null);

            return loader.GetBody(declSymbolId.Path);
        }

        public Name GetInternalModuleName()
        {
            return internalModuleName;
        }

        // symbol의 사용범위.. IR0에 
        public ITypeSymbol? GetListItemType(ITypeSymbol listType)
        {
            var listTypeId = listType.GetSymbolId() as ModuleSymbolId;
            Debug.Assert(listTypeId != null);
            if (listTypeId.IsList(out var _))
                return listType.GetTypeArg(0);

            return null;
        }
    }
}
