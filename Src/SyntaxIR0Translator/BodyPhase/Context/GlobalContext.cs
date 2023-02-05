using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Diagnostics.CodeAnalysis;

using Citron.Infra;
using Citron.Symbol;
using Citron.Collections;
using Citron.Log;

using S = Citron.Syntax;

namespace Citron.Analysis;

partial class GlobalContext : IMutable<GlobalContext>
{   
    SymbolFactory symbolFactory;
    ImmutableArray<ModuleDeclSymbol> moduleDeclSymbols;

    ILogger logger;

    // instances    
    InternalBinaryOperatorQueryService internalBinOpQueryService;
    InternalGlobalVariableRepository internalGlobalVarRepo;

    public GlobalContext(SymbolFactory symbolFactory, ImmutableArray<ModuleDeclSymbol> moduleDeclSymbols, ILogger logger)
    {
        this.symbolFactory = symbolFactory;
        this.moduleDeclSymbols = moduleDeclSymbols;        

        this.logger = logger;        

        this.internalBinOpQueryService = new InternalBinaryOperatorQueryService(GetBoolType(), GetIntType(), GetStringType());
        this.internalGlobalVarRepo = new InternalGlobalVariableRepository();
    }

    GlobalContext(SymbolFactory factory, ImmutableArray<ModuleDeclSymbol> moduleDeclSymbols, ILogger logger, InternalBinaryOperatorQueryService internalBinOpQueryService, InternalGlobalVariableRepository internalGlobalVarRepo)
    {
        this.symbolFactory = factory;
        this.moduleDeclSymbols = moduleDeclSymbols;
        this.logger = logger;
        this.internalBinOpQueryService = internalBinOpQueryService;
        this.internalGlobalVarRepo = internalGlobalVarRepo;
    }

    GlobalContext IMutable<GlobalContext>.Clone(CloneContext cloneContext)
    {
        var symbolFactory = this.symbolFactory;
        var moduleDeclSymbols = this.moduleDeclSymbols;
        var logger = cloneContext.GetClone(this.logger);

        var internalBinOpQueryService = this.internalBinOpQueryService;        
        var internalGlobalVarRepo = cloneContext.GetClone(this.internalGlobalVarRepo);

        return new GlobalContext(symbolFactory, moduleDeclSymbols, logger, internalBinOpQueryService, internalGlobalVarRepo);
    }

    // typeParams를 치환하지 않고 그대로 만든다
    //public TSymbol? LoadOpenSymbol<TSymbol>(SymbolId outerId, string name, ImmutableArray<string> typeParams, ImmutableArray<FuncParamId> paramIds)
    //    where TSymbol : class, ISymbolNode
    //{
    //    // class C<T, U> { C<T, U> c; }
    //    // C<T, U>.T에서 T를 만들려면 C<T, U>정의가 필요하고, C<T, U>를 만들려면 T의 정의가 필요하다
    //    // 먼저, T, U를 outer 없이 만들고, C<T, U>를 만든 다음, outer에 채워넣는다

    //    // 1. typeArgIds만들기
    //    var typeVarPaths = new List<SymbolPath>(); // 나중에 outer를 넣기 위해 저장
    //    var typeArgIdsBuilder = ImmutableArray.CreateBuilder<SymbolId>();
    //    for (int i = 0; i < typeParams.Length; i++)
    //    {
    //        // outer를 일단 null로 지정한다
    //        var typeVarPath = new SymbolPath(null, new Name.Normal(typeParams[i]));
    //        var typeVarId = new ModuleSymbolId(outerId.ModuleName, typeVarPath);
    //        typeArgIdsBuilder.Add(typeVarId);
    //        typeVarPaths.Add(typeVarPath);
    //    }
    //    var typeArgIds = typeArgIdsBuilder.ToImmutable();

    //    // 2. C<T, U>만들기
    //    var symbolId = outerId.Child(new Name.Normal(name), typeArgIds, paramIds);

    //    // 3. 다시 채워넣기, NOTICE: typeVarPath가 ref type이라서 가능하다
    //    foreach (var typeVarPath in typeVarPaths)
    //        typeVarPath.Outer = symbolId.Path;

    //    return symbolLoader.Load(symbolId) as TSymbol;
    //}

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
        return new VoidType();
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

    public IType GetListIterType(IType? itemType)
    {
        throw new NotImplementedException();
    }

    public IType GetListType(IType elemType)
    {
        var typeArgs = Arr(elemType.GetSymbolId());
        var listId = new ModuleSymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System")).Child(new Name.Normal("List"), typeArgs);

        return (ITypeSymbol)symbolLoader.Load(listId);
    }

    public void AddInternalGlobalVarInfo(bool bRef, IType typeValue, string name)
    {
        internalGlobalVarRepo.AddInternalGlobalVariable(bRef, typeValue, name);
    }

    public bool IsVoidType(IType type)
    {
        return voidId.Equals(type.GetTypeId());
    }

    public bool IsBoolType(IType type)
    {   
        return boolId.Equals(type.GetTypeId());
    }

    public bool IsIntType(IType type)
    {
        return intId.Equals(type.GetTypeId());
    }

    public bool IsStringType(IType type)
    {
        return stringId.Equals(type.GetTypeId());
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

    public IType MakeType(S.TypeExp typeExp)
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

    public LambdaSymbol MakeLambda(IFuncSymbol outer, LambdaDeclSymbol decl)
    {
        return symbolFactory.MakeLambda(outer, decl);
    }

    public bool IsSeqType(IType type, [NotNullWhen(returnValue: true)] out IType? itemType)
    {
        throw new NotImplementedException();
    }

    public bool IsListType(IType type, [NotNullWhen(returnValue: true)] out IType? itemType)
    {
        throw new NotImplementedException();
    }
}