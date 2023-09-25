using Citron.IR0;
using ISyntaxNode = Citron.Syntax.ISyntaxNode;
using static Citron.Analysis.SyntaxAnalysisErrorCode;
using System;
using Citron.Symbol;

namespace Citron.Analysis;

// 최종
struct IntermediateRefExpIR0ExpTranslator : IIntermediateRefExpVisitor<TranslationResult<IR0ExpResult>>
{
    ScopeContext context;
    ISyntaxNode nodeForErrorReport;

    public static TranslationResult<IR0ExpResult> Translate(IntermediateRefExp imRefExp, ScopeContext context, ISyntaxNode nodeForErrorReport)
    {
        var translator = new IntermediateRefExpIR0ExpTranslator { context = context, nodeForErrorReport = nodeForErrorReport };
        return imRefExp.Accept<IntermediateRefExpIR0ExpTranslator, TranslationResult<IR0ExpResult>>(ref translator);
    }

    static TranslationResult<IR0ExpResult> Valid(IR0ExpResult expResult)
    {
        return TranslationResult.Valid(expResult);
    }

    TranslationResult<IR0ExpResult> Fatal(SyntaxAnalysisErrorCode code)
    {
        context.AddFatalError(code, nodeForErrorReport);
        return TranslationResult.Error<IR0ExpResult>();
    }

    // box S* pS = ...
    // &(*pS)
    TranslationResult<IR0ExpResult> IIntermediateRefExpVisitor<TranslationResult<IR0ExpResult>>.VisitDerefedBoxValue(IntermediateRefExp.DerefedBoxValue imRefExp)
    {
        // Warning / Fatal..
        return Fatal(A3003_Reference_UselessDereferenceReferencedValue);
    }

    // &C
    TranslationResult<IR0ExpResult> IIntermediateRefExpVisitor<TranslationResult<IR0ExpResult>>.VisitClass(IntermediateRefExp.Class imRefExp)
    {
        return Fatal(A3001_Reference_CantMakeReference);
    }

    // &E
    TranslationResult<IR0ExpResult> IIntermediateRefExpVisitor<TranslationResult<IR0ExpResult>>.VisitEnum(IntermediateRefExp.Enum imRefExp)
    {
        return Fatal(A3001_Reference_CantMakeReference);
    }

    // &NS
    TranslationResult<IR0ExpResult> IIntermediateRefExpVisitor<TranslationResult<IR0ExpResult>>.VisitNamespace(IntermediateRefExp.Namespace imRefExp)
    {
        return Fatal(A3001_Reference_CantMakeReference);
    }

    // &S
    TranslationResult<IR0ExpResult> IIntermediateRefExpVisitor<TranslationResult<IR0ExpResult>>.VisitStruct(IntermediateRefExp.Struct imRefExp)
    {
        return Fatal(A3001_Reference_CantMakeReference);
    }

    // &T
    TranslationResult<IR0ExpResult> IIntermediateRefExpVisitor<TranslationResult<IR0ExpResult>>.VisitTypeVar(IntermediateRefExp.TypeVar imRefExp)
    {
        return Fatal(A3001_Reference_CantMakeReference);
    }

    // &C.x
    TranslationResult<IR0ExpResult> IIntermediateRefExpVisitor<TranslationResult<IR0ExpResult>>.VisitStaticRef(IntermediateRefExp.StaticRef imRefExp)
    {
        throw new NotImplementedException();
    }

    record struct BoxRefVisitor : IntermediateRefExp.IBoxRefVisitor<IR0ExpResult>
    {
        // &c.x
        IR0ExpResult IntermediateRefExp.IBoxRefVisitor<IR0ExpResult>.VisitClassMember(IntermediateRefExp.BoxRef.ClassMember boxRef)
        { 
            return new IR0ExpResult (new ClassMemberBoxRefExp(boxRef.Loc, boxRef.Symbol), new BoxPtrType(boxRef.Symbol.GetDeclType()));
        }

        // &(*pS).x
        IR0ExpResult IntermediateRefExp.IBoxRefVisitor<IR0ExpResult>.VisitStructIndirectMember(IntermediateRefExp.BoxRef.StructIndirectMember boxRef)
        {
            return new IR0ExpResult(new StructIndirectMemberBoxRefExp(boxRef.Loc, boxRef.Symbol), new BoxPtrType(boxRef.Symbol.GetDeclType()));
        }

        // &c.x.a
        // &(box S()).x.y
        IR0ExpResult IntermediateRefExp.IBoxRefVisitor<IR0ExpResult>.VisitStructMember(IntermediateRefExp.BoxRef.StructMember boxRef)
        {
            var parentVisitor = new BoxRefVisitor();
            var parentExp = boxRef.Parent.AcceptBoxRef<BoxRefVisitor, IR0ExpResult>(ref parentVisitor);

            return new IR0ExpResult(
                new StructMemberBoxRefExp(new TempLoc(parentExp.Exp, parentExp.ExpType), boxRef.Symbol),
                new BoxPtrType(boxRef.Symbol.GetDeclType())
            );
        }
    }

    // &c.x
    TranslationResult<IR0ExpResult> IIntermediateRefExpVisitor<TranslationResult<IR0ExpResult>>.VisitBoxRef(IntermediateRefExp.BoxRef imRefExp)
    {
        var visitor = new BoxRefVisitor();
        return Valid(imRefExp.AcceptBoxRef<BoxRefVisitor, IR0ExpResult>(ref visitor));
    }

    // 가장 쉬운 &s.x
    TranslationResult<IR0ExpResult> IIntermediateRefExpVisitor<TranslationResult<IR0ExpResult>>.VisitLocalRef(IntermediateRefExp.LocalRef imRefExp)
    {
        return Valid(new IR0ExpResult(new LocalRefExp(imRefExp.Loc), new LocalPtrType(imRefExp.LocType)));
    }

    // &G()
    TranslationResult<IR0ExpResult> IIntermediateRefExpVisitor<TranslationResult<IR0ExpResult>>.VisitLocalValue(IntermediateRefExp.LocalValue imRefExp)
    {
        return Fatal(A3002_Reference_CantReferenceTempValue);
    }

    // &this, this는 특수 키워드이고, local storage에 속하지 않는다. 에러를 내도록 한다
    TranslationResult<IR0ExpResult> IIntermediateRefExpVisitor<TranslationResult<IR0ExpResult>>.VisitThis(IntermediateRefExp.ThisVar imRefExp)
    {
        return Fatal(A3004_Reference_CantReferenceThis);
    }
}

