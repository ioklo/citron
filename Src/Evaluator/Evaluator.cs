using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Citron.CompileTime;

using Citron.Collections;
using Citron.Infra;
using System.Runtime.InteropServices;

namespace Citron
{
    public class EvalThread
    {
        Evaluator evaluator;

        // stack정보 => increasing
        // | ... | v | ... | args ... | savedBase | savedRet | locals ... | temps ... |
        //      ret                  base                                            cur
        //const int STACK_SIZE = 1024 * 1024;

        //unsafe void* stackPtr;
        //unsafe void* basePtr;
        //unsafe void* retPtr;
        //unsafe byte* curPtr; // increase 하기 위해서 byte

        public EvalThread()
        {   
            //var stack = Marshal.AllocHGlobal(STACK_SIZE);
            //GC.AddMemoryPressure(STACK_SIZE);

            //unsafe
            //{
            //    byte* stackPtr = (byte*)stack.ToPointer();

            //    // -------------------------------
            //    // |retPtr|prevBasePtr|prevRetPtr|
            //    // -------------------------------      
            //    // stackPtr
            //    //        basePtr                curPtr
            //    int* retPtr = (int*)stackPtr; // int 할당
            //    void** prevBasePtr = (void**)(stackPtr + sizeof(int));
            //    void** prevRetPtr = (void**)(stackPtr + sizeof(int) + sizeof(byte*));

            //    basePtr = stackPtr + sizeof(int);
            //    curPtr = stackPtr + sizeof(int) + sizeof(byte*) * 2;

            //    *prevBasePtr = null;
            //    *prevRetPtr = null;
            //}
        }

        public TValue StackAlloc<TValue>(SymbolId typeId)
            where TValue : Value
        {
            return evaluator.AllocValue<TValue>(typeId);

            //unsafe
            //{
            //    byte* result = curPtr;
            //    curPtr += size;
            //    return (IntPtr)result;
            //}
        }

        public void NewFrame(Value newRetValue)
        {
            throw new NotImplementedException();

            //// base ret cur
            //unsafe
            //{
            //    void** savedBasePtr = (void**)curPtr;
            //    void** savedRetPtr = (void**)(curPtr + sizeof(void*));

            //    // savedBase, savedRet
            //    *savedBasePtr = basePtr;
            //    *savedRetPtr = retPtr;

            //    basePtr = curPtr;
            //    curPtr += sizeof(void*) * 2; // savedBase, savedRet
            //    retPtr = newRetPtr.ToPointer();
            //}
        }

        // 언제 사라질지 정하지 못한다
        //~EvalThread()
        //{
        //    unsafe
        //    {
        //        Marshal.FreeHGlobal(new IntPtr(stackPtr));
        //        GC.RemoveMemoryPressure(STACK_SIZE);
        //    }
        //}
    }

    // 겉 껍데기
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
        
        // 다 속도가 느린 버전
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
            else if (typeId.IsList())
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
    }
}
