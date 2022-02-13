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

namespace Citron.IR0Evaluator
{
    // 컴파일 중간 결과물을 모듈로 만들어서 테스트 용도로 쓰는 드라이버..
    class IR0ModuleDriver : IModuleDriver
    {        
        // 이 이름의 모듈 하나만을 포함한다
        Name moduleName;

        Evaluator evaluator; // 상위 evaluator, 외부 호출을 할 때 사용한다
        IR0GlobalContext globalContext;
        SymbolLoader symbolLoader; // symbolId => symbol
        StmtLoader stmtLoader;     // symbolId => stmt

        public IR0ModuleDriver(Name moduleName, Evaluator evaluator, IR0GlobalContext globalContext, SymbolLoader symbolLoader, StmtLoader stmtLoader)
        {
            this.moduleName = moduleName;
            this.evaluator = evaluator;
            this.globalContext = globalContext;
            this.symbolLoader = symbolLoader;
            this.stmtLoader = stmtLoader;
        }

        // globalFuncId는 이미 Apply된 상태
        public ValueTask ExecuteGlobalFuncAsync(ModuleSymbolId globalFuncId, ImmutableArray<Value> args, Value retValue)
        {
            Debug.Assert(moduleName.Equals(globalFuncId.ModuleName));

            // 심볼만 얻어서는.. Body도 얻어와야 한다
            var globalFuncSymbol = symbolLoader.Load(globalFuncId);
            var body = stmtLoader.Load(globalFuncId);

            var builder = ImmutableDictionary.CreateBuilder<Name, Value>();

            for (int i = 0; i < args.Length; i++)
                builder.Add(globalFuncSymbol.Parameters[i].Name, args[i]);
            
            var funcTypeContext = TypeContext.Make(globalFuncId);

            var context = new IR0EvalContext(evaluator, funcTypeContext, EvalFlowControl.None, null, retValue);
            var localContext = new LocalContext(builder.ToImmutable());
            var localTaskContext = new LocalTaskContext();

            return IR0StmtEvaluator.EvalAsync(globalContext, context, localContext, localTaskContext, body);
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
                var values = ImmutableDictionary.CreateBuilder<Name, Value>();

                int varCount = structSymbol.GetMemberVarCount();
                for (int i = 0; i < varCount; i++)
                {
                    var memberVar = structSymbol.GetMemberVar(i);
                    var memberValue = evaluator.AllocValue(memberVar.GetDeclType().GetSymbolId());
                    values.Add(memberVar.GetName(), memberValue);
                }

                result = new StructValue(values.ToImmutable());
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

        public Value AllocValue(ModuleSymbolId id)
        {            
            var typeSymbol = symbolLoader.Load(id) as ITypeSymbol;
            if (typeSymbol == null)
                throw new UnreachableCodeException();

            var allocator = new TypeAllocator(evaluator);
            typeSymbol.Apply(allocator);
            return allocator.result;
        }

        public void InitializeClassInstance(SymbolPath path, ImmutableArray<Value>.Builder builder)
        {
            var classSymbol = symbolLoader.Load(new ModuleSymbolId(moduleName, path)) as ClassSymbol;
            Debug.Assert(classSymbol != null);

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
            var classSymbol = symbolLoader.Load(new ModuleSymbolId(moduleName, classPath)) as ClassSymbol;
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
            var memberVar = symbolLoader.Load(new ModuleSymbolId(moduleName, memberVarPath)) as ClassMemberVarSymbol;
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

        void IModuleDriver.ExecuteClassConstructor(SymbolPath constructorPath, ClassValue thisValue, ImmutableArray<Value> args)
        {
            throw new NotImplementedException();
        }

        ValueTask IModuleDriver.ExecuteClassMemberFuncAsync(SymbolPath memberFuncPath, Value? thisValue, ImmutableArray<Value> args, Value retValue)
        {
            throw new NotImplementedException();
        }

        ValueTask IModuleDriver.ExecuteStructMemberFuncAsync(SymbolPath memberFuncPath, Value? thisValue, ImmutableArray<Value> args, Value retValue)
        {
            throw new NotImplementedException();
        }

        void IModuleDriver.ExecuteStructConstructor(SymbolPath constructorPath, StructValue thisValue, ImmutableArray<Value> args)
        {
            throw new NotImplementedException();
        }
    }
}
