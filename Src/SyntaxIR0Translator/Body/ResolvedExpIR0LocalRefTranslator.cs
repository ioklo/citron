using System.Diagnostics;

using Citron.IR0;
using Citron.Symbol;
using static Citron.Analysis.SyntaxAnalysisErrorCode;

using ISyntaxNode = Citron.Syntax.ISyntaxNode;

namespace Citron.Analysis;

struct ResolvedExpIR0LocalRefTranslator : IResolvedExpVisitor<TranslationResult<Exp>>
{
    ScopeContext context;
    ISyntaxNode nodeForErrorReport;

    public static TranslationResult<Exp> Translate(ResolvedExp reExp, ScopeContext context, ISyntaxNode nodeForErrorReport)
    {
        var translator = new ResolvedExpIR0LocalRefTranslator() { context = context, nodeForErrorReport = nodeForErrorReport };
        return reExp.Accept<ResolvedExpIR0LocalRefTranslator, TranslationResult<Exp>>(ref translator);
    }

    static TranslationResult<Exp> Valid(Exp exp)
    {
        return TranslationResult.Valid(exp);
    }

    static TranslationResult<Exp> Error()
    {
        return TranslationResult.Error<Exp>();
    }

    TranslationResult<Exp> HandleLoc(Loc loc, IType type)
    {
        if (type is LocalRefType)
        {
            // class C { public int& a; } var c = new C(...); var& x = c.a;
            return Valid(new LoadExp(loc, type));
        }
        else if (type is BoxRefType boxRefType)
        {
            // class C { public box int& a; } var c = new C(...); var& x = c.a;
            // box - local conversion
            return Valid(new CastBoxRefToLocalRefExp(new LoadExp(loc, type), new LocalRefType(boxRefType.InnerType)));
        }
        else
        {
            // class C { public int a; } var c = new C(...); var& x = c.a;
            return Valid(new LocalRefExp(loc, type));
        }
    }

    TranslationResult<Exp> HandleLocTranslationResult(TranslationResult<Loc> locResult, IType type)
    {
        if (!locResult.IsValid(out var loc))
            return Error();

        return HandleLoc(loc, type);
    }

    TranslationResult<Exp> IResolvedExpVisitor<TranslationResult<Exp>>.VisitIR0Exp(ResolvedExp.IR0Exp reExp)
    {
        // var& x = F();
        var expType = reExp.GetExpType();
        if (expType is LocalRefType)
        {
            return Valid(reExp.Exp);
        }
        else if (expType is BoxRefType)
        {
            // 이 에러체크는 원래 다른곳에서 하는데, 여기선 에러가 확실하기 때문에
            context.AddFatalError(A3007_LocalRef_CantMakeLocalRefWithTemporalBoxRef, nodeForErrorReport);
            return Error();
        }
        else
        {
            context.AddFatalError(A3008_LocalRef_CantMakeLocalRefWithTemporalLocation, nodeForErrorReport);
            return Error();
        }
    }

    // var& x = exp.a; // class member var
    TranslationResult<Exp> IResolvedExpVisitor<TranslationResult<Exp>>.VisitClassMemberVar(ResolvedExp.ClassMemberVar reExp)
    {
        var translator = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport);
        var locResult = translator.TranslateClassMemberVar(reExp); // Load없이 바로

        return HandleLocTranslationResult(locResult, reExp.GetExpType());

        //var expType = reExp.GetExpType();
        //if (expType is LocalRefType) 
        //{
        //    // class C { public int& a; } var c = new C(...); var& x = c.a;

        //    var translator = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport);
        //    var loc = translator.TranslateClassMemberVar(reExp); // Load없이 바로
        //    return new LoadExp(loc, expType);
        //}
        //else if (expType is BoxRefType boxRefType) 
        //{
        //    // class C { public box int& a; } var c = new C(...); var& x = c.a;
        //    var translator = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport);
        //    var loc = translator.TranslateClassMemberVar(reExp); // Load없이 바로

        //    // box - local conversion
        //    return new CastBoxRefToLocalRefExp(new LoadExp(loc, expType), new LocalRefType(boxRefType.InnerType));
        //}
        //else 
        //{
        //    // class C { public int a; } var c = new C(...); var& x = c.a;
        //    var translator = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport);
        //    var loc = translator.TranslateClassMemberVar(reExp); // Load없이 바로

        //    return new LocalRefExp(loc, expType);
        //}
    }

    TranslationResult<Exp> IResolvedExpVisitor<TranslationResult<Exp>>.VisitEnumElemMemberVar(ResolvedExp.EnumElemMemberVar reExp)
    {
        var translator = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport);
        var locResult = translator.TranslateEnumElemMemberVar(reExp); // Load없이 바로
        return HandleLocTranslationResult(locResult, reExp.GetExpType());
    }

    TranslationResult<Exp> IResolvedExpVisitor<TranslationResult<Exp>>.VisitLambdaMemberVar(ResolvedExp.LambdaMemberVar reExp)
    {
        var translator = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport);
        var loc = translator.TranslateLambdaMemberVar(reExp); // Load없이 바로
        return HandleLoc(loc, reExp.GetExpType());
    }

    TranslationResult<Exp> IResolvedExpVisitor<TranslationResult<Exp>>.VisitListIndexer(ResolvedExp.ListIndexer reExp)
    {
        var translator = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport);
        var locResult = translator.TranslateListIndexer(reExp); // Load없이 바로
        return HandleLocTranslationResult(locResult, reExp.GetExpType());
    }

    TranslationResult<Exp> IResolvedExpVisitor<TranslationResult<Exp>>.VisitLocalVar(ResolvedExp.LocalVar reExp)
    {
        var translator = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport);
        var loc = translator.TranslateLocalVar(reExp); // Load없이 바로
        return HandleLoc(loc, reExp.GetExpType());
    }

    TranslationResult<Exp> IResolvedExpVisitor<TranslationResult<Exp>>.VisitStructMemberVar(ResolvedExp.StructMemberVar reExp)
    {
        var translator = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport);
        var locResult = translator.TranslateStructMemberVar(reExp); // Load없이 바로
        return HandleLocTranslationResult(locResult, reExp.GetExpType());
    }

    TranslationResult<Exp> IResolvedExpVisitor<TranslationResult<Exp>>.VisitThis(ResolvedExp.ThisVar reExp)
    {
        var translator = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport);
        var loc = translator.TranslateThis(reExp); // Load없이 바로
        return HandleLoc(loc, reExp.GetExpType());
    }
}
