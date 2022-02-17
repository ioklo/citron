using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Citron.CompileTime;

using Citron.Collections;
using Citron.Infra;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;

namespace Citron
{
    // 레퍼런스용 Big Step Evaluator, 다 속도가 느린 버전
    // TODO: Small Step으로 가야하지 않을까 싶다 (yield로 실행 point 잡는거 해보면 재미있을 것 같다)    
    public class Evaluator
    {
        public static async ValueTask<int> EvalAsync(ImmutableArray<Func<ModuleDriverInitializationContext, IModuleDriver>> moduleDriverConstructors, ModuleSymbolId entry)
        {
            // 일단 module들 등록
            var initContext = new ModuleDriverInitializationContext();
            foreach(var moduleDriverConstructor in moduleDriverConstructors)
            {
                //  여기에 등록 컨텍스트를 넣어서 모듈에서 알아서 등록하도록
                moduleDriverConstructor.Invoke(initContext);
            }

            var evaluator = new Evaluator();
            // MakeEvaluator with initContext
            // var context = ;

            var mainThread = new EvalThread();
            var ret = mainThread.StackAlloc<IntValue>(ModuleSymbolId.Int);

            // TODO: static인거 확인하고, 인자 없는거 확인하고, int리턴하는지 확인하고            
            await evaluator.ExecuteGlobalFuncAsync(entry, default, ret);

            return ret.GetInt();
        }
        
        internal Evaluator()
        {
        }        

        (IModuleDriver Driver, SymbolPath Path) GetModuleDriverAndPath(SymbolId Id)
        {
            throw new NotImplementedException();
        }

        public Value GetClassStaticMemberValue(SymbolId memberVarId)
        {
            var (driver, path) = GetModuleDriverAndPath(memberVarId);
            return driver.GetClassStaticMemberValue(path);
        }
        
        public Value GetClassMemberValue(ClassValue classValue, SymbolId classMemberVarId)
        {
            // module이름으로 driver선택
            var (driver, path) = GetModuleDriverAndPath(classMemberVarId);
            var index = driver.GetClassMemberVarIndex(path);

            return classValue.GetInstance().GetMemberValue(index);
        }

        public int GetTotalClassMemberVarCount(SymbolId classId)
        {
            // module이름으로 driver선택
            var (driver, classPath) = GetModuleDriverAndPath(classId);
            return driver.GetTotalClassMemberVarCount(classPath);
        }

        // Local은 이 단계에서는 보이지 않는다
        public async ValueTask ExecuteGlobalFuncAsync(SymbolId globalFuncId, ImmutableArray<Value> args, Value retValue)
        {
            // module이름으로 driver선택
            var (driver, path) = GetModuleDriverAndPath(globalFuncId);
            await driver.ExecuteGlobalFuncAsync(path, args, retValue);
        }

        public void ExecuteStructConstructor(SymbolId constructorId, StructValue thisValue, ImmutableArray<Value> args)
        {
            // module이름으로 driver선택
            var (driver, path) = GetModuleDriverAndPath(constructorId);
            driver.ExecuteStructConstructor(path, thisValue, args);
        }

        public ValueTask ExecuteClassMemberFuncAsync(SymbolId memberFuncId, Value? thisValue, ImmutableArray<Value> args, Value retValue)
        {
            // module이름으로 driver선택
            var (driver, path) = GetModuleDriverAndPath(memberFuncId);
            return driver.ExecuteClassMemberFuncAsync(path, thisValue, args, retValue);
        }

        public bool IsEnumElem(EnumValue value, SymbolId elemId)
        {
            return value.IsElem(elemId);
        }

        // 실패시 RuntimeFatalException        
        // null리턴하면 이 클래스에 진짜 Base가 없는 것이다
        public SymbolId? GetBaseClass(SymbolId classId)
        {
            var (driver, path) = GetModuleDriverAndPath(classId);
            return driver.GetBaseClass(path);
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
            var (driver, path) = GetModuleDriverAndPath(memberFuncId);
            return driver.ExecuteStructMemberFuncAsync(path, thisValue, args, retValue);
        }

        public void ExecuteClassConstructor(SymbolId constructorId, ClassValue thisValue, ImmutableArray<Value> args)
        {
            // module이름으로 driver선택
            var (driver, path) = GetModuleDriverAndPath(constructorId);
            driver.ExecuteClassConstructor(path, thisValue, args);
        }

        public TValue AllocValue<TValue>(SymbolId typeId)
            where TValue : Value
        {
            return (TValue)AllocValue(typeId);
        }

        public RefValue AllocRefValue()
        {
            return new RefValue();
        }

        // 로컬을 만든다 스코프가 끝나면 제거한다
        // TODO: 인자로 stack을 받는다 public Value AllocValue(EvalStack stack, SymbolId typeId)
        public Value AllocValue(SymbolId typeId)
        {
            // 임시. 원래는 로딩된 runtime에서 해줘야 한다
            if (typeId.Equals(SymbolId.Bool))
            {
                return new BoolValue();
            }
            else if (typeId.Equals(SymbolId.Int))
            {
                return new IntValue();
            }
            else if (typeId.Equals(SymbolId.String))
            {
                return new StringValue();
            }
            else if (typeId.Equals(SymbolId.Void))
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

            switch(typeId)
            {
                case ModuleSymbolId moduleTypeId:
                    {
                        // 드라이버에서 처리
                        var (driver, path) = GetModuleDriverAndPath(moduleTypeId);
                        return driver.Alloc(path);
                    }

                // int? => Nullable<int>
                // int?? => Nullable<Nullable<int>>
                // C? => C
                // C?? => Nullable<C>
                case NullableSymbolId nullableTypeId:
                    // TODO: 임시: 원래는 runtime 모듈에서 생성해야 한다
                    throw new NotImplementedException();

                case VoidSymbolId:
                    return VoidValue.Instance;
                
                case TupleSymbolId tupleTypeId:
                    {
                        // TODO: 임시 implementation, 추후 TupleValue제거
                        // (int a, C c) => tuple<int, C>로 runtime 모듈에서 생성해야 한다
                        var values = ImmutableArray.CreateBuilder<Value>(tupleTypeId.MemberVarIds.Length);
                        foreach(var memberVarId in tupleTypeId.MemberVarIds)
                        {
                            var value = AllocValue(memberVarId.TypeId);
                            values.Add(value);
                        }

                        return new TupleValue(values.MoveToImmutable());
                    }                                        

                case TypeVarSymbolId:
                case VarSymbolId:
                    throw new NotImplementedException(); // 에러 처리

                default:
                    throw new UnreachableCodeException();
            }
        }

        public void InitializeClassInstance(SymbolId symbolId, ImmutableArray<Value>.Builder builder)
        {
            var (driver, path) = GetModuleDriverAndPath(symbolId);
            driver.InitializeClassInstance(path, builder);
        } 

        // 최종으로 치환된 타입이 들어온다
        public ClassInstance AllocClassInstance(SymbolId classId)
        {   
            var memberVars = ImmutableArray.CreateBuilder<Value>();
            InitializeClassInstance(classId, memberVars);

            return new ClassInstance(classId, memberVars.MoveToImmutable());
        }

        public Value GetStructStaticMemberValue(SymbolId memberVarId)
        {
            var (driver, path) = GetModuleDriverAndPath(memberVarId);
            return driver.GetStructStaticMemberValue(path);
        }

        public Value GetStructMemberValue(StructValue structValue, SymbolId memberVarId)
        {
            var (driver, path) = GetModuleDriverAndPath(memberVarId);
            var index = driver.GetStructMemberVarIndex(path);
            return structValue.GetMemberValue(index);
        }
    }
}
