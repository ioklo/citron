using System;
using Citron.Symbol;

using S = Citron.Syntax;
using R = Citron.IR0;

using static Citron.Analysis.SyntaxAnalysisErrorCode;

namespace Citron.Analysis;

struct VarDeclElemVisitor
{
    bool bRefDeclType;
    IType declType;
    ScopeContext context;

    public VarDeclElemVisitor(bool bRefDeclType, IType declType, ScopeContext context)
    {
        this.bRefDeclType = bRefDeclType;
        this.declType = declType;
        this.context = context;
    }

    void HandleNormalInit(S.VarDeclElement syntax, S.Exp initExpSyntax)
    {
        if (!bRefDeclType)
        {
            if (declType is not VarType) // int x = ...;
            {
                var initExp = ExpVisitor.TranslateAsCastExp(initExpSyntax, context, hintType: declType, targetType: declType);
                if (initExp == null)
                    throw new NotImplementedException(); // 캐스팅이 실패했습니다.

                context.AddStmt(new R.LocalVarDeclStmt(declType, syntax.VarName, initExp));
                context.AddLocalVarInfo(false, declType, new Name.Normal(syntax.VarName));
            }
            else // var x = ...;
            {
                var initExp = ExpVisitor.TranslateAsExp(initExpSyntax, context, hintType: null);
                var initExpType = initExp.GetExpType();

                context.AddStmt(new R.LocalVarDeclStmt(initExpType, syntax.VarName, initExp));
                context.AddLocalVarInfo(false, initExpType, new Name.Normal(syntax.VarName));
            }
        }
        else
        {
            if (declType is not VarType) // ref int x = ...;
            {
                // TODO: Box expression에서 ref 가져오기 
                // box<int> b = box 3;
                // ref int = b;
                throw new NotImplementedException();
            }
            else
            {
                // ref var x = 0;
                context.AddFatalError(A0107_VarDecl_DontAllowVarWithRef, syntax);
            }
        }
    }

    void HandleRefInit(S.VarDeclElement syntax, S.Exp initExpSyntax)
    {
        if (!bRefDeclType)
        {
            if (declType is not VarType)  // int x = ref y;
            {
                context.AddFatalError(A0109_VarDecl_ShouldBeRefDeclWithRefInitializer, syntax);
            }
            else // var x = ref y
            {
                var initResult = ExpVisitor.TranslateAsLoc(initExpSyntax, context, hintType: null, bWrapExpAsLoc: false);
                if (initResult == null)
                    context.AddFatalError(A0108_VarDecl_RefNeedLocation, initExpSyntax);

                var (initLoc, initType) = initResult.Value;
                var stmt = new R.LocalRefVarDeclStmt(syntax.VarName, initLoc);
                context.AddStmt(stmt);
                context.AddLocalVarInfo(true, initType, new Name.Normal(syntax.VarName));
            }
        }
        else
        {
            if (declType is not VarType) // ref int x = ref y;
            {
                var initResult = ExpVisitor.TranslateAsLoc(initExpSyntax, context, hintType: null, bWrapExpAsLoc: false);
                if (initResult == null)
                    context.AddFatalError(A0108_VarDecl_RefNeedLocation, initExpSyntax);

                var (initLoc, _) = initResult.Value;
                var stmt = new R.LocalRefVarDeclStmt(syntax.VarName, initLoc);
                context.AddStmt(stmt);

                context.AddLocalVarInfo(true, declType, new Name.Normal(syntax.VarName));
            }
            else
            {
                context.AddFatalError(A0107_VarDecl_DontAllowVarWithRef, syntax); // ref var x = ref exp; 에러
            }
        }
    }

    void HandleNoInit(S.VarDeclElement syntax)
    {
        if (!bRefDeclType)
        {
            if (declType is not VarType)
            {
                // TODO: local이라면 initializer가 꼭 있어야 합니다 => wrong, default constructor가 있으면 된다
                throw new NotImplementedException();
                // context.AddFatalError(A0111_VarDecl_LocalVarDeclNeedInitializer, syntax);
            }
            else
            {
                context.AddFatalError(A0101_VarDecl_CantInferVarType, syntax);
            }
        }
        else
        {
            context.AddFatalError(A0106_VarDecl_RefDeclNeedInitializer, syntax);
        }

        // default constructor 호출
        // context.AddStmt(new R.LocalVarDeclStmt(declType, elem.VarName, ))
        throw new NotImplementedException();
    }

    public void VisitElem(S.VarDeclElement syntax)
    {
        if (context.DoesLocalVarNameExistInScope(syntax.VarName))
            context.AddFatalError(A0103_VarDecl_LocalVarNameShouldBeUniqueWithinScope, syntax);

        if (syntax.Initializer != null)
        {
            if (!syntax.Initializer.Value.IsRef)
                HandleNormalInit(syntax, syntax.Initializer.Value.Exp);
            else
                HandleRefInit(syntax, syntax.Initializer.Value.Exp);
        }
        else
        {
            HandleNoInit(syntax);
        }
    }
}