using Citron.Analysis;
using Citron.Collections;
using Citron.CompileTime;
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
    class IR0ModuleDriver : IModuleDriver
    {
        Evaluator evaluator;            // 외부 오퍼레이션
        IR0GlobalContext globalContext; // 내부 오퍼레이션 

        public IR0ModuleDriver(Evaluator evaluator, IR0GlobalContext globalContext)
        {
            this.evaluator = evaluator;
            this.globalContext = globalContext;
        }

        // globalFuncId는 이미 Apply된 상태
        public ValueTask ExecuteGlobalFuncAsync(SymbolPath globalFuncPath, ImmutableArray<Value> args, Value retValue)
        {
            // 심볼만 얻어서는.. Body도 얻어와야 한다
            var globalFuncSymbol = globalContext.LoadSymbol<GlobalFuncSymbol>(globalFuncPath);
            var body = globalContext.GetBodyStmt(globalFuncPath);

            var builder = ImmutableDictionary.CreateBuilder<Name, Value>();

            for (int i = 0; i < args.Length; i++)
                builder.Add(globalFuncSymbol.GetParameter(i).Name, args[i]);
            
            var evalContext = globalContext.NewEvalContext(globalFuncPath, thisValue: null, retValue);
            var localContext = new IR0LocalContext(builder.ToImmutable(), default);

            var evaluator = new IR0Evaluator(globalContext, evalContext, localContext);
            return evaluator.EvalStmtSkipYieldAsync(body);
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
                return new EnumValue(null, null);
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

            public void VisitLambda(LambdaSymbol symbol)
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
        }        

        public Value AllocValue(SymbolPath typePath)
        {            
            var typeSymbol = globalContext.LoadSymbol<ITypeSymbol>(typePath);

            var allocator = new TypeAllocator(evaluator);
            typeSymbol.Apply(allocator);
            return allocator.result;
        }

        public void InitializeClassInstance(SymbolPath path, ImmutableArray<Value>.Builder builder)
        {
            var classSymbol = globalContext.LoadSymbol<ClassSymbol>(path);

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

        public int GetTotalClassMemberVarCount(SymbolPath classPath)
        {
            var classSymbol = globalContext.LoadSymbol<ClassSymbol>(classPath);
            Debug.Assert(classSymbol != null);

            // base 
            var baseClass = classSymbol.GetBaseClass();
            int baseCount = 0;
            if (baseClass != null)
                baseCount = evaluator.GetTotalClassMemberVarCount(baseClass.GetSymbolId());

            return baseCount + classSymbol.GetMemberVarCount();
        }

        public int GetClassMemberVarIndex(SymbolPath memberVarPath)
        {
            var memberVar = globalContext.LoadSymbol<ClassMemberVarSymbol>(memberVarPath);
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

        Value IModuleDriver.Alloc(SymbolPath typePath)
        {
            throw new NotImplementedException();
        }

        ValueTask IModuleDriver.ExecuteGlobalFuncAsync(SymbolPath globalFuncPath, ImmutableArray<Value> args, Value retValue)
        {
            throw new NotImplementedException();
        }

        ValueTask IModuleDriver.ExecuteClassConstructor(SymbolPath constructorPath, ClassValue thisValue, ImmutableArray<Value> args)
        {
            var evalContext = globalContext.NewEvalContext(constructorPath, thisValue, VoidValue.Instance);
            var constructorSymbol = globalContext.LoadSymbol<ClassConstructorSymbol>(constructorPath);

            var paramCount = constructorSymbol.GetParameterCount();
            Debug.Assert(paramCount == args.Length);

            var builder = ImmutableDictionary.CreateBuilder<Name, Value>();
            for (int i = 0; i < paramCount; i++)
                builder.Add(constructorSymbol.GetParameter(i).Name, args[i]);

            var localContext = new IR0LocalContext(builder.ToImmutable(), default);
            var body = globalContext.GetBodyStmt(constructorPath);

            var evaluator = new IR0Evaluator(globalContext, evalContext, localContext);
            return evaluator.EvalStmtSkipYieldAsync(body);
        }

        ValueTask IModuleDriver.ExecuteClassMemberFuncAsync(SymbolPath memberFuncPath, Value? thisValue, ImmutableArray<Value> args, Value retValue)
        {
            throw new NotImplementedException();
        }

        ValueTask IModuleDriver.ExecuteStructMemberFuncAsync(SymbolPath memberFuncPath, Value? thisValue, ImmutableArray<Value> args, Value retValue)
        {
            throw new NotImplementedException();
        }

        ValueTask IModuleDriver.ExecuteStructConstructor(SymbolPath constructorPath, StructValue thisValue, ImmutableArray<Value> args)
        {
            throw new NotImplementedException();
        }

        SymbolId? IModuleDriver.GetBaseClass(SymbolPath classPath)
        {
            var @class = globalContext.LoadSymbol<ClassSymbol>(classPath);
            return @class.GetBaseClass()?.GetSymbolId();
        }

        int IModuleDriver.GetStructMemberVarIndex(SymbolPath memberPath)
        {
            throw new NotImplementedException();
        }

        public Value GetClassStaticMemberValue(SymbolPath path)
        {
            throw new NotImplementedException();
        }

        public Value GetStructStaticMemberValue(SymbolPath path)
        {
            throw new NotImplementedException();
        }
    }
}
