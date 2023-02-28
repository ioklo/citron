using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Diagnostics.CodeAnalysis;

using Citron.Infra;
using Citron.Symbol;
using Citron.Collections;
using Citron.Log;

using S = Citron.Syntax;
using R = Citron.IR0;

namespace Citron.Analysis;

partial class GlobalContext : IMutable<GlobalContext>
{   
    SymbolFactory symbolFactory;
    ImmutableArray<ModuleDeclSymbol> moduleDeclSymbols;

    ILogger logger;

    // instances    
    ImmutableArray<R.StmtBody> bodies;
    RuntimeTypeComponent runtimeTypeComponent;
    InternalBinaryOperatorQueryService internalBinOpQueryService;    

    public GlobalContext(SymbolFactory symbolFactory, ImmutableArray<ModuleDeclSymbol> moduleDeclSymbols, ILogger logger)
    {
        this.symbolFactory = symbolFactory;
        this.moduleDeclSymbols = moduleDeclSymbols;        

        this.logger = logger;
        this.bodies = default;

        this.runtimeTypeComponent = new RuntimeTypeComponent();
        this.internalBinOpQueryService = new InternalBinaryOperatorQueryService(GetBoolType(), GetIntType(), GetStringType());
    }

    // memberwise
    GlobalContext(SymbolFactory factory, ImmutableArray<ModuleDeclSymbol> moduleDeclSymbols, ILogger logger, ImmutableArray<R.StmtBody> bodies, RuntimeTypeComponent rtcomp, InternalBinaryOperatorQueryService internalBinOpQueryService)
    {
        this.symbolFactory = factory;
        this.moduleDeclSymbols = moduleDeclSymbols;

        this.logger = logger;
        this.bodies = bodies;

        this.runtimeTypeComponent = rtcomp;
        this.internalBinOpQueryService = internalBinOpQueryService;
    }

    GlobalContext IMutable<GlobalContext>.Clone(CloneContext cloneContext)
    {   
        var clonedLogger = cloneContext.GetClone(this.logger);

        return new GlobalContext(
            this.symbolFactory, 
            this.moduleDeclSymbols, 
            clonedLogger,
            this.bodies, 
            this.runtimeTypeComponent, 
            this.internalBinOpQueryService
        );
    }

    void IMutable<GlobalContext>.Update(GlobalContext src, UpdateContext context)
    {
        throw new NotImplementedException();
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

    //public SeqTypeValue GetSeqTypeValue(R.Path.Nested seq, ITypeSymbol yieldType)
    //{
    //    return itemValueFactory.MakeSeqType(seq, yieldType);
    //}

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

    // funcReturn이 null이면 constructor란 뜻이다
    public ScopeContext MakeNewScopeContext(IFuncDeclSymbol symbol, bool bSeqFunc, FuncReturn? funcReturn)
    {
        var newBodyContext = new BodyContext(moduleDeclSymbols, symbolFactory, outerScopeContext: null, symbol, bSeqFunc, bSetReturn: true, funcReturn);
        return new ScopeContext(this, newBodyContext, parentContext: null, bLoop: false);
    }

    public void AddBody(IFuncDeclSymbol symbol, ImmutableArray<R.Stmt> body)
    {
        var rbody = new R.StmtBody(symbol, body);
        bodies = bodies.Add(rbody);
    }

    public ImmutableArray<R.StmtBody> GetBodies()
    {
        return bodies;
    }
}