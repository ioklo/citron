using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Citron.Collections;
using Citron.Infra;
using System.Diagnostics;
using Citron.IR0;
using Citron.Symbol;

namespace Citron
{
    public class IR0GlobalContext
    {
        Evaluator evaluator;
        IR0Loader loader;
        Name internalModuleName;
        IIR0CommandProvider commandProvider;

        public IR0GlobalContext(Evaluator evaluator, IR0Loader loader, Name internalModuleName, IIR0CommandProvider commandProvider)
        {
            this.evaluator = evaluator;
            this.loader = loader;
            this.internalModuleName = internalModuleName;
            this.commandProvider = commandProvider;
        }
        
        public TSymbol LoadSymbol<TSymbol>(SymbolId symbolId)
            where TSymbol : class, ISymbolNode
        {   
            Debug.Assert(symbolId != null);
            Debug.Assert(symbolId.Path != null);

            return loader.LoadSymbol<TSymbol>(symbolId.Path);
        }
        
        public IR0BodyContext NewBodyContext(TypeContext typeContext, Value? thisValue, Value retValue)
        {   
            return new IR0BodyContext(evaluator, typeContext, IR0EvalFlowControl.None, thisValue, retValue);
        }

        public IR0BodyContext NewBodyContext()
        {
            return new IR0BodyContext(evaluator, TypeContext.Empty, IR0EvalFlowControl.None, null, VoidValue.Instance);
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
            var declSymbolId = symbolId.GetDeclSymbolId();
            Debug.Assert(declSymbolId.ModuleName.Equals(internalModuleName));
            Debug.Assert(declSymbolId.Path != null);

            return loader.GetBody(declSymbolId.Path);
        }

        public Name GetInternalModuleName()
        {
            return internalModuleName;
        }

        // symbol의 사용범위.. IR0에 
        public IType? GetListItemType(IType listType)
        {
            if (listType.GetTypeId().IsList(out var _))
                return listType.GetTypeArg(0);

            return null;
        }

        
    }
}
