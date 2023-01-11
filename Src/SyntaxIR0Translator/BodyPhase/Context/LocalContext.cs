using Citron.Collections;
using Citron.Infra;
using Citron.Symbol;
using System;
using System.Diagnostics;
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
    IType outerType;
    ImmutableDictionary<Name, LocalVarInfo> localVarInfos;

    #region GlobalContext
    public IType MakeType(S.TypeExp typeExp) => globalContext.MakeType(typeExp);
    public void AddFatalError(SyntaxAnalysisErrorCode code, S.ISyntaxNode node) => globalContext.AddFatalError(code, node);
    public InternalGlobalVarInfo? GetInternalGlobalVarInfo(string idName) => globalContext.GetInternalGlobalVarInfo(idName);
    #endregion

    #region BodyContext
    public IType? GetThisType() => bodyContext.GetThisType(); // 함수의 body가 어느 클래스/구조체/글로벌 에서 작성되었는지
    public IFuncDeclSymbol GetFuncDeclSymbol() => bodyContext.GetFuncDeclSymbol();
    #endregion

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

    public LocalContext(LocalContext other, CloneContext cloneContext)
    {
        if (other.parentLocalContext != null)
            this.parentLocalContext = cloneContext.GetClone(other.parentLocalContext);
        else
            this.parentLocalContext = null;

        this.localVarInfos = other.localVarInfos;
        this.bLoop = other.bLoop;
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

    public LocalContext NewLocalContext()
    {
        return new LocalContext(this, bLoop);
    }

    public LocalContext NewLocalContextWithLoop()
    {
        return new LocalContext(this, true);
    }

    public void AddLocalVarInfo(bool bRef, ITypeSymbol typeValue, Name name)
    {
        localVarInfos = localVarInfos.SetItem(name, new LocalVarInfo(bRef, typeValue, name));
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

    // 람다 안에 있을때, outerType은 람다, thisType은 body의 타입
    public IType GetOuterType()
    {
        return outerType;
    }

    public SymbolQueryResult QueryMember(string idName, int length)
    {
        throw new NotImplementedException();
    }

    
}