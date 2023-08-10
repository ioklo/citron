using Citron.IR0;
using ISyntaxNode = Citron.Syntax.ISyntaxNode;
using static Citron.Analysis.SyntaxAnalysisErrorCode;
using System;

namespace Citron.Analysis;

// 최종
struct IntermediateRefExpIR0ExpTranslator : IIntermediateRefExpVisitor<TranslationResult<Exp>>
{
    ScopeContext context;
    ISyntaxNode nodeForErrorReport;

    public static TranslationResult<Exp> Translate(IntermediateRefExp imRefExp, ScopeContext context, ISyntaxNode nodeForErrorReport)
    {
        var translator = new IntermediateRefExpIR0ExpTranslator { context = context, nodeForErrorReport = nodeForErrorReport };
        return imRefExp.Accept<IntermediateRefExpIR0ExpTranslator, TranslationResult<Exp>>(ref translator);
    }

    static TranslationResult<Exp> Valid(Exp exp)
    {
        return TranslationResult.Valid(exp);
    }

    TranslationResult<Exp> Fatal(SyntaxAnalysisErrorCode code)
    {
        context.AddFatalError(code, nodeForErrorReport);
        return TranslationResult.Error<Exp>();
    }

    // box S* pS = ...
    // &(*pS)
    TranslationResult<Exp> IIntermediateRefExpVisitor<TranslationResult<Exp>>.VisitDerefedBoxValue(IntermediateRefExp.DerefedBoxValue imRefExp)
    {
        // Warning / Fatal..
        return Fatal(A3003_Reference_UselessDereferenceReferencedValue);
    }

    // &C
    TranslationResult<Exp> IIntermediateRefExpVisitor<TranslationResult<Exp>>.VisitClass(IntermediateRefExp.Class imRefExp)
    {
        return Fatal(A3001_Reference_CantMakeReference);
    }

    // &E
    TranslationResult<Exp> IIntermediateRefExpVisitor<TranslationResult<Exp>>.VisitEnum(IntermediateRefExp.Enum imRefExp)
    {
        return Fatal(A3001_Reference_CantMakeReference);
    }

    // &NS
    TranslationResult<Exp> IIntermediateRefExpVisitor<TranslationResult<Exp>>.VisitNamespace(IntermediateRefExp.Namespace imRefExp)
    {
        return Fatal(A3001_Reference_CantMakeReference);
    }

    // &S
    TranslationResult<Exp> IIntermediateRefExpVisitor<TranslationResult<Exp>>.VisitStruct(IntermediateRefExp.Struct imRefExp)
    {
        return Fatal(A3001_Reference_CantMakeReference);
    }

    // &T
    TranslationResult<Exp> IIntermediateRefExpVisitor<TranslationResult<Exp>>.VisitTypeVar(IntermediateRefExp.TypeVar imRefExp)
    {
        return Fatal(A3001_Reference_CantMakeReference);
    }

    // &C.x
    TranslationResult<Exp> IIntermediateRefExpVisitor<TranslationResult<Exp>>.VisitStaticRef(IntermediateRefExp.StaticRef imRefExp)
    {
        throw new NotImplementedException();
    }

    // &c.x
    TranslationResult<Exp> IIntermediateRefExpVisitor<TranslationResult<Exp>>.VisitBoxRef(IntermediateRefExp.BoxRef imRefExp)
    {
        // box ref는 
        throw new NotImplementedException();
    }

    // 가장 쉬운 &s.x
    TranslationResult<Exp> IIntermediateRefExpVisitor<TranslationResult<Exp>>.VisitLocalRef(IntermediateRefExp.LocalRef imRefExp)
    {
        return Valid(new LocalRefExp(imRefExp.Loc, imRefExp.LocType));
    }

    // &G()
    TranslationResult<Exp> IIntermediateRefExpVisitor<TranslationResult<Exp>>.VisitLocalValue(IntermediateRefExp.LocalValue imRefExp)
    {
        return Fatal(A3002_Reference_CantReferenceTempValue);
    }

    // &this, this는 특수 키워드이고, local storage에 속하지 않는다. 에러를 내도록 한다
    TranslationResult<Exp> IIntermediateRefExpVisitor<TranslationResult<Exp>>.VisitThis(IntermediateRefExp.ThisVar imRefExp)
    {
        return Fatal(A3004_Reference_CantReferenceThis);
    }
}

