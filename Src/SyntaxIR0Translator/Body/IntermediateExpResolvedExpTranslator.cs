using System;
using System.Diagnostics;
using S = Citron.Syntax;
using R = Citron.IR0;

using static Citron.Analysis.SyntaxAnalysisErrorCode;

namespace Citron.Analysis;

// outermost로 변경
// IntermediateExp -> ResolvedExp
struct IntermediateExpResolvedExpTranslator : IIntermediateExpVisitor<ResolvedExp>
{
    ScopeContext context;
    S.ISyntaxNode nodeForErrorReport;

    public static ResolvedExp Translate(IntermediateExp imExp, ScopeContext context, S.ISyntaxNode nodeForErrorReport)
    {
        var translator = new IntermediateExpResolvedExpTranslator { context = context, nodeForErrorReport = nodeForErrorReport };
        return imExp.Accept<IntermediateExpResolvedExpTranslator, ResolvedExp>(ref translator);
    }

    ResolvedExp Fatal(SyntaxAnalysisErrorCode code)
    {
        context.AddFatalError(code, nodeForErrorReport);
        throw new UnreachableException();
    }

    ResolvedExp IIntermediateExpVisitor<ResolvedExp>.VisitClass(IntermediateExp.Class exp)
    {
        return Fatal(A2008_ResolveIdentifier_CantUseTypeAsExpression);
    }

    ResolvedExp IIntermediateExpVisitor<ResolvedExp>.VisitClassMemberFuncs(IntermediateExp.ClassMemberFuncs exp)
    {
        // funcs가 한개이면, lambda (boxed lambda)로 변환할 수 있다.
        throw new NotImplementedException();
    }

    ResolvedExp IIntermediateExpVisitor<ResolvedExp>.VisitClassMemberVar(IntermediateExp.ClassMemberVar exp)
    {
        return new ResolvedExp.ClassMemberVar(exp.Symbol, exp.HasExplicitInstance, exp.ExplicitInstance);
    }

    ResolvedExp IIntermediateExpVisitor<ResolvedExp>.VisitEnum(IntermediateExp.Enum exp)
    {
        return Fatal(A2008_ResolveIdentifier_CantUseTypeAsExpression);
    }

    ResolvedExp IIntermediateExpVisitor<ResolvedExp>.VisitEnumElem(IntermediateExp.EnumElem exp)
    {
        // standalone이면 값으로 처리한다
        if (exp.Symbol.IsStandalone())
            return new ResolvedExp.IR0Exp(new R.NewEnumElemExp(exp.Symbol, default));

        // lambda (boxed lambda)로 변환할 수 있다.
        throw new NotImplementedException();
    }

    ResolvedExp IIntermediateExpVisitor<ResolvedExp>.VisitEnumElemMemberVar(IntermediateExp.EnumElemMemberVar exp)
    {
        return new ResolvedExp.EnumElemMemberVar(exp.Symbol, exp.Instance);
    }

    ResolvedExp IIntermediateExpVisitor<ResolvedExp>.VisitGlobalFuncs(IntermediateExp.GlobalFuncs exp)
    {
        // funcs가 한개이면, lambda (boxed lambda)로 변환할 수 있다.
        throw new NotImplementedException();
    }

    ResolvedExp IIntermediateExpVisitor<ResolvedExp>.VisitIR0Exp(IntermediateExp.IR0Exp exp)
    {
        return new ResolvedExp.IR0Exp(exp.Exp);
    }

    ResolvedExp IIntermediateExpVisitor<ResolvedExp>.VisitLambdaMemberVar(IntermediateExp.LambdaMemberVar exp)
    {
        return new ResolvedExp.LambdaMemberVar(exp.Symbol);
    }

    ResolvedExp IIntermediateExpVisitor<ResolvedExp>.VisitListIndexer(IntermediateExp.ListIndexer exp)
    {
        return new ResolvedExp.ListIndexer(exp.Instance, exp.Index, exp.ItemType);
    }

    ResolvedExp IIntermediateExpVisitor<ResolvedExp>.VisitLocalVar(IntermediateExp.LocalVar exp)
    {
        return new ResolvedExp.LocalVar(exp.Type, exp.Name);
    }

    ResolvedExp IIntermediateExpVisitor<ResolvedExp>.VisitNamespace(IntermediateExp.Namespace exp)
    {
        return Fatal(A2013_ResolveIdentifier_CantUseNamespaceAsExpression);
    }

    ResolvedExp IIntermediateExpVisitor<ResolvedExp>.VisitStruct(IntermediateExp.Struct exp)
    {
        return Fatal(A2008_ResolveIdentifier_CantUseTypeAsExpression);
    }

    ResolvedExp IIntermediateExpVisitor<ResolvedExp>.VisitStructMemberFuncs(IntermediateExp.StructMemberFuncs exp)
    {
        // funcs가 한개이면, lambda (boxed lambda)로 변환할 수 있다.
        throw new NotImplementedException();
    }

    ResolvedExp IIntermediateExpVisitor<ResolvedExp>.VisitStructMemberVar(IntermediateExp.StructMemberVar exp)
    {
        return new ResolvedExp.StructMemberVar(exp.Symbol, exp.HasExplicitInstance, exp.ExplicitInstance);
    }

    ResolvedExp IIntermediateExpVisitor<ResolvedExp>.VisitThis(IntermediateExp.ThisVar exp)
    {
        return new ResolvedExp.ThisVar(exp.Type);
    }

    ResolvedExp IIntermediateExpVisitor<ResolvedExp>.VisitTypeVar(IntermediateExp.TypeVar exp)
    {
        return Fatal(A2008_ResolveIdentifier_CantUseTypeAsExpression);
    }
}
