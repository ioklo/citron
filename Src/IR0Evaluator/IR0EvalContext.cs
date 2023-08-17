using Citron.Collections;
using Citron.IR0;
using Citron.Symbol;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace Citron
{
    partial class IR0EvalContext
    {
        IR0GlobalContext globalContext;
        IR0BodyContext bodyContext;
        IR0LocalContext localContext;

        public IR0EvalContext(IR0GlobalContext globalContext, IR0BodyContext bodyContext, IR0LocalContext localContext)
        {
            this.globalContext = globalContext;
            this.bodyContext = bodyContext;
            this.localContext = localContext;
        }

        public IR0EvalContext NewLambdaContext(LambdaValue lambdaValue, Value result, ImmutableDictionary<Name, Value> localVars)
        {
            var newBodyContext = bodyContext.NewLambdaContext(lambdaValue, result);
            var newLocalContext = new IR0LocalContext(localVars, tasks: default);

            return new IR0EvalContext(globalContext, newBodyContext, newLocalContext);
        }

        public IR0EvalContext NewScopeContext()
        {
            var newLocalContext = new IR0LocalContext(localContext);
            return new IR0EvalContext(globalContext, bodyContext, newLocalContext);
        }

        public IR0EvalContext NewTaskLocalContext()
        {
            var newLocalContext = localContext.NewTaskLocalContext();
            return new IR0EvalContext(globalContext, bodyContext, newLocalContext);
        }
        
        #region GlobalContext
        public Value GetStructStaticMemberValue(SymbolId symbolId) => globalContext.GetStructStaticMemberValue(symbolId);
        public Value GetStructMemberValue(StructValue structValue, SymbolId symbolId) => globalContext.GetStructMemberValue(structValue, symbolId);
        public Value GetClassStaticMemberValue(SymbolId symbolId) => globalContext.GetClassStaticMemberValue(symbolId);
        public Value GetClassMemberValue(ClassValue classValue, SymbolId symbolId) => globalContext.GetClassMemberValue(classValue, symbolId);
        public Value GetEnumElemMemberValue(EnumElemValue enumElemValue, SymbolId symbolId) => globalContext.GetEnumElemMemberValue(enumElemValue, symbolId);
        public IType? GetListItemType(IType listType) => globalContext.GetListItemType(listType);
        public ImmutableArray<Stmt> GetBodyStmt(SymbolId symbolId) => globalContext.GetBodyStmt(symbolId);
        public Task ExecuteCommandAsync(string cmdText) => globalContext.ExecuteCommandAsync(cmdText);
        #endregion GlobalContext

        #region BodyContext
        public Value AllocValue(IType type) => bodyContext.AllocValue(type);
        public TValue AllocValue<TValue>(TypeId typeId) where TValue : Value => bodyContext.AllocValue<TValue>(typeId);
        public Value GetThisValue() => bodyContext.GetThisValue();

        // typeContext때문에 global이 아니라 bodyContext에서 수행해야 한다
        public ValueTask ExecuteGlobalFuncAsync(GlobalFuncSymbol func, ImmutableArray<Value> args, Value result) => bodyContext.ExecuteGlobalFuncAsync(func, args, result);

        public void ExecuteClassConstructor(ClassConstructorSymbol constructor, ClassValue thisValue, ImmutableArray<Value> args)
            => bodyContext.ExecuteClassConstructor(constructor, thisValue, args);
        public ValueTask ExecuteClassMemberFuncAsync(ClassMemberFuncSymbol classMemberFunc, Value? thisValue, ImmutableArray<Value> args, Value result)
            => bodyContext.ExecuteClassMemberFuncAsync(classMemberFunc, thisValue, args, result);
        public ClassInstance AllocClassInstance(ClassSymbol classSymbol) 
            => bodyContext.AllocClassInstance(classSymbol);

        public void ExecuteStructConstructor(StructConstructorSymbol constructor, LocalPtrValue thisValue, ImmutableArray<Value> args)
            => bodyContext.ExecuteStructConstructor(constructor, thisValue, args);
        public ValueTask ExecuteStructMemberFuncAsync(StructMemberFuncSymbol structMemberFunc, Value? thisValue, ImmutableArray<Value> args, Value result)
            => bodyContext.ExecuteStructMemberFuncAsync(structMemberFunc, thisValue, args, result);
        
        public IR0EvalFlowControl GetFlowControl() => bodyContext.GetFlowControl();
        public void SetFlowControl(IR0EvalFlowControl newFlowControl) => bodyContext.SetFlowControl(newFlowControl);
        public Value GetRetValue() => bodyContext.GetRetValue();
        public Value GetYieldValue() => bodyContext.GetYieldValue();

        public bool IsEnumElem(EnumValue value, EnumElemSymbol enumElem) => bodyContext.IsEnumElem(value, enumElem);
        public bool IsDerivedClassOf(ClassValue value, ClassSymbol @class) => bodyContext.IsDerivedClassOf(value, @class);
        #endregion BodyContext

        #region LocalContext
        public Value GetLocalValue(Name name) => localContext.GetLocalValue(name);
        public void AddLocalVar(Name name, Value value) => localContext.AddLocalVar(name, value);

        public void AddTask(Task task) => localContext.AddTask(task);
        public Task WaitAllAsync() => localContext.WaitAllAsync();
        #endregion LocalContext
    }
}