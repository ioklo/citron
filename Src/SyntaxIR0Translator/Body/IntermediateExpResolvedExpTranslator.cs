using System;
using System.Diagnostics;
using S = Citron.Syntax;
using R = Citron.IR0;

using static Citron.Analysis.SyntaxAnalysisErrorCode;
using Citron.Symbol;

namespace Citron.Analysis;

// outermost로 변경
// IntermediateExp -> TranslationResult<ResolvedExp>
struct IntermediateExpResolvedExpTranslator : IIntermediateExpVisitor<TranslationResult<ResolvedExp>>
{
    ScopeContext context;
    S.ISyntaxNode nodeForErrorReport;

    public static TranslationResult<ResolvedExp> Translate(IntermediateExp imExp, ScopeContext context, S.ISyntaxNode nodeForErrorReport)
    {
        var translator = new IntermediateExpResolvedExpTranslator { context = context, nodeForErrorReport = nodeForErrorReport };
        return imExp.Accept<IntermediateExpResolvedExpTranslator, TranslationResult<ResolvedExp>>(ref translator);
    }

    TranslationResult<ResolvedExp> Valid(ResolvedExp exp)
    {
        return TranslationResult.Valid(exp);
    }
    

    // 내부 에러
    TranslationResult<ResolvedExp> Fatal(SyntaxAnalysisErrorCode code)
    {
        context.AddFatalError(code, nodeForErrorReport);
        return TranslationResult.Error<ResolvedExp>();
    }

    TranslationResult<ResolvedExp> IIntermediateExpVisitor<TranslationResult<ResolvedExp>>.VisitClass(IntermediateExp.Class exp)
    {
        return Fatal(A2008_ResolveIdentifier_CantUseTypeAsExpression);
    }

    TranslationResult<ResolvedExp> IIntermediateExpVisitor<TranslationResult<ResolvedExp>>.VisitClassMemberFuncs(IntermediateExp.ClassMemberFuncs exp)
    {
        // funcs가 한개이면, lambda (boxed lambda)로 변환할 수 있다.
        throw new NotImplementedException();
    }

    TranslationResult<ResolvedExp> IIntermediateExpVisitor<TranslationResult<ResolvedExp>>.VisitClassMemberVar(IntermediateExp.ClassMemberVar exp)
    {
        return Valid(new ResolvedExp.ClassMemberVar(exp.Symbol, exp.HasExplicitInstance, exp.ExplicitInstance));
    }

    TranslationResult<ResolvedExp> IIntermediateExpVisitor<TranslationResult<ResolvedExp>>.VisitEnum(IntermediateExp.Enum exp)
    {
        return Fatal(A2008_ResolveIdentifier_CantUseTypeAsExpression);
    }

    TranslationResult<ResolvedExp> IIntermediateExpVisitor<TranslationResult<ResolvedExp>>.VisitEnumElem(IntermediateExp.EnumElem exp)
    {
        // standalone이면 값으로 처리한다
        if (exp.Symbol.IsStandalone())
            return Valid(new ResolvedExp.IR0Exp(new IR0ExpResult(new R.NewEnumElemExp(exp.Symbol, default), new EnumElemType(exp.Symbol))));

        // lambda (boxed lambda)로 변환할 수 있다.
        throw new NotImplementedException();
    }

    TranslationResult<ResolvedExp> IIntermediateExpVisitor<TranslationResult<ResolvedExp>>.VisitEnumElemMemberVar(IntermediateExp.EnumElemMemberVar exp)
    {
        return Valid(new ResolvedExp.EnumElemMemberVar(exp.Symbol, exp.Instance));
    }

    TranslationResult<ResolvedExp> IIntermediateExpVisitor<TranslationResult<ResolvedExp>>.VisitGlobalFuncs(IntermediateExp.GlobalFuncs exp)
    {
        // funcs가 한개이면, lambda (boxed lambda)로 변환할 수 있다.
        throw new NotImplementedException();
    }

    TranslationResult<ResolvedExp> IIntermediateExpVisitor<TranslationResult<ResolvedExp>>.VisitIR0Exp(IntermediateExp.IR0Exp exp)
    {
        return Valid(new ResolvedExp.IR0Exp(exp.ExpResult));
    }

    TranslationResult<ResolvedExp> IIntermediateExpVisitor<TranslationResult<ResolvedExp>>.VisitLambdaMemberVar(IntermediateExp.LambdaMemberVar exp)
    {
        return Valid(new ResolvedExp.LambdaMemberVar(exp.Symbol));
    }

    TranslationResult<ResolvedExp> IIntermediateExpVisitor<TranslationResult<ResolvedExp>>.VisitListIndexer(IntermediateExp.ListIndexer exp)
    {
        return Valid(new ResolvedExp.ListIndexer(exp.Instance, exp.Index, exp.ItemType));
    }

    TranslationResult<ResolvedExp> IIntermediateExpVisitor<TranslationResult<ResolvedExp>>.VisitLocalVar(IntermediateExp.LocalVar exp)
    {
        return Valid(new ResolvedExp.LocalVar(exp.Type, exp.Name));
    }

    TranslationResult<ResolvedExp> IIntermediateExpVisitor<TranslationResult<ResolvedExp>>.VisitNamespace(IntermediateExp.Namespace exp)
    {
        return Fatal(A2013_ResolveIdentifier_CantUseNamespaceAsExpression);
    }

    TranslationResult<ResolvedExp> IIntermediateExpVisitor<TranslationResult<ResolvedExp>>.VisitStruct(IntermediateExp.Struct exp)
    {
        return Fatal(A2008_ResolveIdentifier_CantUseTypeAsExpression);
    }

    TranslationResult<ResolvedExp> IIntermediateExpVisitor<TranslationResult<ResolvedExp>>.VisitStructMemberFuncs(IntermediateExp.StructMemberFuncs exp)
    {
        // funcs가 한개이면, lambda (boxed lambda)로 변환할 수 있다.
        throw new NotImplementedException();
    }

    TranslationResult<ResolvedExp> IIntermediateExpVisitor<TranslationResult<ResolvedExp>>.VisitStructMemberVar(IntermediateExp.StructMemberVar exp)
    {
        return Valid(new ResolvedExp.StructMemberVar(exp.Symbol, exp.HasExplicitInstance, exp.ExplicitInstance));
    }

    TranslationResult<ResolvedExp> IIntermediateExpVisitor<TranslationResult<ResolvedExp>>.VisitThis(IntermediateExp.ThisVar exp)
    {
        return Valid(new ResolvedExp.ThisVar(exp.Type));
    }

    TranslationResult<ResolvedExp> IIntermediateExpVisitor<TranslationResult<ResolvedExp>>.VisitTypeVar(IntermediateExp.TypeVar exp)
    {
        return Fatal(A2008_ResolveIdentifier_CantUseTypeAsExpression);
    }

    TranslationResult<ResolvedExp> IIntermediateExpVisitor<TranslationResult<ResolvedExp>>.VisitLocalDeref(IntermediateExp.LocalDeref exp)
    {
        return Valid(new ResolvedExp.LocalDeref(exp.Target));
    }

    TranslationResult<ResolvedExp> IIntermediateExpVisitor<TranslationResult<ResolvedExp>>.VisitBoxDeref(IntermediateExp.BoxDeref exp)
    {
        return Valid(new ResolvedExp.BoxDeref(exp.Target));
    }
}
