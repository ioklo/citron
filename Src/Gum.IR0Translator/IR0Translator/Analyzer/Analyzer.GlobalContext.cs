using Gum.Infra;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using S = Gum.Syntax;
using M = Gum.CompileTime;
using R = Gum.IR0;
using Gum.Log;
using Gum.Analysis;

using static Gum.Infra.Misc;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        // Analyzer는 backtracking이 없어서, MutableContext를 쓴다 => TODO: 함수 인자 계산할때 backtracking이 생긴다
        class GlobalContext : IMutable<GlobalContext>
        {
            SymbolLoader symbolLoader;
            TypeSymbolInfoService typeSymbolInfoService;

            InternalBinaryOperatorQueryService internalBinOpQueryService;            

            TypeExpInfoService typeExpInfoService;            
            ILogger logger;
            InternalGlobalVariableRepository internalGlobalVarRepo;

            public GlobalContext(SymbolLoader symbolLoader, TypeSymbolInfoService typeSymbolInfoService, ILogger logger)
            {
                this.symbolLoader = symbolLoader;
                this.typeSymbolInfoService = typeSymbolInfoService;

                this.internalBinOpQueryService = new InternalBinaryOperatorQueryService(GetBoolType(), GetIntType(), GetStringType());
                
                this.logger = logger;
                this.internalGlobalVarRepo = new InternalGlobalVariableRepository();
            }

            // typeParams를 치환하지 않고 그대로 만든다
            public TSymbol? LoadOpenSymbol<TSymbol>(ModuleSymbolId outerId, string name, ImmutableArray<string> typeParams, ImmutableArray<FuncParamId> paramIds)
                where TSymbol : class, ISymbolNode
            {
                // typeVarId만들어야 함
                var outerDeclId = outerId.GetDeclSymbolId();

                var typeArgIdsBuilder = ImmutableArray.CreateBuilder<SymbolId>();
                foreach (var typeParam in typeParams)
                {
                    var typeVarDeclId = outerDeclId.Child(new M.Name.Normal(typeParam), 0, default);
                    var typeVarId = new TypeVarSymbolId(typeVarDeclId);
                    typeArgIdsBuilder.Add(typeVarId);
                }

                return symbolLoader.Load(outerId.Child(new M.Name.Normal(name), typeArgIdsBuilder.ToImmutable(), paramIds)) as TSymbol;
            }

            GlobalContext(
                GlobalContext other,
                CloneContext cloneContext)
            {
                this.symbolLoader = other.symbolLoader;
                this.internalBinOpQueryService = other.internalBinOpQueryService;
                this.typeExpInfoService = other.typeExpInfoService;

                this.logger = cloneContext.GetClone(other.logger);
                this.internalGlobalVarRepo = cloneContext.GetClone(other.internalGlobalVarRepo);
            }

            public GlobalContext Clone(CloneContext cloneContext)
            {
                return new GlobalContext(this, cloneContext);
            }

            public void Update(GlobalContext src, UpdateContext updateContext)
            {
                // this.symbolLoader = src.symbolLoader; // 업데이트 대상이 아니다
                this.internalBinOpQueryService = src.internalBinOpQueryService;
                this.typeExpInfoService = src.typeExpInfoService;

                updateContext.Update(this.logger, src.logger);
                updateContext.Update(this.internalGlobalVarRepo, src.internalGlobalVarRepo);
            }

            public void AddError(AnalyzeErrorCode code, S.ISyntaxNode node)
            {
                logger.Add(new AnalyzeErrorLog(code, node, code.ToString()));
            }

            [DoesNotReturn]
            public void AddFatalError(AnalyzeErrorCode code, S.ISyntaxNode node)
            {
                logger.Add(new AnalyzeErrorLog(code, node, code.ToString()));
                throw new AnalyzerFatalException();
            }

            static VoidSymbolId voidId = new VoidSymbolId();
            public ITypeSymbol GetVoidType()
            {
                return (ITypeSymbol)symbolLoader.Load(new VoidSymbolId());
            }

            static ModuleSymbolId boolId = new ModuleSymbolId(new M.Name.Normal("System.Runtime"), null).Child(new M.Name.Normal("System")).Child(new M.Name.Normal("Boolean"));
            public ITypeSymbol GetBoolType()
            {   
                return (ITypeSymbol)symbolLoader.Load(boolId);                    
            }

            static ModuleSymbolId intId = new ModuleSymbolId(new M.Name.Normal("System.Runtime"), null).Child(new M.Name.Normal("System")).Child(new M.Name.Normal("Int32"));
            public ITypeSymbol GetIntType()
            {
                return (ITypeSymbol)symbolLoader.Load(boolId);
            }

            static ModuleSymbolId stringId = new ModuleSymbolId(new M.Name.Normal("System.Runtime"), null).Child(new M.Name.Normal("System")).Child(new M.Name.Normal("String"));
            public ITypeSymbol GetStringType()
            {
                return (ITypeSymbol)symbolLoader.Load(stringId);
            }
            
            public ITypeSymbol GetListType(ITypeSymbol elemType)
            {
                var typeArgs = Arr(elemType.GetSymbolId());
                var listId = new ModuleSymbolId(new M.Name.Normal("System.Runtime"), null).Child(new M.Name.Normal("System")).Child(new M.Name.Normal("List"), typeArgs);

                return (ITypeSymbol)symbolLoader.Load(listId);
            }
            
            public void AddInternalGlobalVarInfo(bool bRef, ITypeSymbol typeValue, string name)
            {
                internalGlobalVarRepo.AddInternalGlobalVariable(bRef, typeValue, name);
            }

            public bool IsBoolType(ITypeSymbol type)
            {
                var decl = type.GetDeclSymbolNode();
                var declId = decl.GetDeclSymbolId();

                return declId.Equals(boolId);
            }

            public bool IsIntType(ITypeSymbol type)
            {
                var decl = type.GetDeclSymbolNode();
                var declId = decl.GetDeclSymbolId();

                return declId.Equals(intId);
            }            

            public bool IsStringType(ITypeSymbol type)
            {
                var decl = type.GetDeclSymbolNode();
                var declId = decl.GetDeclSymbolId();

                return declId.Equals(stringId);
            }
            
            public ITypeSymbol GetSymbolByTypeExp(S.TypeExp typeExp)
            {
                var symbol = typeSymbolInfoService.GetSymbol(typeExp);
                Debug.Assert(symbol != null);
                return symbol;
            }

            public InternalGlobalVarInfo? GetInternalGlobalVarInfo(string idName)
            {
                return internalGlobalVarRepo.GetVariable(idName);
            }

            public bool DoesInternalGlobalVarNameExist(string name)
            {
                return internalGlobalVarRepo.HasVariable(name);
            }

            // 람다는 모듈 레퍼런스에 존재하지 않은
            public LambdaSymbol MakeLambdaSymbol(IFuncSymbol outer, ITypeSymbol retType, ImmutableArray<ParamInfo> paramInfos)
            {
                return symbolLoader.Load(.MakeLambdaType(lambda, retType, paramInfos);
            }

            public SeqTypeValue GetSeqTypeValue(R.Path.Nested seq, ITypeSymbol yieldType)
            {
                return itemValueFactory.MakeSeqType(seq, yieldType);
            }

            // outerDeclPath 밑의 (name, typeParamCount)로 가능한 것들을 돌려준다
            public SymbolQueryResult QuerySymbol(SymbolPath? outerPath, M.Name name, int typeParamCount)
            {
                return symbolLoader.Query(outerPath, name, typeParamCount);
            } 

            public ImmutableArray<InternalBinaryOperatorInfo> GetBinaryOpInfos(S.BinaryOpKind kind)
            {
                return internalBinOpQueryService.GetInfos(kind);
            }

            public ITypeSymbol MakeTypeValue(ItemValueOuter outer, ITypeDeclSymbol typeInfo, ImmutableArray<ITypeSymbol> typeArgs)
            {
                return itemValueFactory.MakeTypeValue(outer, typeInfo, typeArgs);
            }

            public FuncValue MakeFuncValue(ItemValueOuter outer, IModuleFuncDecl funcInfo, ImmutableArray<ITypeSymbol> typeArgs)
            {
                return itemValueFactory.MakeFunc(outer, funcInfo, typeArgs);
            }

            public ConstructorValue MakeConstructorValue(NormalTypeValue outer, IModuleConstructorDecl info)
            {
                return itemValueFactory.MakeConstructorValue(outer, info);
            }

            public MemberVarValue MakeMemberVarValue(NormalTypeValue outer, IModuleMemberVarInfo info)
            {
                return itemValueFactory.MakeMemberVarValue(outer, info);
            }

            public TupleTypeValue GetTupleType(ImmutableArray<(ITypeSymbol Type, string? Name)> elems)
            {
                return symbolLoader.Load(.MakeTupleType(elems);
            }

            public R.Exp? TryCastExp_Exp(ExpResult.Exp expResult, ITypeSymbol expectedType) // nothrow
            {
                // 같으면 그대로 리턴
                if (expResult.TypeSymbol.Equals(expectedType))
                    return expResult;

                // TODO: TypeValue에 TryCast를 각각 넣기
                // expectType.TryCast(expResult); // expResult를 넣는것도 이상하다.. 그건 그때가서

                // 1. enumElem -> enum
                if (expResult.TypeSymbol is EnumElemSymbol enumElem)
                {
                    if (expectedType is EnumSymbol expectEnumType)
                    {
                        if (expectedType.Equals(enumElem.GetOuter()))
                        {
                            return new R.CastEnumElemToEnumExp(expResult.Result, enumElem);
                        }
                    }

                    return null;
                }

                // 2. exp is class type
                if (expResult.TypeSymbol is ClassSymbol @class)
                {
                    if (expectedType is ClassSymbol expectedClass)
                    {
                        // allows upcast
                        if (expectedClass.IsBaseOf(@class))
                        {
                            return new R.CastClassExp(expResult.Result, expectedClass);
                        }

                        return null;
                    }

                    // TODO: interface
                    // if (expectType is InterfaceTypeValue )
                }

                // 3. T -> T? 는 허용
                if (expectedType is NullableSymbol expectedNullableType)
                {
                    // C -> B -> B? 허용
                    var expectInnerType = expectedNullableType.GetInnerTypeValue();

                    // C -> B 시도
                    var castToInnerTypeResult = TryCastExp_Exp(expResult, expectInnerType);
                    if (castToInnerTypeResult != null)
                    {
                        // B -> B?
                        var castExp = new R.NewNullableExp(expectInnerType.MakeRPath(), castToInnerTypeResult.Result);
                        return new ExpResult.Exp(castExp, expectedType);
                    }
                }

                return null;
            }            

            public StructSymbol MakeStructTypeValue(ItemValueOuter outer, IModuleStructDecl structInfo, ImmutableArray<ITypeSymbol> typeArgs)
            {
                return itemValueFactory.MakeStructValue(outer, structInfo, typeArgs);
            }

            public ClassSymbol MakeClassTypeValue(ItemValueOuter outer, IModuleClassDecl classInfo, ImmutableArray<ITypeSymbol> typeArgs)
            {
                return itemValueFactory.MakeClassSymbol(outer, classInfo, typeArgs);
            }
        }
    }
}
