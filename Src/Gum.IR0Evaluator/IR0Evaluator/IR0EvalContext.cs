using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Citron;
using Gum;
using Citron.Analysis;
using Gum.Collections;
using Gum.CompileTime;
using R = Gum.IR0;
using Void = Gum.Infra.Void;

namespace Gum.IR0Evaluator
{
    // 함수 실행단위
    public partial class IR0EvalContext
    {
        Evaluator evaluator;

        TypeContext typeContext;
        EvalFlowControl flowControl;
        Value? thisValue;
        Value retValue;
        Value? yieldValue;

        public IR0EvalContext(Evaluator evaluator, Value retValue)
        {
            this.evaluator = evaluator;
            this.typeContext = TypeContext.Empty;
            this.flowControl = EvalFlowControl.None;
            this.thisValue = null;
            this.retValue = retValue;
        }

        public IR0EvalContext(
            Evaluator evaluator,
            TypeContext typeContext,
            EvalFlowControl flowControl,
            Value? thisValue,
            Value retValue)
        {
            this.evaluator = evaluator;
            this.typeContext = typeContext;
            this.flowControl = flowControl;
            this.thisValue = thisValue;
            this.retValue = retValue;
        }

        public Value GetStaticValue(R.Path type)
        {
            throw new NotImplementedException();
        }

        public bool IsFlowControl(EvalFlowControl testValue)
        {
            return flowControl == testValue;
        }

        public EvalFlowControl GetFlowControl()
        {
            return flowControl;
        }

        public void SetFlowControl(EvalFlowControl newFlowControl)
        {
            flowControl = newFlowControl;
        }

        public Value GetRetValue()
        {
            return retValue!;
        }

        // struct 이면 refValue, boxed struct 이면 boxValue, class 이면 ClassValue
        public Value GetThisValue()
        {
            Debug.Assert(thisValue != null);
            return thisValue;
        }
        
        public void SetYieldValue(Value value)
        {
            yieldValue = value;
        }

        public Value GetYieldValue()
        {
            return yieldValue!;
        }

        // typeContext와 연관이 있으므로 GlobalContext가 아니라 EvalContext에서 수행한다
        public ClassInstance AllocClassInstance(ClassSymbol classSymbol)
        {
            var classId = classSymbol.GetSymbolId();
            var appliedClassId = typeContext.Apply(classId);
            
            return evaluator.AllocClassInstance(appliedClassId);
        }

        public TValue AllocValue<TValue>(SymbolId typeId)
            where TValue : Value
        {
            var appliedTypeId = typeContext.Apply(typeId);
            return evaluator.AllocValue<TValue>(appliedTypeId);
        }

        public Value AllocValue(SymbolId typeId)
        {
            return AllocValue<Value>(typeId);
        }

        public Value AllocValue(ITypeSymbol type)
        {
            var typeId = type.GetSymbolId();
            return AllocValue<Value>(typeId);
        }

        public ValueTask ExecuteGlobalFuncAsync(GlobalFuncSymbol globalFunc, ImmutableArray<Value> args, Value retValue)
        {
            var globalFuncId = globalFunc.GetSymbolId();
            var appliedGlobalFuncId = typeContext.Apply(globalFuncId);
            return evaluator.ExecuteGlobalFuncAsync(appliedGlobalFuncId, args, retValue);
        }

        public void ExecuteClassConstructor(ClassConstructorSymbol constructor, ClassValue thisValue, ImmutableArray<Value> args)
        {
            var constructorId = constructor.GetSymbolId();
            var appliedConstructorId = typeContext.Apply(constructorId);

            evaluator.ExecuteClassConstructor(appliedConstructorId, thisValue, args);
        }

        public void ExecuteStructConstructor(StructConstructorSymbol constructor, StructValue thisValue, ImmutableArray<Value> args)
        {
            var constructorId = constructor.GetSymbolId();
            var appliedConstructorId = typeContext.Apply(constructorId);

            evaluator.ExecuteStructConstructor(appliedConstructorId, thisValue, args);
        }

        public ValueTask ExecuteClassMemberFuncAsync(ClassMemberFuncSymbol classMemberFunc, Value? thisValue, ImmutableArray<Value> args, Value result)
        {
            var funcId = classMemberFunc.GetSymbolId();
            var appliedFuncId = typeContext.Apply(funcId);

            return evaluator.ExecuteClassMemberFuncAsync(appliedFuncId, thisValue, args, result);
        }

        public ValueTask ExecuteStructMemberFuncAsync(StructMemberFuncSymbol structMemberFunc, Value? thisValue, ImmutableArray<Value> args, Value result)
        {
            var funcId = structMemberFunc.GetSymbolId();
            var appliedFuncId = typeContext.Apply(funcId);

            return evaluator.ExecuteStructMemberFuncAsync(appliedFuncId, thisValue, args, result);
        }
    }
}
