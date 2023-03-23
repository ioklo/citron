using System;
using Citron.Symbol;

using S = Citron.Syntax;
using R = Citron.IR0;

using static Citron.Analysis.SyntaxAnalysisErrorCode;
using Citron.Infra;

namespace Citron.Analysis;

struct VarDeclElemVisitor
{    
    IType declType;
    ScopeContext context;

    public VarDeclElemVisitor(IType declType, ScopeContext context)
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

    public void VisitElem(S.VarDeclElement syntax)
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