using Citron.Collections;
using Citron.Infra;
using Citron.Symbol;
using Pretune;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using R = Citron.IR0;
using S = Citron.Syntax;

namespace Citron.Analysis;

[AutoConstructor]
partial class ScopeContext : IMutable<ScopeContext>
{
    // 멤버 변수가 변경되면, IMutable.Clone을 변경해줘야 한다
    GlobalContext globalContext;
    BodyContext bodyContext;     // 본문 전체에 대한 컨텍스트
    ScopeContext? parentContext;

    bool bLoop;    
    ImmutableDictionary<Name, LocalVarInfo> localVarInfos;    

    // private
    ScopeContext IMutable<ScopeContext>.Clone(CloneContext cloneContext)
    {
        var newGlobalContext = cloneContext.GetClone(globalContext);
        var newBodyContext = cloneContext.GetClone(bodyContext);
        var newParentContext = (parentContext != null) ? cloneContext.GetClone(parentContext) : null;

        return new ScopeContext(newGlobalContext, newBodyContext, newParentContext, bLoop, localVarInfos);
    }

    void IMutable<ScopeContext>.Update(ScopeContext src, UpdateContext context)
    {
        context.Update(globalContext, src.globalContext);
        context.Update(bodyContext, src.bodyContext);

        if (parentContext != null)
        {
            Debug.Assert(src.parentContext != null);
            context.Update(parentContext, src.parentContext);
        }
        else
        {
            Debug.Assert(src.parentContext == null);
        }

        bLoop = src.bLoop;
        localVarInfos = src.localVarInfos;
    }

    public ScopeContext(GlobalContext globalContext, BodyContext bodyContext, ScopeContext? parentContext, bool bLoop)
        : this(globalContext, bodyContext, parentContext, bLoop, ImmutableDictionary<Name, LocalVarInfo>.Empty)
    {   
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

    public (ScopeContext, LambdaSymbol) MakeLambdaBodyContext(FuncReturn? ret, ImmutableArray<FuncParameter> parameters, bool bLastParamVariadic)
    {
        var (newBodyContext, lambda) = bodyContext.MakeLambdaBodyContext(this, ret, parameters, bLastParamVariadic);
        var newScopeContext = new ScopeContext(globalContext, newBodyContext, null, false);

        return (newScopeContext, lambda);
    }

    public void AddLocalVarInfo(IType type, Name name)
    {
        localVarInfos = localVarInfos.SetItem(name, new LocalVarInfo(type, name));
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

    // 간소화
    public IntermediateExp? ResolveIdentifier(Name name, ImmutableArray<IType> typeArgs) // throws IdentifierResolverMultipleCandidatesException
    {   
        // scope context는 local을 검색한다
        if (localVarInfos.TryGetValue(name, out var info))
            return new IntermediateExp.LocalVar(info.Type, name);

        if (parentContext != null)
            return parentContext.ResolveIdentifier(name, typeArgs);
        else        
            // body에서 검색
            return bodyContext.ResolveIdentifier(name, typeArgs);
    }
}