using Citron.Collections;
using Citron.Infra;
using Citron.Symbol;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using R = Citron.IR0;
using S = Citron.Syntax;

namespace Citron.Analysis;

partial class ScopeContext : IMutable<ScopeContext>
{
    // 멤버 변수가 변경되면, IMutable.Clone을 변경해줘야 한다
    GlobalContext globalContext;
    BodyContext bodyContext;     // 본문 전체에 대한 컨텍스트
    ScopeContext? parentContext;

    bool bLoop;    
    ImmutableDictionary<Name, LocalVarInfo> localVarInfos;
    ImmutableArray<R.Stmt> rstmts;

    // private
    ScopeContext(GlobalContext globalContext, BodyContext bodyContext, ScopeContext? parentContext, bool bLoop, ImmutableDictionary<Name, LocalVarInfo> localVarInfos)
    {
        this.globalContext = globalContext;
        this.bodyContext = bodyContext;
        this.parentContext = parentContext;

        this.bLoop = bLoop;
        this.localVarInfos = localVarInfos;
    }

    ScopeContext IMutable<ScopeContext>.Clone(CloneContext cloneContext)
    {
        var globalContext = cloneContext.GetClone(this.globalContext);
        var bodyContext = cloneContext.GetClone(this.bodyContext);
        var parentContext = (this.parentContext != null) ? cloneContext.GetClone(this.parentContext) : null;

        var bLoop = this.bLoop;
        var localVarInfos = this.localVarInfos; // immutable dictionary라서 그냥 대입해도 된다

        return new ScopeContext(globalContext, bodyContext, parentContext, bLoop, localVarInfos);
    }

    void IMutable<ScopeContext>.Update(ScopeContext src, UpdateContext context)
    {
        throw new NotImplementedException();
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
        rstmts = rstmts.Add(stmt);
    }

    public void SetFlowEndsCompletely()
    {
        throw new NotImplementedException();
    }

    public ScopeContext MakeNestedScopeContext()
    {
        return new ScopeContext(globalContext, bodyContext, this, bLoop);
    }

    public ScopeContext MakeLoopNestedScopeContext()
    {
        return new ScopeContext(globalContext, bodyContext, this, bLoop: true);
    }

    public (ScopeContext, LambdaSymbol) MakeLambdaBodyContext(FuncReturn? ret, ImmutableArray<FuncParameter> parameters)
    {
        var (newBodyContext, lambda) = bodyContext.MakeLambdaBodyContext(this, ret, parameters);
        var newScopeContext = new ScopeContext(globalContext, newBodyContext, null, false);

        return (newScopeContext, lambda);
    }

    public void AddLocalVarInfo(bool bRef, IType type, Name name)
    {
        localVarInfos = localVarInfos.SetItem(name, new LocalVarInfo(bRef, type, name));
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

    // 현재까지 모인 Stmts들을 ImmutableArray로 리턴한다
    public ImmutableArray<R.Stmt> MakeStmts()
    {
        return rstmts;
    }

    public ExpResult ResolveIdentifier(Name name, ImmutableArray<IType> typeArgs)
    {
        // scope context는 local을 검색한다
        if (localVarInfos.TryGetValue(name, out var info))
            return new ExpResult.LocalVar(info.IsRef, info.Type, name);

        if (parentContext != null)
            return parentContext.ResolveIdentifier(name, typeArgs);
        else
            // body에서 검색
            return bodyContext.ResolveIdentifier(name, typeArgs);
    }
}