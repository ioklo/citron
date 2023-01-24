using Citron.Collections;
using Citron.Infra;
using Citron.Symbol;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using R = Citron.IR0;
using S = Citron.Syntax;

namespace Citron.Analysis;

class ScopeContext
{
    public struct LocalVarInfo
    {
        public bool IsRef { get; }
        public IType Type { get; }
        public Name Name { get; }

        public LocalVarInfo(bool bRef, IType type, Name name)
        {
            IsRef = bRef;
            Name = name;
            Type = type;
        }

        public LocalVarInfo UpdateTypeValue(IType newType)
        {
            return new LocalVarInfo(IsRef, newType, Name);
        }
    }

    GlobalContext globalContext;
    BodyContext bodyContext;     // 본문 전체에 대한 컨텍스트
    ScopeContext? parentContext;

    bool bLoop;    
    ImmutableDictionary<Name, LocalVarInfo> localVarInfos;

    #region GlobalContext
    public IType MakeType(S.TypeExp typeExp) => globalContext.MakeType(typeExp);
    [DoesNotReturn] 
    public void AddFatalError(SyntaxAnalysisErrorCode code, S.ISyntaxNode node) => globalContext.AddFatalError(code, node);
    public void AddError(SyntaxAnalysisErrorCode code, S.ISyntaxNode node) => globalContext.AddError(code, node);
    public InternalGlobalVarInfo? GetInternalGlobalVarInfo(string idName) => globalContext.GetInternalGlobalVarInfo(idName);

    public IType GetVoidType() => globalContext.GetVoidType();
    public IType GetBoolType() => globalContext.GetBoolType();
    public IType GetIntType() => globalContext.GetIntType();
    public IType GetStringType() => globalContext.GetStringType();
    public IType GetListType(IType itemType) => globalContext.GetListType(itemType);
    public IType GetListIterType(IType itemType) => globalContext.GetListIterType(itemType);

    public bool IsVoidType(IType type) => globalContext.IsVoidType(type);
    public bool IsBoolType(IType type) => globalContext.IsBoolType(type);
    public bool IsIntType(IType type) => globalContext.IsIntType(type);
    public bool IsStringType(IType type) => globalContext.IsStringType(type);
    public bool IsListType(IType type, [NotNullWhen(returnValue: true)] out IType? itemType) => globalContext.IsListType(type, out itemType);
    public bool IsSeqType(IType type, [NotNullWhen(returnValue: true)] out IType? itemType) => globalContext.IsSeqType(type, out itemType);

    public ImmutableArray<InternalBinaryOperatorInfo> GetBinaryOpInfos(S.BinaryOpKind kind) => globalContext.GetBinaryOpInfos(kind);

    #endregion

    #region BodyContext
    public IType? GetThisType() => bodyContext.GetThisType(); // 함수의 body가 어느 클래스/구조체/글로벌 에서 작성되었는지
    public IFuncDeclSymbol GetFuncDeclSymbol() => bodyContext.GetFuncDeclSymbol();
    public bool CanAccess(ISymbolNode node) => bodyContext.CanAccess(node);
    public bool IsSetReturn() => bodyContext.IsSetReturn();
    public bool IsSeqFunc() => bodyContext.IsSeqFunc();
    public FuncReturn? GetReturn() => bodyContext.GetReturn();
    public void SetReturn(bool bRef, IType retType) => bodyContext.SetReturn(bRef, retType);
    #endregion

    public ScopeContext(ScopeContext other, CloneContext cloneContext)
    {
        if (other.parentContext != null)
            this.parentContext = cloneContext.GetClone(other.parentContext);
        else
            this.parentContext = null;

        this.localVarInfos = other.localVarInfos;
        this.bLoop = other.bLoop;
    }

    public ScopeContext(GlobalContext globalContext, BodyContext bodyContext, ScopeContext? parentContext, bool bLoop)
    {
        this.globalContext = globalContext;
        this.bodyContext = bodyContext;
        this.parentContext = parentContext;

        this.bLoop = bLoop;        

        this.localVarInfos = ImmutableDictionary<Name, LocalVarInfo>.Empty;
    }

    public void AddStmt(R.Stmt stmt)
    {
        throw new NotImplementedException();
    }

    public void SetFlowEndsCompletely()
    {
        throw new NotImplementedException();
    }
    
    public LocalContext Clone(CloneContext context)
    {
        return new LocalContext(this, context);
    }

    public void Update(LocalContext src, UpdateContext context)
    {
        if (this.parentLocalContext != null)
            context.Update(this.parentLocalContext, src.parentLocalContext!);
        else
            Debug.Assert(src.parentLocalContext == null);

        this.localVarInfos = src.localVarInfos;
        this.bLoop = src.bLoop;
    }

    public ScopeContext MakeNestedScopeContext()
    {
        return new ScopeContext(globalContext, bodyContext, this, bLoop);
    }

    public ScopeContext MakeLoopNestedScopeContext()
    {
        return new ScopeContext(globalContext, bodyContext, this, bLoop: true);
    }

    public ScopeContext MakeNestedBodyContext(FuncReturn? ret, bool bSeqFunc)
    {
        throw new NotImplementedException();
    }

    public void AddLocalVarInfo(bool bRef, IType type, Name name)
    {
        localVarInfos = localVarInfos.SetItem(name, new LocalVarInfo(bRef, type, name));
    }

    public void SetLocalVarType(string name, ITypeSymbol typeValue)
    {
        var normalName = new Name.Normal(name);

        var value = localVarInfos[normalName];
        localVarInfos = localVarInfos.SetItem(normalName, value.UpdateTypeValue(typeValue));
    }

    public LocalVarInfo? GetLocalVarInfo(Name varName)
    {
        if (localVarInfos.TryGetValue(varName, out var info))
            return info;

        if (parentContext != null)
            return parentContext.GetLocalVarInfo(varName);

        return null;
    }

    public bool DoesLocalVarNameExistInScope(string name)
    {
        return localVarInfos.ContainsKey(new Name.Normal(name));
    }

    public bool IsInLoop()
    {
        return bLoop;
    }
    
    public SymbolQueryResult QueryMember(string idName, int length)
    {
        throw new NotImplementedException();
    }

    public IdExpIdentifierResolver MakeIdentifierResolver(Name name, ImmutableArray<IType> typeArgs)
    {
        return globalContext.MakeIdentifierResolver(name, typeArgs, this);
    }

    // 현재까지 모인 Stmts들을 Block 또는 단일 Stmt로 만들고 리턴한다
    public R.Stmt MakeSingleStmt()
    {
        throw new NotImplementedException();
    }

    // 현재까지 모인 Stmts들을 ImmutableArray로 리턴한다
    public ImmutableArray<R.Stmt> MakeStmts()
    {
        throw new NotImplementedException();
    }
}