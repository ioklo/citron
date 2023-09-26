﻿using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Citron.Collections;
using Citron.Infra;
using Citron.Symbol;

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
        public ValueTask ExecuteGlobalFuncAsync(SymbolId globalFuncId, ImmutableArray<Value> args, Value retValue)
        {
            // 심볼만 얻어서는.. Body도 얻어와야 한다
            var globalFuncSymbol = globalContext.LoadSymbol<GlobalFuncSymbol>(globalFuncId);
            var body = globalContext.GetBodyStmt(globalFuncId);

            var builder = ImmutableDictionary.CreateBuilder<Name, Value>();

            for (int i = 0; i < args.Length; i++)
                builder.Add(globalFuncSymbol.GetParameter(i).Name, args[i]);

            var typeContext = TypeContext.Make(globalFuncId.Path);
            var bodyContext = globalContext.NewBodyContext(typeContext, thisValue: null, retValue);
            var localContext = new IR0LocalContext(builder.ToImmutable(), default);

            var context = new IR0EvalContext(globalContext, bodyContext, localContext);
            var stmtEvaluator = new IR0StmtEvaluator(context);

            return stmtEvaluator.EvalBodySkipYieldAsync(body);
        }

        // 이 모듈에 해당하는 타입만 할당한다
        struct TypeAllocator : ITypeSymbolVisitor<Value>
        {
            Evaluator evaluator;
            
            public TypeAllocator(Evaluator evaluator)
            {
                this.evaluator = evaluator;
            }

            Value ITypeSymbolVisitor<Value>.VisitClass(ClassSymbol symbol)
            {
                return new ClassValue();
            }

            Value ITypeSymbolVisitor<Value>.VisitStruct(StructSymbol symbol)
            {
                int varCount = symbol.GetMemberVarCount();
                var values = ImmutableArray.CreateBuilder<Value>(varCount);
                
                for (int i = 0; i < varCount; i++)
                {
                    var memberVar = symbol.GetMemberVar(i);
                    var memberValue = evaluator.AllocValue(memberVar.GetDeclType().GetTypeId());
                    values.Add(memberValue);
                }

                return new StructValue(values.MoveToImmutable());
            }

            Value ITypeSymbolVisitor<Value>.VisitEnum(EnumSymbol symbol)
            {
                var thisEvaluator = evaluator;
                EnumElemValue ElemAllocator(SymbolId symbolId)
                {   
                    Debug.Assert(symbolId.GetOuter().Equals(symbol.GetSymbolId()));
                    Debug.Assert(symbolId.Path != null);

                    var elem = symbol.GetElement(((Name.Normal)symbolId.Path.Name).Text);
                    Debug.Assert(elem != null);

                    var memberVarCount = elem.GetMemberVarCount();
                    var builder = ImmutableArray.CreateBuilder<Value>(memberVarCount);
                    for (int i = 0; i < memberVarCount; i++)
                    {
                        var memberVar = elem.GetMemberVar(i);
                        var memberValue = thisEvaluator.AllocValue(memberVar.GetDeclType().GetTypeId());
                        builder.Add(memberValue);
                    }

                    return new EnumElemValue(builder.MoveToImmutable());
                }

                return new EnumValue(ElemAllocator, null);
            }

            Value ITypeSymbolVisitor<Value>.VisitEnumElem(EnumElemSymbol symbol)
            {
                var memberVarCount = symbol.GetMemberVarCount();
                var builder = ImmutableArray.CreateBuilder<Value>(memberVarCount);

                for(int i = 0; i < memberVarCount; i++)
                {
                    var memberVar = symbol.GetMemberVar(i);
                    var value = evaluator.AllocValue(memberVar.GetDeclType().GetTypeId());
                    builder.Add(value);
                }
                
                return new EnumElemValue(builder.MoveToImmutable());
            }

            Value ITypeSymbolVisitor<Value>.VisitInterface(InterfaceSymbol symbol)
            {
                throw new NotImplementedException();
            }
            
            Value ITypeSymbolVisitor<Value>.VisitLambda(LambdaSymbol symbol)
            {
                var builder = ImmutableDictionary.CreateBuilder<Name, Value>();

                int memberVarCount = symbol.GetMemberVarCount();
                for (int i = 0; i < memberVarCount; i++)
                {
                    var memberVar = symbol.GetMemberVar(i);
                    var memberName = memberVar.GetName();

                    var memberDeclType = memberVar.GetDeclType();
                    var memberValue = evaluator.AllocValue(memberDeclType.GetTypeId());

                    builder.Add(memberName, memberValue);
                }

                return new LambdaValue(builder.ToImmutable());
            }
        }

        // 단일 IR0 모듈 
        public static void Init(ModuleDriverContext driverContext, Evaluator evaluator, IR0GlobalContext globalContext, Name moduleName)
        {
            var driver = new IR0ModuleDriver(evaluator, globalContext);
            driverContext.AddDriver(driver);
            driverContext.AddModule(moduleName, driver);
        }
        
        public Value AllocValue(SymbolId symbolId)
        {            
            var typeSymbol = globalContext.LoadSymbol<ITypeSymbol>(symbolId);

            var allocator = new TypeAllocator(evaluator);
            return typeSymbol.Accept<TypeAllocator, Value>(ref allocator);
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

                var memberValue = evaluator.AllocValue(declType.GetTypeId());
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
            var typeContext = TypeContext.Make(globalFuncId.Path);
            var evalContext = globalContext.NewBodyContext(typeContext, null, retValue);
            var globalFunc = globalContext.LoadSymbol<GlobalFuncSymbol>(globalFuncId);

            var paramCount = globalFunc.GetParameterCount();
            Debug.Assert(paramCount == args.Length);

            var builder = ImmutableDictionary.CreateBuilder<Name, Value>();
            for (int i = 0; i < paramCount; i++)
                builder.Add(globalFunc.GetParameter(i).Name, args[i]);

            var localContext = new IR0LocalContext(builder.ToImmutable(), default);
            var body = globalContext.GetBodyStmt(globalFuncId);

            var context = new IR0EvalContext(globalContext, evalContext, localContext);
            var stmtEvaluator = new IR0StmtEvaluator(context);

            return stmtEvaluator.EvalBodySkipYieldAsync(body);
        }

        ValueTask IModuleDriver.ExecuteClassConstructor(SymbolId constructor, ClassValue thisValue, ImmutableArray<Value> args)
        {
            var typeContext = TypeContext.Make(constructor.Path);

            var evalContext = globalContext.NewBodyContext(typeContext, thisValue, VoidValue.Instance);
            var constructorSymbol = globalContext.LoadSymbol<ClassConstructorSymbol>(constructor);

            var paramCount = constructorSymbol.GetParameterCount();
            Debug.Assert(paramCount == args.Length);

            var builder = ImmutableDictionary.CreateBuilder<Name, Value>();
            for (int i = 0; i < paramCount; i++)
                builder.Add(constructorSymbol.GetParameter(i).Name, args[i]);

            var localContext = new IR0LocalContext(builder.ToImmutable(), default);
            var body = globalContext.GetBodyStmt(constructor);

            var context = new IR0EvalContext(globalContext, evalContext, localContext);
            var stmtEvaluator = new IR0StmtEvaluator(context);

            return stmtEvaluator.EvalBodySkipYieldAsync(body);
        }

        ValueTask IModuleDriver.ExecuteClassMemberFuncAsync(SymbolId memberFunc, Value? thisValue, ImmutableArray<Value> args, Value retValue)
        {
            throw new NotImplementedException();
        }

        ValueTask IModuleDriver.ExecuteStructMemberFuncAsync(SymbolId memberFuncId, Value? thisValue, ImmutableArray<Value> args, Value retValue)
        {
            var typeContext = TypeContext.Make(memberFuncId.Path);

            var evalContext = globalContext.NewBodyContext(typeContext, thisValue, retValue);
            var memberFuncSymbol = globalContext.LoadSymbol<StructMemberFuncSymbol>(memberFuncId);

            var paramCount = memberFuncSymbol.GetParameterCount();
            Debug.Assert(paramCount == args.Length);

            var builder = ImmutableDictionary.CreateBuilder<Name, Value>();
            for (int i = 0; i < paramCount; i++)
                builder.Add(memberFuncSymbol.GetParameter(i).Name, args[i]);

            var localContext = new IR0LocalContext(builder.ToImmutable(), default);
            var body = globalContext.GetBodyStmt(memberFuncId);

            var context = new IR0EvalContext(globalContext, evalContext, localContext);
            var stmtEvaluator = new IR0StmtEvaluator(context);

            return stmtEvaluator.EvalBodySkipYieldAsync(body);
        }

        ValueTask IModuleDriver.ExecuteStructConstructor(SymbolId constructor, LocalPtrValue thisValue, ImmutableArray<Value> args)
        {
            var typeContext = TypeContext.Make(constructor.Path);

            var evalContext = globalContext.NewBodyContext(typeContext, thisValue, VoidValue.Instance);
            var constructorSymbol = globalContext.LoadSymbol<StructConstructorSymbol>(constructor);

            var paramCount = constructorSymbol.GetParameterCount();
            Debug.Assert(paramCount == args.Length);

            var builder = ImmutableDictionary.CreateBuilder<Name, Value>();
            for (int i = 0; i < paramCount; i++)
                builder.Add(constructorSymbol.GetParameter(i).Name, args[i]);

            var localContext = new IR0LocalContext(builder.ToImmutable(), default);
            var body = globalContext.GetBodyStmt(constructor);

            var context = new IR0EvalContext(globalContext, evalContext, localContext);
            var stmtEvaluator = new IR0StmtEvaluator(context);

            return stmtEvaluator.EvalBodySkipYieldAsync(body);
        }

        SymbolId? IModuleDriver.GetBaseClass(SymbolId baseClass)
        {
            var @class = globalContext.LoadSymbol<ClassSymbol>(baseClass);
            return @class.GetBaseClass()?.GetSymbolId();
        }

        int IModuleDriver.GetStructMemberVarIndex(SymbolId memberVarId)
        {
            var memberVar = globalContext.LoadSymbol<StructMemberVarSymbol>(memberVarId);
            Debug.Assert(memberVar != null);

            var @struct = memberVar.GetOuter();

            // base 
            var baseStruct = @struct.GetBaseStruct();
            int baseIndex = 0;
            if (baseStruct != null)
                baseIndex = evaluator.GetTotalStructMemberVarCount(baseStruct.GetSymbolId());

            int memberVarCount = @struct.GetMemberVarCount();
            for (int i = 0; i < memberVarCount; i++)
            {
                var curMemberVar = @struct.GetMemberVar(i);
                if (curMemberVar.Equals(memberVar))
                    return baseIndex + i;
            }

            throw new RuntimeFatalException();
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

        int IModuleDriver.GetTotalStructMemberVarCount(SymbolId structId)
        {
            var structSymbol = globalContext.LoadSymbol<StructSymbol>(structId);
            Debug.Assert(structSymbol != null);

            // base 
            var baseStruct = structSymbol.GetBaseStruct();
            int baseCount = 0;
            if (baseStruct != null)
                baseCount = evaluator.GetTotalStructMemberVarCount(baseStruct.GetSymbolId());

            return baseCount + structSymbol.GetMemberVarCount();
        }
    }
}
