using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using Citron.Module;
using Citron.Log;
using Citron.Symbol;
using Citron.Collections;
using Citron.Infra;

using S = Citron.Syntax;
using R = Citron.IR0;

using static Citron.Infra.Misc;

namespace Citron.Analysis
{    
    // Analyzer는 backtracking이 없어서, MutableContext를 쓴다 => TODO: 함수 인자 계산할때 backtracking이 생긴다
    class GlobalContext : IMutable<GlobalContext>
    {
        SymbolLoader symbolLoader;
        TypeSymbolInfoService typeInfoService;
        SymbolFactory symbolFactory;

        InternalBinaryOperatorQueryService internalBinOpQueryService;
        
        ILogger logger;
        InternalGlobalVariableRepository internalGlobalVarRepo;

        static VoidSymbolId voidId;
        static ModuleSymbolId boolId, intId, stringId;
        static DeclSymbolId boolDeclId, intDeclId, stringDeclId;

        static DeclSymbolId nullableDeclId, listDeclId;

        static GlobalContext()
        {
            voidId = new VoidTypeId();

            boolDeclId = new DeclSymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System")).Child(new Name.Normal("Boolean"));
            boolId = new SymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System")).Child(new Name.Normal("Boolean"));

            intDeclId = new DeclSymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System")).Child(new Name.Normal("Int32"));
            intId = new SymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System")).Child(new Name.Normal("Int32"));

            stringDeclId = new DeclSymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System")).Child(new Name.Normal("String"));
            stringId = new SymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System")).Child(new Name.Normal("String"));

            listDeclId = new DeclSymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System")).Child(new Name.Normal("List"), 1);
            nullableDeclId = new DeclSymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System")).Child(new Name.Normal("Nullable"), 1);
        }

        public GlobalContext(SymbolLoader symbolLoader, TypeSymbolInfoService typeSymbolInfoService, SymbolFactory symbolFactory, ILogger logger)
        {
            this.symbolLoader = symbolLoader;
            this.typeInfoService = typeSymbolInfoService;
            this.symbolFactory = symbolFactory;

            this.internalBinOpQueryService = new InternalBinaryOperatorQueryService(GetBoolType(), GetIntType(), GetStringType());
                
            this.logger = logger;
            this.internalGlobalVarRepo = new InternalGlobalVariableRepository();
        }

        // typeParams를 치환하지 않고 그대로 만든다
        public TSymbol? LoadOpenSymbol<TSymbol>(ModuleSymbolId outerId, string name, ImmutableArray<string> typeParams, ImmutableArray<FuncParamId> paramIds)
            where TSymbol : class, ISymbolNode
        {
            // class C<T, U> { C<T, U> c; }
            // C<T, U>.T에서 T를 만들려면 C<T, U>정의가 필요하고, C<T, U>를 만들려면 T의 정의가 필요하다
            // 먼저, T, U를 outer 없이 만들고, C<T, U>를 만든 다음, outer에 채워넣는다

            // 1. typeArgIds만들기
            var typeVarPaths = new List<SymbolPath>(); // 나중에 outer를 넣기 위해 저장
            var typeArgIdsBuilder = ImmutableArray.CreateBuilder<SymbolId>();
            for(int i = 0; i < typeParams.Length; i++)
            {
                // outer를 일단 null로 지정한다
                var typeVarPath = new SymbolPath(null, new Name.Normal(typeParams[i]));
                var typeVarId = new ModuleSymbolId(outerId.ModuleName, typeVarPath);
                typeArgIdsBuilder.Add(typeVarId);
                typeVarPaths.Add(typeVarPath);
            }
            var typeArgIds = typeArgIdsBuilder.ToImmutable();

            // 2. C<T, U>만들기
            var symbolId = outerId.Child(new Name.Normal(name), typeArgIds, paramIds);

            // 3. 다시 채워넣기, NOTICE: typeVarPath가 ref type이라서 가능하다
            foreach(var typeVarPath in typeVarPaths)
                typeVarPath.Outer = symbolId.Path;

            return symbolLoader.Load(symbolId) as TSymbol;
        }

        GlobalContext(
            GlobalContext other,
            CloneContext cloneContext)
        {
            this.symbolLoader = other.symbolLoader;
            this.internalBinOpQueryService = other.internalBinOpQueryService;

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

            updateContext.Update(this.logger, src.logger);
            updateContext.Update(this.internalGlobalVarRepo, src.internalGlobalVarRepo);
        }

        public void AddError(SyntaxAnalysisErrorCode code, S.ISyntaxNode node)
        {
            logger.Add(new SyntaxAnalysisErrorLog(code, node, code.ToString()));
        }

        [DoesNotReturn]
        public void AddFatalError(SyntaxAnalysisErrorCode code, S.ISyntaxNode node)
        {
            logger.Add(new SyntaxAnalysisErrorLog(code, node, code.ToString()));
            throw new AnalyzerFatalException();
        }
            
        public IType GetVoidType()
        {
            return (ITypeSymbol)symbolLoader.Load(new VoidSymbolId());
        }            

        public IType GetBoolType()
        {   
            return (ITypeSymbol)symbolLoader.Load(boolId);                    
        }
            
        public IType GetIntType()
        {
            return (ITypeSymbol)symbolLoader.Load(intId);
        }
            
        public IType GetStringType()
        {
            return (ITypeSymbol)symbolLoader.Load(stringId);
        }

        internal ITypeSymbol GetListIterType(ITypeSymbol? itemType)
        {
            throw new NotImplementedException();
        }

        public ITypeSymbol GetListType(ITypeSymbol elemType)
        {
            var typeArgs = Arr(elemType.GetSymbolId());
            var listId = new ModuleSymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System")).Child(new Name.Normal("List"), typeArgs);

            return (ITypeSymbol)symbolLoader.Load(listId);
        }
            
        public void AddInternalGlobalVarInfo(bool bRef, IType typeValue, string name)
        {
            internalGlobalVarRepo.AddInternalGlobalVariable(bRef, typeValue, name);
        }

        public bool IsVoidType(ITypeSymbol type)
        {
            return type.GetSymbolId().Equals(voidId);
        }

        public bool IsBoolType(ITypeSymbol type)
        {
            var declId = type.GetDeclSymbolId();
            return boolDeclId.Equals(declId);
        }

        public bool IsIntType(ITypeSymbol type)
        {
            var declId = type.GetDeclSymbolId();
            return intDeclId.Equals(declId);
        }            

        public bool IsStringType(ITypeSymbol type)
        {
            var declId = type.GetDeclSymbolId();
            return stringDeclId.Equals(declId);
        }

        //public bool IsNullableType(ITypeSymbol type, [NotNullWhen(returnValue: true)] out ITypeSymbol? innerType)
        //{
        //    var declType = type.GetDeclSymbolNode();
        //    var declId = declType.GetDeclSymbolId();

        //    if (!declId.Equals(nullableDeclId))
        //    {
        //        innerType = null;
        //        return false;
        //    }

        //    var typeArgs = type.GetTypeArgs();
        //    Debug.Assert(typeArgs.Length == 1);

        //    innerType = typeArgs[0];
        //    return true;
        //}

        public IType GetSymbolByTypeExp(S.TypeExp typeExp)
        {
            var symbol = typeInfoService.GetSymbol(typeExp);
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

        //public SeqTypeValue GetSeqTypeValue(R.Path.Nested seq, ITypeSymbol yieldType)
        //{
        //    return itemValueFactory.MakeSeqType(seq, yieldType);
        //}

        // outerDeclPath 밑의 (name, typeParamCount)로 가능한 것들을 돌려준다
        public SymbolQueryResult QuerySymbol(SymbolPath? outerPath, Name name, int typeParamCount)
        {
            return symbolLoader.Query(outerPath, name, typeParamCount);
        } 

        public ImmutableArray<InternalBinaryOperatorInfo> GetBinaryOpInfos(S.BinaryOpKind kind)
        {
            return internalBinOpQueryService.GetInfos(kind);
        }

        public TupleSymbol GetTupleType(ImmutableArray<(ITypeSymbol Type, string? Name)> elems)
        {
            throw new NotImplementedException();
            // return symbolLoader.Load(.MakeTupleType(elems);
        }            

        public R.Exp? TryCastExp_Exp(R.Exp exp, ITypeSymbol expectedType) // nothrow
        {
            var expType = exp.GetTypeSymbol();

            // 같으면 그대로 리턴
            if (expectedType.Equals(expType))
                return exp;

            // TODO: TypeValue에 TryCast를 각각 넣기
            // expectType.TryCast(exp); // expResult를 넣는것도 이상하다.. 그건 그때가서

            // 1. enumElem -> enum
            if (expType is EnumElemSymbol enumElem)
            {
                if (expectedType is EnumSymbol expectEnumType)
                {
                    if (expectedType.Equals(enumElem.GetOuter()))
                    {
                        return new R.CastEnumElemToEnumExp(exp, enumElem);
                    }
                }

                return null;
            }

            // 2. exp is class type
            if (expType is ClassSymbol @class)
            {
                if (expectedType is ClassSymbol expectedClass)
                {
                    // allows upcast
                    if (expectedClass.IsBaseOf(@class))
                    {
                        return new R.CastClassExp(exp, expectedClass);
                    }

                    return null;
                }

                // TODO: interface
                // if (expectType is InterfaceTypeValue )
            }
            
            // TODO: 3. C -> Nullable<C>, C -> B -> Nullable<B> 허용
            //if (IsNullableType(expectedType, out var expectedInnerType))
            //{
            //    // C -> B 시도
            //    var castToInnerTypeExp = TryCastExp_Exp(exp, expectedInnerType);
            //    if (castToInnerTypeExp != null)
            //    {
            //        // B -> B?
            //        return MakeNullableExp(castToInnerTypeExp);
            //        return new R.NewNullableExp(castToInnerTypeExp, expectedNullableType);
            //    }
            //}

            return null;
        }

        public LambdaSymbol MakeLambda(IFuncSymbol outer, LambdaDeclSymbol decl)
        {
            return symbolFactory.MakeLambda(outer, decl);
        }

        public bool IsSeqType(ITypeSymbol typeSymbol, [NotNullWhen(returnValue: true)] out ITypeSymbol? itemType)
        {
            throw new NotImplementedException();
        }

        public bool IsListType(ITypeSymbol typeSymbol, [NotNullWhen(returnValue: true)] out ITypeSymbol? itemType)
        {
            throw new NotImplementedException();
        }
    }
    
}
