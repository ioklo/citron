using System;
using Citron.Symbol;

using S = Citron.Syntax;
using R = Citron.IR0;

using static Citron.Analysis.SyntaxAnalysisErrorCode;
using Citron.Infra;
using System.Diagnostics;
using Citron.Collections;

namespace Citron.Analysis;

struct VarDeclVisitor
{
    public static ImmutableArray<R.Stmt> Visit(S.VarDecl decl, ScopeContext context)
    {
        var declType = context.MakeType(decl.Type);

        switch (declType)
        {
            case LocalRefType localRefType:
                return LocalRefVarDeclElemVisitor.Visit(localRefType, context, decl.Elems);                

            case BoxRefType:
                throw new NotImplementedException();

            default:
                return VarDeclElemVisitor.Visit(declType, context, decl.Elems);                
        }
    }
}

struct VarDeclElemVisitor
{
    IType declType;
    ScopeContext context;
    ImmutableArray<R.Stmt>.Builder builder;

    public static ImmutableArray<R.Stmt> Visit(IType declType, ScopeContext context, ImmutableArray<S.VarDeclElement> elems)
    {
        var builder = ImmutableArray.CreateBuilder<R.Stmt>();

        var visitor = new VarDeclElemVisitor(declType, context, builder);
        foreach (var elem in elems)
            visitor.VisitElem(elem);

        return builder.ToImmutable();
    }

    VarDeclElemVisitor(IType declType, ScopeContext context, ImmutableArray<R.Stmt>.Builder builder)
    {   
        this.declType = declType;
        this.context = context;
        this.builder = builder;
    }

    void HandleVarDeclType(S.VarDeclElement syntax)
    {
        if (syntax.InitExp == null)
        {
            context.AddFatalError(A0111_VarDecl_LocalVarDeclNeedInitializer, syntax);
            return Error();
        }

        var initExp = ExpIR0ExpTranslator.Translate(syntax.InitExp, context, hintType: null);
        var initExpType = initExp.GetExpType();

        builder.Add(new R.LocalVarDeclStmt(initExpType, syntax.VarName, initExp));
        context.AddLocalVarInfo(initExpType, new Name.Normal(syntax.VarName));
    }

    void HandleExplicitDeclType(S.VarDeclElement syntax)
    {
        if (syntax.InitExp == null)
        {
            context.AddFatalError(A0111_VarDecl_LocalVarDeclNeedInitializer, syntax);
            return Error();
        }

        var initExp = ExpIR0ExpTranslator.Translate(syntax.InitExp, context, declType);
        var castInitExp = BodyMisc.TryCastExp_Exp(initExp, declType);
        
        if (castInitExp == null)
            throw new NotImplementedException(); // 캐스팅이 실패했습니다.

        builder.Add(new R.LocalVarDeclStmt(declType, syntax.VarName, initExp));
        context.AddLocalVarInfo(declType, new Name.Normal(syntax.VarName));
    }

    void VisitElem(S.VarDeclElement syntax)
    {
        if (context.DoesLocalVarNameExistInScope(syntax.VarName))
        {
            context.AddFatalError(A0103_VarDecl_LocalVarNameShouldBeUniqueWithinScope, syntax);
            return Error();
        }

        if (declType is VarType)
        {
            HandleVarDeclType(syntax);
        }
        else
        {
            HandleExplicitDeclType(syntax);
        }
    }
}

struct LocalRefVarDeclElemVisitor
{    
    LocalRefType declType;
    ScopeContext context;
    ImmutableArray<R.Stmt>.Builder builder;

    public static ImmutableArray<R.Stmt> Visit(LocalRefType declType, ScopeContext context, ImmutableArray<S.VarDeclElement> elems)
    {
        var builder = ImmutableArray.CreateBuilder<R.Stmt>();
        var visitor = new LocalRefVarDeclElemVisitor(declType, context, builder);
        foreach (var elem in elems)
            visitor.VisitElem(elem);

        return builder.ToImmutable();
    }

    LocalRefVarDeclElemVisitor(LocalRefType declType, ScopeContext context, ImmutableArray<R.Stmt>.Builder builder)
    {
        this.declType = declType;
        this.context = context;
        this.builder = builder;
    }

    void HandleVarDeclType(string varName, S.Exp initExpSyntax)
    {
        // box ref가 나와도 local ref로 바꿔준다
        var initExp = ExpIR0LocalRefTranslator.Translate(initExpSyntax, context, innerHintType: null); // throws FatalErrorException

        var initExpType = initExp.GetExpType();
        builder.Add(new R.LocalVarDeclStmt(initExpType, varName, initExp));
        context.AddLocalVarInfo(initExpType, new Name.Normal(varName));
    }

    void HandleExplicitDeclType(string varName, S.Exp initExpSyntax)
    {   
        var initExp = ExpIR0LocalRefTranslator.Translate(initExpSyntax, context, declType.InnerType); // throws FatalErrorException

        var initExpType = initExp.GetExpType();
        
        var compareContext = new CyclicEqualityCompareContext();
        if (!compareContext.CompareClass(declType, initExpType))
        {
            context.AddFatalError(A0102_VarDecl_MismatchBetweenRefDeclTypeAndRefInitType, initExpSyntax);
            return Error();
        }

        builder.Add(new R.LocalVarDeclStmt(declType, varName, initExp));
        context.AddLocalVarInfo(declType, new Name.Normal(varName));
    }

    void VisitElem(S.VarDeclElement syntax)
    {
        if (context.DoesLocalVarNameExistInScope(syntax.VarName))
        {
            context.AddFatalError(A0103_VarDecl_LocalVarNameShouldBeUniqueWithinScope, syntax);
            return Error();
        }

        // 공통 처리
        if (syntax.InitExp == null)
        {
            context.AddFatalError(A0106_VarDecl_RefDeclNeedInitializer, syntax);
            return Error();
        }

        if (declType.InnerType is VarType)
        {
            HandleVarDeclType(syntax.VarName, syntax.InitExp);
        }
        else
        {
            HandleExplicitDeclType(syntax.VarName, syntax.InitExp);
        }
    }
}