using Citron.Analysis;
using Citron.Collections;
using Citron.Module;
using Citron.Infra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron
{
    // 컴파일 중간 결과물을 모듈로 만들어서 테스트 용도로 쓰는 드라이버..
    public class IR0ModuleDriver : IModuleDriver
    {
        Evaluator evaluator;            // 외부 오퍼레이션
        IR0GlobalContext globalContext; // 내부 오퍼레이션 

        public IR0ModuleDriver(Evaluator evaluator, IR0GlobalContext globalContext)
        {
            this.evaluator = evaluator;
            this.globalContext = globalContext;
        }

        // globalFuncId는 이미 Apply된 상태
        public ValueTask ExecuteGlobalFuncAsync(SymbolId globalFunc, ImmutableArray<Value> args, Value retValue)
        {
            // 심볼만 얻어서는.. Body도 얻어와야 한다
            var globalFuncSymbol = globalContext.LoadSymbol<GlobalFuncSymbol>(globalFunc);
            var body = globalContext.GetBodyStmt(globalFunc);

            var builder = ImmutableDictionary.CreateBuilder<Name, Value>();

            for (int i = 0; i < args.Length; i++)
                builder.Add(globalFuncSymbol.GetParameter(i).Name, args[i]);

            var typeContext = TypeContext.Make(globalFunc);
            var evalContext = globalContext.NewEvalContext(typeContext, thisValue: null, retValue);
            var localContext = new IR0LocalContext(builder.ToImmutable(), default);

            var evaluator = new IR0Evaluator(globalContext, evalContext, localContext);
            return evaluator.EvalBodySkipYieldAsync(body);
        }

        class TypeAllocator : ITypeSymbolVisitor
        {
            Evaluator evaluator;            

            [AllowNull]
            internal Value result;

            public TypeAllocator(Evaluator evaluator)
            {
                this.evaluator = evaluator;
            }

            public void VisitClass(ClassSymbol symbol)
            {
                result = new ClassValue();
            }

            public void VisitStruct(StructSymbol structSymbol)
            {
                int varCount = structSymbol.GetMemberVarCount();
                var values = ImmutableArray.CreateBuilder<Value>(varCount);
                
                for (int i = 0; i < varCount; i++)
                {
                    var memberVar = structSymbol.GetMemberVar(i);
                    var memberValue = evaluator.AllocValue(memberVar.GetDeclType().GetSymbolId());
                    values.Add(memberValue);
                }

                result = new StructValue(values.MoveToImmutable());
            }

            public void VisitEnum(EnumSymbol symbol)
            {
                EnumElemValue ElemAllocator(SymbolId id_symbol)
                {
                    var id = id_symbol as ModuleSymbolId;
                    Debug.Assert(id != null);
                    Debug.Assert(id.GetOuter().Equals(symbol.GetSymbolId()));
                    Debug.Assert(id.Path != null);

                    var elem = symbol.GetElement(((Name.Normal)id.Path.Name).Text);
                    Debug.Assert(elem != null);

                    var memberVarCount = elem.GetMemberVarCount();
                    var builder = ImmutableArray.CreateBuilder<Value>(memberVarCount);
                    for (int i = 0; i < memberVarCount; i++)
                    {
                        var memberVar = elem.GetMemberVar(i);
                        var memberValue = evaluator.AllocValue(memberVar.GetDeclType().GetSymbolId());
                        builder.Add(memberValue);
                    }

                    return new EnumElemValue(builder.MoveToImmutable());
                }

                result = new EnumValue(ElemAllocator, null);
            }

            public void VisitEnumElem(EnumElemSymbol symbol)
            {
                var memberVarCount = symbol.GetMemberVarCount();
                var builder = ImmutableArray.CreateBuilder<Value>(memberVarCount);

                for(int i = 0; i < memberVarCount; i++)
                {
                    var memberVar = symbol.GetMemberVar(i);
                    var value = evaluator.AllocValue(memberVar.GetDeclType().GetSymbolId());
                    builder.Add(value);
                }
                
                result = new EnumElemValue(builder.MoveToImmutable());
            }

            public void VisitInterface(InterfaceSymbol symbol)
            {
                throw new NotImplementedException();
            }

            // 최상위 타입들은 여기서 allocation하지 않는다.
            public void VisitNullable(NullableSymbol symbol)
            {
                throw new UnreachableCodeException();
            }

            public void VisitTuple(TupleSymbol symbol)
            {
                throw new UnreachableCodeException();
            }

            public void VisitVoid(VoidSymbol symbol)
            {
                throw new UnreachableCodeException();
            }

            public void VisitLambda(LambdaSymbol lambdaSymbol)
            {
                var builder = ImmutableDictionary.CreateBuilder<Name, Value>();

                int memberVarCount = lambdaSymbol.GetMemberVarCount();
                for (int i = 0; i < memberVarCount; i++)
                {
                    var memberVar = lambdaSymbol.GetMemberVar(i);
                    var memberName = memberVar.GetName();

                    var memberDeclType = memberVar.GetDeclType();
                    var memberValue = evaluator.AllocValue(memberDeclType.GetSymbolId());

                    builder.Add(memberName, memberValue);
                    result = new LambdaValue(builder.ToImmutable());
                }
            }
        }

        // 단일 IR0 모듈 
        public static void Init(ModuleDriverContext driverContext, Evaluator evaluator, IR0GlobalContext globalContext, Name moduleName)
        {
            var driver = new IR0ModuleDriver(evaluator, globalContext);
            driverContext.AddDriver(driver);
            driverContext.AddModule(moduleName, driver);
        }

        public Value AllocValue(SymbolId typeId)
        {            
            var typeSymbol = globalContext.LoadSymbol<ITypeSymbol>(typeId);

            var allocator = new TypeAllocator(evaluator);
            typeSymbol.Apply(allocator);
            return allocator.result;
        }

        public void InitializeClassInstance(SymbolId classId, ImmutableArray<Value>.Builder builder)
        {
            var classSymbol = globalContext.LoadSymbol<ClassSymbol>(classId);

            // base 
            var baseClass = classSymbol.GetBaseClass();
            if (baseClass != null)
                evaluator.InitializeClassInstance(baseClass.GetSymbolId(), builder);

            // 멤버 개수만큼
            var memberVarCount = classSymbol.GetMemberVarCount();
            for (int i = 0; i < memberVarCount; i++)
            {
                var memberVar = classSymbol.GetMemberVar(i);
                var declType = memberVar.GetDeclType();

                var memberValue = evaluator.AllocValue(declType.GetSymbolId());
                builder.Add(memberValue);
            }
        }

        public int GetTotalClassMemberVarCount(SymbolId classId)
        {
            var classSymbol = globalContext.LoadSymbol<ClassSymbol>(classId);
            Debug.Assert(classSymbol != null);

            // base 
            var baseClass = classSymbol.GetBaseClass();
            int baseCount = 0;
            if (baseClass != null)
                baseCount = evaluator.GetTotalClassMemberVarCount(baseClass.GetSymbolId());

            return baseCount + classSymbol.GetMemberVarCount();
        }

        public int GetClassMemberVarIndex(SymbolId memberVarId)
        {
            var memberVar = globalContext.LoadSymbol<ClassMemberVarSymbol>(memberVarId);
            Debug.Assert(memberVar != null);

            var @class = memberVar.GetOuter();

            // base 
            var baseClass = @class.GetBaseClass();
            int baseIndex = 0;
            if (baseClass != null)
                baseIndex = evaluator.GetTotalClassMemberVarCount(baseClass.GetSymbolId());

            int memberVarCount = @class.GetMemberVarCount();
            for (int i = 0; i < memberVarCount; i++)
            {
                var curMemberVar = @class.GetMemberVar(i);
                if (curMemberVar.Equals(memberVar))
                    return baseIndex + i;
            }

            throw new RuntimeFatalException();
        }

        Value IModuleDriver.Alloc(SymbolId id) => AllocValue(id);

        ValueTask IModuleDriver.ExecuteGlobalFuncAsync(SymbolId globalFuncId, ImmutableArray<Value> args, Value retValue)
        {
            var typeContext = TypeContext.Make(globalFuncId);
            var evalContext = globalContext.NewEvalContext(typeContext, null, retValue);
            var globalFunc = globalContext.LoadSymbol<GlobalFuncSymbol>(globalFuncId);

            var paramCount = globalFunc.GetParameterCount();
            Debug.Assert(paramCount == args.Length);

            var builder = ImmutableDictionary.CreateBuilder<Name, Value>();
            for (int i = 0; i < paramCount; i++)
                builder.Add(globalFunc.GetParameter(i).Name, args[i]);

            var localContext = new IR0LocalContext(builder.ToImmutable(), default);
            var body = globalContext.GetBodyStmt(globalFuncId);

            var evaluator = new IR0Evaluator(globalContext, evalContext, localContext);
            return evaluator.EvalBodySkipYieldAsync(body);
        }

        ValueTask IModuleDriver.ExecuteClassConstructor(SymbolId constructor, ClassValue thisValue, ImmutableArray<Value> args)
        {
            var typeContext = TypeContext.Make(constructor);

            var evalContext = globalContext.NewEvalContext(typeContext, thisValue, VoidValue.Instance);
            var constructorSymbol = globalContext.LoadSymbol<ClassConstructorSymbol>(constructor);

            var paramCount = constructorSymbol.GetParameterCount();
            Debug.Assert(paramCount == args.Length);

            var builder = ImmutableDictionary.CreateBuilder<Name, Value>();
            for (int i = 0; i < paramCount; i++)
                builder.Add(constructorSymbol.GetParameter(i).Name, args[i]);

            var localContext = new IR0LocalContext(builder.ToImmutable(), default);
            var body = globalContext.GetBodyStmt(constructor);

            var evaluator = new IR0Evaluator(globalContext, evalContext, localContext);
            return evaluator.EvalBodySkipYieldAsync(body);
        }

        ValueTask IModuleDriver.ExecuteClassMemberFuncAsync(SymbolId memberFunc, Value? thisValue, ImmutableArray<Value> args, Value retValue)
        {
            throw new NotImplementedException();
        }

        ValueTask IModuleDriver.ExecuteStructMemberFuncAsync(SymbolId memberFunc, Value? thisValue, ImmutableArray<Value> args, Value retValue)
        {
            throw new NotImplementedException();
        }

        ValueTask IModuleDriver.ExecuteStructConstructor(SymbolId constructor, StructValue thisValue, ImmutableArray<Value> args)
        {
            throw new NotImplementedException();
        }

        SymbolId? IModuleDriver.GetBaseClass(SymbolId baseClass)
        {
            var @class = globalContext.LoadSymbol<ClassSymbol>(baseClass);
            return @class.GetBaseClass()?.GetSymbolId();
        }

        int IModuleDriver.GetStructMemberVarIndex(SymbolId memberVar)
        {
            throw new NotImplementedException();
        }

        Value IModuleDriver.GetClassStaticMemberValue(SymbolId memberVarId)
        {
            throw new NotImplementedException();
        }

        Value IModuleDriver.GetStructStaticMemberValue(SymbolId memberVarId)
        {
            throw new NotImplementedException();
        }

        int IModuleDriver.GetEnumElemMemberVarIndex(SymbolId memberVarId)
        {
            throw new NotImplementedException();
        }
    }
}
