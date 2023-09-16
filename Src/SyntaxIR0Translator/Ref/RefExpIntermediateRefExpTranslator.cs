using Citron.Symbol;
using S = Citron.Syntax;
using R = Citron.IR0;

using static Citron.Analysis.SyntaxAnalysisErrorCode;
using System.ComponentModel.DataAnnotations;
using System;

namespace Citron.Analysis;

// & exp syntax를 중간과정으로 번역해주는 역할
// SyntaxExp -> IntermediateRefExp
struct RefExpIntermediateRefExpTranslator : S.IExpVisitor<TranslationResult<IntermediateRefExp>>
{
    ScopeContext context;

    public static TranslationResult<IntermediateRefExp> Translate(S.Exp exp, ScopeContext context)
    {
        var translator = new RefExpIntermediateRefExpTranslator { context = context };
        return exp.Accept<RefExpIntermediateRefExpTranslator, TranslationResult<IntermediateRefExp>>(ref translator);
    }

    static TranslationResult<IntermediateRefExp> Valid(IntermediateRefExp imRefExp)
    {
        return TranslationResult.Valid(imRefExp);
    }

    static TranslationResult<IntermediateRefExp> Error()
    {
        return TranslationResult.Error<IntermediateRefExp>();
    }

    TranslationResult<IntermediateRefExp> Fatal(SyntaxAnalysisErrorCode code, S.ISyntaxNode nodeForErrorReport)
    {
        context.AddFatalError(code, nodeForErrorReport);
        return TranslationResult.Error<IntermediateRefExp>();
    }

    TranslationResult<IntermediateRefExp> HandleValue(S.Exp exp)
    {
        var result = ExpIR0ExpTranslator.Translate(exp, context, hintType: null);
        if (!result.IsValid(out var expResult))
            return Error();

        return Valid(new IntermediateRefExp.LocalValue(expResult));
    }
    
    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitIdentifier(S.IdentifierExp exp)
    {
        try
        {
            var typeArgs = BodyMisc.MakeTypeArgs(exp.TypeArgs, context);
            var imExp = context.ResolveIdentifier(new Name.Normal(exp.Value), typeArgs);
            if (imExp == null)
            {
                return Fatal(A2007_ResolveIdentifier_NotFound, exp);
            }

            var imRefExp = IntermediateExpIntermediateRefExpTranslator.Translate(imExp);
            if (imRefExp == null)
            {
                return Fatal(A3001_Reference_CantMakeReference, exp);
            }

            return Valid(imRefExp);
        }
        catch (IdentifierResolverMultipleCandidatesException)
        {
            return Fatal(A2001_ResolveIdentifier_MultipleCandidatesForIdentifier, exp);
        }
    }

    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitMember(S.MemberExp exp)
    {
        var parentResult = RefExpIntermediateRefExpTranslator.Translate(exp.Parent, context);
        if (!parentResult.IsValid(out var parent))
            return Error();

        var typeArgs = BodyMisc.MakeTypeArgs(exp.MemberTypeArgs, context);

        return IntermediateRefExpMemberBinder.Bind(parent, new Name.Normal(exp.MemberName), typeArgs, context, exp.Parent);
    }

    // e[e] 꼴
    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitIndexer(S.IndexerExp exp)
    {
        // location으로 쓰지 않고 value로 쓴다
        return HandleValue(exp);
    }

    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitUnaryOp(S.UnaryOpExp exp)
    {
        if (exp.Kind == S.UnaryOpKind.Ref) // & &는 불가능
        {
            var refExpResult = RefExpIR0ExpTranslator.Translate(exp.Operand, context);
            if (!refExpResult.IsValid(out var refExp))
                return Error();

            return Valid(new IntermediateRefExp.LocalValue(refExp));
        }
        else if (exp.Kind == S.UnaryOpKind.Deref) // *pS
        {
            var operandResult = ExpIR0LocTranslator.Translate(exp.Operand, context, hintType: null, bWrapExpAsLoc: true, A2015_ResolveIdentifier_ExpressionIsNotLocation);
            if (!operandResult.IsValid(out var operandLocResult))
                return Error();

            return Valid(new IntermediateRefExp.DerefedBoxValue(operandLocResult.Loc, operandLocResult.LocType));
        }
        else
        {
            return HandleValue(exp);
        }
    }

    // string은 중간과정에서는 value로 평가하면 될 것 같다
    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitString(S.StringExp exp)
    {
        return HandleValue(exp);
    }

    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitIntLiteral(S.IntLiteralExp exp)
    {
        return HandleValue(exp);
    }

    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitBoolLiteral(S.BoolLiteralExp exp)
    {
        return HandleValue(exp);
    }

    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitNullLiteral(S.NullLiteralExp exp)
    {
        return HandleValue(exp);
    }

    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitBinaryOp(S.BinaryOpExp exp)
    {
        // assign 제외
        return HandleValue(exp);
    }
    
    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitCall(S.CallExp exp)
    {
        return HandleValue(exp);
    }

    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitLambda(S.LambdaExp exp)
    {
        return HandleValue(exp);
    }

    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitList(S.ListExp exp)
    {
        return HandleValue(exp);
    }

    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitNew(S.NewExp exp)
    {
        return HandleValue(exp);
    }

    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitBox(S.BoxExp exp)
    {
        return HandleValue(exp);
    }

    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitIs(S.IsExp exp)
    {
        return HandleValue(exp);
    }

    TranslationResult<IntermediateRefExp> S.IExpVisitor<TranslationResult<IntermediateRefExp>>.VisitAs(S.AsExp exp)
    {
        return HandleValue(exp);
    }
}

