using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Citron.Collections;
using Citron.Infra;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using Citron.Symbol;

namespace Citron
{
    // 레퍼런스용 Big Step Evaluator, 다 속도가 느린 버전
    // TODO: Small Step으로 가야하지 않을까 싶다 (yield로 실행 point 잡는거 해보면 재미있을 것 같다)    
    public class Evaluator
    {
        [AllowNull]
        ModuleDriverContext driverContext;

        public static async ValueTask<int> EvalAsync(ImmutableArray<Action<ModuleDriverContext>> moduleDriverInitializers, SymbolId entry)
        {
            var evaluator = new Evaluator();
            evaluator.driverContext = new ModuleDriverContext(evaluator);

            foreach(var moduleDriverInitializer in moduleDriverInitializers)
            {
                moduleDriverInitializer.Invoke(evaluator.driverContext);
            }            

            var mainThread = new EvalThread(evaluator);
            var ret = mainThread.StackAlloc<IntValue>(TypeIds.Int);

            // TODO: static인거 확인하고, 인자 없는거 확인하고, int리턴하는지 확인하고
            await evaluator.ExecuteGlobalFuncAsync(entry, default, ret);

            return ret.GetInt();
        }

        public Value GetClassStaticMemberValue(SymbolId memberVarId)
        {
            var driver = driverContext.GetModuleDriver(memberVarId);
            return driver.GetClassStaticMemberValue(memberVarId);
        }
        
        public Value GetClassMemberValue(ClassValue classValue, SymbolId classMemberVarId)
        {
            // module이름으로 driver선택
            var driver = driverContext.GetModuleDriver(classMemberVarId);
            var index = driver.GetClassMemberVarIndex(classMemberVarId);

            return classValue.GetInstance().GetMemberValue(index);
        }

        public int GetTotalClassMemberVarCount(SymbolId classId)
        {
            // module이름으로 driver선택
            var driver = driverContext.GetModuleDriver(classId);
            return driver.GetTotalClassMemberVarCount(classId);
        }

        // Local은 이 단계에서는 보이지 않는다
        public async ValueTask ExecuteGlobalFuncAsync(SymbolId globalFuncId, ImmutableArray<Value> args, Value retValue)
        {
            // module이름으로 driver선택
            var driver = driverContext.GetModuleDriver(globalFuncId);
            await driver.ExecuteGlobalFuncAsync(globalFuncId, args, retValue);
        }

        public void ExecuteStructConstructor(SymbolId constructorId, LocalPtrValue thisValue, ImmutableArray<Value> args)
        {
            // module이름으로 driver선택
            var driver = driverContext.GetModuleDriver(constructorId);
            driver.ExecuteStructConstructor(constructorId, thisValue, args);
        }

        public ValueTask ExecuteClassMemberFuncAsync(SymbolId memberFuncId, Value? thisValue, ImmutableArray<Value> args, Value retValue)
        {
            // module이름으로 driver선택
            var driver = driverContext.GetModuleDriver(memberFuncId);
            return driver.ExecuteClassMemberFuncAsync(memberFuncId, thisValue, args, retValue);
        }

        public Value GetEnumElemMemberValue(EnumElemValue enumElemValue, SymbolId memberVarId)
        {
            // module이름으로 driver선택
            var driver = driverContext.GetModuleDriver(memberVarId);
            int index = driver.GetEnumElemMemberVarIndex(memberVarId);
            return enumElemValue.GetMemberValue(index);
        }

        public bool IsEnumElem(EnumValue value, SymbolId elemId)
        {
            return value.IsElem(elemId);
        }

        // 실패시 RuntimeFatalException        
        // null리턴하면 이 클래스에 진짜 Base가 없는 것이다
        public SymbolId? GetBaseClass(SymbolId classId)
        {
            var driver = driverContext.GetModuleDriver(classId);
            return driver.GetBaseClass(classId);
        }

        // target, class 둘다 확정 타입이 들어온다
        public bool IsDerivedClassOf(SymbolId targetId, SymbolId classId)
        {
            SymbolId? curId = targetId;

            while (curId != null)
            {
                if (EqualityComparer<SymbolId?>.Default.Equals(curId, classId))
                    return true;

                var baseId = GetBaseClass(curId);
                if (baseId == null)
                    break;

                curId = baseId;
            }

            return false;
        }

        public ValueTask ExecuteStructMemberFuncAsync(SymbolId memberFuncId, Value? thisValue, ImmutableArray<Value> args, Value retValue)
        {
            // module이름으로 driver선택
            var driver = driverContext.GetModuleDriver(memberFuncId);
            return driver.ExecuteStructMemberFuncAsync(memberFuncId, thisValue, args, retValue);
        }

        public void ExecuteClassConstructor(SymbolId constructorId, ClassValue thisValue, ImmutableArray<Value> args)
        {
            // module이름으로 driver선택
            var driver = driverContext.GetModuleDriver(constructorId);
            driver.ExecuteClassConstructor(constructorId, thisValue, args);
        }

        public TValue AllocValue<TValue>(TypeId typeId)
            where TValue : Value
        {
            return (TValue)AllocValue(typeId);
        }

        struct Allocator : ITypeIdVisitor<Value>
        {
            [AllowNull]
            ModuleDriverContext driverContext;

            public Allocator(ModuleDriverContext driverContext)
            {
                this.driverContext = driverContext;
            }

            Value ITypeIdVisitor<Value>.VisitFunc(FuncTypeId typeId)
            {
                throw new NotImplementedException();
            }

            Value ITypeIdVisitor<Value>.VisitLambda(LambdaTypeId typeId)
            {
                throw new NotImplementedException();
            }

            Value ITypeIdVisitor<Value>.VisitLocalPtr(LocalPtrTypeId typeId)
            {
                return new LocalPtrValue();
            }

            Value ITypeIdVisitor<Value>.VisitBoxPtr(BoxPtrTypeId typeId)
            {
                return new BoxPtrValue();
            }

            // int? => Nullable<int> 
            // int?? => Nullable<Nullable<int>>
            // C? => C
            // C?? => Nullable<C>
            Value ITypeIdVisitor<Value>.VisitNullable(NullableTypeId typeId)
            {
                // TODO: 임시: 원래는 runtime 모듈에서 생성해야 한다
                throw new NotImplementedException();
            }

            Value ITypeIdVisitor<Value>.VisitSymbol(SymbolTypeId typeId)
            {
                if (typeId.Equals(TypeIds.Bool))
                {
                    return new BoolValue();
                }
                else if (typeId.Equals(TypeIds.Int))
                {
                    return new IntValue();
                }
                else if (typeId.Equals(TypeIds.String))
                {
                    return new StringValue();
                }
                else if (typeId.Equals(TypeIds.Void))
                {
                    return VoidValue.Instance;
                }
                else if (typeId.IsList(out var _))
                {
                    return new ListValue();
                }
                else if (typeId.IsSeq())
                {
                    return new SeqValue();
                }

                // 이외에는 드라이버에서 처리
                var driver = driverContext.GetModuleDriver(typeId.SymbolId);
                return driver.Alloc(typeId.SymbolId);
            }

            Value ITypeIdVisitor<Value>.VisitTuple(TupleTypeId typeId)
            {
                // TODO: 임시 implementation, 추후 TupleValue제거
                // (int a, C c) => tuple<int, C>로 runtime 모듈에서 생성해야 한다
                var values = ImmutableArray.CreateBuilder<Value>(typeId.MemberVarIds.Length);
                foreach (var memberVarId in typeId.MemberVarIds)
                {
                    var value = memberVarId.TypeId.Accept<Allocator, Value>(ref this); // this에 저장할 것이 없기 때문에 바로 호출
                    values.Add(value);
                }

                return new TupleValue(values.MoveToImmutable());
            }

            Value ITypeIdVisitor<Value>.VisitTypeVar(TypeVarTypeId typeId)
            {
                throw new NotImplementedException();
            }
            
            Value ITypeIdVisitor<Value>.VisitVoid(VoidTypeId typeId)
            {
                return VoidValue.Instance;
            }
        }

        // 로컬을 만든다 스코프가 끝나면 제거한다
        // TODO: 인자로 stack을 받는다 public Value AllocValue(EvalStack stack, SymbolId typeId)
        public Value AllocValue(TypeId typeId)
        {
            var allocator = new Allocator(driverContext);
            return typeId.Accept<Allocator, Value>(ref allocator);
        }

        public void InitializeClassInstance(SymbolId classId, ImmutableArray<Value>.Builder builder)
        {
            var driver = driverContext.GetModuleDriver(classId);
            driver.InitializeClassInstance(classId, builder);
        } 

        // 최종으로 치환된 타입이 들어온다
        public ClassInstance AllocClassInstance(SymbolId classId)
        {   
            var memberVars = ImmutableArray.CreateBuilder<Value>();
            InitializeClassInstance(classId, memberVars);

            return new ClassInstance(classId, memberVars.ToImmutable());
        }

        public Value GetStructStaticMemberValue(SymbolId memberVarId)
        {
            var driver = driverContext.GetModuleDriver(memberVarId);
            return driver.GetStructStaticMemberValue(memberVarId);
        }

        public Value GetStructMemberValue(StructValue structValue, SymbolId memberVarId)
        {
            var driver = driverContext.GetModuleDriver(memberVarId);
            var index = driver.GetStructMemberVarIndex(memberVarId);
            return structValue.GetMemberValue(index);
        }

        public int GetTotalStructMemberVarCount(SymbolId structId)
        {
            // module이름으로 driver선택
            var driver = driverContext.GetModuleDriver(structId);
            return driver.GetTotalStructMemberVarCount(structId);
        }
    }
}
