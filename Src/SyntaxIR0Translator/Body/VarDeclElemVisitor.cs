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
    public static TranslationResult<ImmutableArray<R.Stmt>> Visit(S.VarDecl decl, ScopeContext context)
    {
        IType? declType;

        if (BodyMisc.IsVarType(decl.Type))
            declType = null;
        else
            declType = context.MakeType(decl.Type);

        return VarDeclElemVisitor.Visit(declType, context, decl.Elems);
    }
}

struct VarDeclElemVisitor
{
    IType? declType; // type or null(var)
    ScopeContext context;
    ImmutableArray<R.Stmt>.Builder builder;

    public static TranslationResult<ImmutableArray<R.Stmt>> Visit(IType? declType, ScopeContext context, ImmutableArray<S.VarDeclElement> elems)
    {
        var builder = ImmutableArray.CreateBuilder<R.Stmt>();

        var visitor = new VarDeclElemVisitor { declType = declType, context = context, builder = builder };
        foreach (var elem in elems)
        {
            if (!visitor.VisitElem(elem))
                return TranslationResult.Error<ImmutableArray<R.Stmt>>();
        }

        return TranslationResult.Valid(builder.ToImmutable());
    }

    bool HandleVarDeclType(S.VarDeclElement syntax)
    {
        if (syntax.InitExp == null)
        {
            context.AddFatalError(A0111_VarDecl_LocalVarDeclNeedInitializer, syntax);
            return false;
        }
        
        var initExpResult = ExpIR0ExpTranslator.Translate(syntax.InitExp, context, hintType: null);
        if (!initExpResult.IsValid(out var initExp))
            return false;

        var initExpType = context.GetExpType(initExp);

        builder.Add(new R.LocalVarDeclStmt(initExpType, syntax.VarName, initExp));
        context.AddLocalVarInfo(initExpType, new Name.Normal(syntax.VarName));

        return true;
    }

    bool HandleExplicitDeclType(S.VarDeclElement syntax)
    {
        Debug.Assert(declType != null);

        if (syntax.InitExp == null)
        {
            context.AddFatalError(A0111_VarDecl_LocalVarDeclNeedInitializer, syntax);
            return false;
        }

        var initExpResult = ExpIR0ExpTranslator.Translate(syntax.InitExp, context, declType);
        if (!initExpResult.IsValid(out var initExp))
            return false;

        var castInitExp = BodyMisc.TryCastExp_Exp(initExp, declType, context);
        
        if (castInitExp == null)
            throw new NotImplementedException(); // 캐스팅이 실패했습니다.

        builder.Add(new R.LocalVarDeclStmt(declType, syntax.VarName, initExp));
        context.AddLocalVarInfo(declType, new Name.Normal(syntax.VarName));

        return true;
    }

    bool VisitElem(S.VarDeclElement syntax)
    {
        if (context.DoesLocalVarNameExistInScope(syntax.VarName))
        {
            context.AddFatalError(A0103_VarDecl_LocalVarNameShouldBeUniqueWithinScope, syntax);
            return true;
        }

        if (declType == null)
        {
            return HandleVarDeclType(syntax);
        }
        else
        {
            return HandleExplicitDeclType(syntax);
        }
    }
}