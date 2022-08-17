using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Citron.Analysis;
using Citron.Collections;
using Citron.Module;
using R = Citron.IR0;
using Void = Citron.Infra.Void;

namespace Citron
{
    // 함수 실행단위
    public partial class IR0EvalContext
    {
        Evaluator evaluator;

        TypeContext typeContext;
        IR0EvalFlowControl flowControl;
        Value? thisValue;
        Value retValue;
        Value? yieldValue;

        public IR0EvalContext(Evaluator evaluator, Value retValue)
        {
            this.evaluator = evaluator;
            this.typeContext = TypeContext.Empty;
            this.flowControl = IR0EvalFlowControl.None;
            this.thisValue = null;
            this.retValue = retValue;
        }

        public IR0EvalContext(
            Evaluator evaluator,
            TypeContext typeContext,
            IR0EvalFlowControl flowControl,
            Value? thisValue,
            Value retValue)
        {
            this.evaluator = evaluator;
            this.typeContext = typeContext;
            this.flowControl = flowControl;
            this.thisValue = thisValue;
            this.retValue = retValue;
        }

        public bool IsFlowControl(IR0EvalFlowControl testValue)
        {
            return flowControl == testValue;
        }

        public IR0EvalFlowControl GetFlowControl()
        {
            return flowControl;
        }

        public void SetFlowControl(IR0EvalFlowControl newFlowControl)
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

        public RefValue AllocRefValue()
        {
            return evaluator.AllocRefValue();
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

        public IR0EvalContext NewLambdaContext(LambdaValue lambdaValue, Value result)
        {
            return new IR0EvalContext(evaluator, typeContext, IR0EvalFlowControl.None, lambdaValue, result);
        }
        
        // value는 TypeContext없는 실제 타입을 가지고 있고,
        // class는 TypeContext를 반영해야 한다
        public bool IsDerivedClassOf(ClassValue value, ClassSymbol @class)
        {
            var targetId = value.GetActualType();

            var classId = @class.GetSymbolId();
            var appliedClassId = typeContext.Apply(classId);

            return evaluator.IsDerivedClassOf(targetId, appliedClassId);
        }

        public bool IsEnumElem(EnumValue value, EnumElemSymbol enumElem)
        {
            var enumElemId = enumElem.GetSymbolId();
            var appliedEnumElemId = typeContext.Apply(enumElemId);

            return evaluator.IsEnumElem(value, appliedEnumElemId);
        }
    }
}
