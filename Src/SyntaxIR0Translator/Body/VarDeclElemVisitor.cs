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

    public static void Visit(IType declType, ScopeContext context, ImmutableArray<S.VarDeclElement> elems)
    {
        var visitor = new VarDeclElemVisitor(declType, context);
        foreach (var elem in elems)
            visitor.VisitElem(elem);
    }

    VarDeclElemVisitor(IType declType, ScopeContext context)
    {   
        this.declType = declType;
        this.context = context;
    }

    void HandleVarDeclType(S.VarDeclElement syntax)
    {
        if (syntax.InitExp == null)
        {
            context.AddFatalError(A0111_VarDecl_LocalVarDeclNeedInitializer, syntax);
            throw new UnreachableCodeException();
        }

        var initExp = ExpVisitor.TranslateAsExp(syntax.InitExp, context, hintType: null);
        var initExpType = initExp.GetExpType();

        context.AddStmt(new R.LocalVarDeclStmt(initExpType, syntax.VarName, initExp));
        context.AddLocalVarInfo(initExpType, new Name.Normal(syntax.VarName));
    }

    void HandleExplicitDeclType(S.VarDeclElement syntax)
    {
        if (syntax.InitExp == null)
        {
            context.AddFatalError(A0111_VarDecl_LocalVarDeclNeedInitializer, syntax);
            throw new UnreachableCodeException();
        }

        var initExp = ExpVisitor.TranslateAsCastExp(syntax.InitExp, context, hintType: declType, targetType: declType);
        if (initExp == null)
            throw new NotImplementedException(); // 캐스팅이 실패했습니다.

        context.AddStmt(new R.LocalVarDeclStmt(declType, syntax.VarName, initExp));
        context.AddLocalVarInfo(declType, new Name.Normal(syntax.VarName));
    }

    void VisitElem(S.VarDeclElement syntax)
    {
        if (context.DoesLocalVarNameExistInScope(syntax.VarName))
            context.AddFatalError(A0103_VarDecl_LocalVarNameShouldBeUniqueWithinScope, syntax);

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

    public static void Visit(LocalRefType declType, ScopeContext context, ImmutableArray<S.VarDeclElement> elems)
    {
        var visitor = new LocalRefVarDeclElemVisitor(declType, context);
        foreach (var elem in elems)
            visitor.VisitElem(elem);
    }

    LocalRefVarDeclElemVisitor(LocalRefType declType, ScopeContext context)
    {
        this.declType = declType;
        this.context = context;
    }

    void HandleVarDeclType(string varName, S.Exp initExpSyntax)
    {
        // box ref가 나와도 local ref로 바꿔준다
        var initExp = RefExpVisitor.TranslateAsLocalRef(initExpSyntax, context, hintInnerType: null); // throws FatalErrorException

        var initExpType = initExp.GetExpType();
        context.AddStmt(new R.LocalVarDeclStmt(initExpType, varName, initExp));
        context.AddLocalVarInfo(initExpType, new Name.Normal(varName));
    }

    void HandleExplicitDeclType(string varName, S.Exp initExpSyntax)
    {   
        var initExp = RefExpVisitor.TranslateAsLocalRef(initExpSyntax, context, declType.InnerType); // throws FatalErrorException

        var initExpType = initExp.GetExpType();
        
        var compareContext = new CyclicEqualityCompareContext();
        if (!compareContext.CompareClass(declType, initExpType))        
            context.AddFatalError(A0102_VarDecl_MismatchBetweenRefDeclTypeAndRefInitType, initExpSyntax);

        context.AddStmt(new R.LocalVarDeclStmt(declType, varName, initExp));
        context.AddLocalVarInfo(declType, new Name.Normal(varName));
    }

    void VisitElem(S.VarDeclElement syntax)
    {
        if (context.DoesLocalVarNameExistInScope(syntax.VarName))
            context.AddFatalError(A0103_VarDecl_LocalVarNameShouldBeUniqueWithinScope, syntax);

        // 공통 처리
        if (syntax.InitExp == null)
            context.AddFatalError(A0106_VarDecl_RefDeclNeedInitializer, syntax);

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