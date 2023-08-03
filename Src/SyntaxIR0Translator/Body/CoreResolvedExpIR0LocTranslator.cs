using System;

using Citron.IR0;
using Pretune;

using ISyntaxNode = Citron.Syntax.ISyntaxNode;
using static Citron.Analysis.SyntaxAnalysisErrorCode;
using System.ComponentModel;

namespace Citron.Analysis;

struct CoreResolvedExpIR0LocTranslator
{
    ScopeContext context;
    ISyntaxNode nodeForErrorReport;

    static TranslationResult<Loc> Valid(Loc loc)
    {
        return TranslationResult.Valid(loc);
    }

    static TranslationResult<Loc> Error()
    {
        return TranslationResult.Error<Loc>();
    }
    
    public CoreResolvedExpIR0LocTranslator(ScopeContext context, ISyntaxNode nodeForErrorReport)
    {
        this.context = context;
        this.nodeForErrorReport = nodeForErrorReport;
    }    

    public Loc TranslateThis(ResolvedExp.ThisVar reExp) // nothrow
    {
        return new ThisLoc();
    }

    public TranslationResult<Loc> TranslateClassMemberVar(ResolvedExp.ClassMemberVar reExp)
    {
        if (reExp.HasExplicitInstance) // c.x, C.x 둘다 해당
        {
            Loc? instance = null;
            if (reExp.ExplicitInstance != null)
            {   
                var instResult = ResolvedExpIR0LocTranslator.Translate(reExp.ExplicitInstance, context, bWrapExpAsLoc: true, nodeForErrorReport, A2015_ResolveIdentifier_ExpressionIsNotLocation);
                if (!instResult.IsValid(out var inst))
                    return Error();

                instance = inst.Loc;
            }

            return Valid(new ClassMemberLoc(instance, reExp.Symbol));
        }
        else // x, x (static) 둘다 해당
        {
            var instanceLoc = reExp.Symbol.IsStatic() ? null : new ThisLoc();
            return Valid(new ClassMemberLoc(instanceLoc, reExp.Symbol));
        }
    }

    public Loc TranslateLocalVar(ResolvedExp.LocalVar reExp)
    {
        return new LocalVarLoc(reExp.Name);
    }

    public Loc TranslateLambdaMemberVar(ResolvedExp.LambdaMemberVar reExp)
    {
        return new LambdaMemberVarLoc(reExp.Symbol);
    }

    public TranslationResult<Loc> TranslateStructMemberVar(ResolvedExp.StructMemberVar reExp)
    {
        if (reExp.HasExplicitInstance) // c.x, C.x 둘다 해당
        {
            Loc? instance = null;

            if (reExp.ExplicitInstance != null)
            {   
                var instanceResult = ResolvedExpIR0LocTranslator.Translate(reExp.ExplicitInstance, context, bWrapExpAsLoc: true, nodeForErrorReport, A2015_ResolveIdentifier_ExpressionIsNotLocation);
                if (!instanceResult.IsValid(out var inst))
                    return Error();

                instance = inst.Loc;
            }

            return Valid(new StructMemberLoc(instance, reExp.Symbol));
        }
        else // x, x (static) 둘다 해당
        {
            var instanceLoc = reExp.Symbol.IsStatic() ? null : new ThisLoc();
            return Valid(new StructMemberLoc(instanceLoc, reExp.Symbol));
        }
    }

    public TranslationResult<Loc> TranslateEnumElemMemberVar(ResolvedExp.EnumElemMemberVar reExp)
    {   
        var instResult = ResolvedExpIR0LocTranslator.Translate(reExp.Instance, context, bWrapExpAsLoc: true, nodeForErrorReport, A2015_ResolveIdentifier_ExpressionIsNotLocation);
        if (!instResult.IsValid(out var inst))
            return Error();

        return Valid(new EnumElemMemberLoc(inst.Loc, reExp.Symbol));
    }

    public TranslationResult<Loc> TranslateListIndexer(ResolvedExp.ListIndexer reExp)
    {
        var instanceResult = ResolvedExpIR0LocTranslator.Translate(reExp.Instance, context, bWrapExpAsLoc: true, nodeForErrorReport, A2015_ResolveIdentifier_ExpressionIsNotLocation);
        if (!instanceResult.IsValid(out var instance))
            return Error();

        return Valid(new ListIndexerLoc(instance.Loc, reExp.Index));
    }

    public TranslationResult<Loc> TranslateLocalDeref(ResolvedExp.LocalDeref reExp)
    {
        // *x, *G()
        var targetResult = ResolvedExpIR0ExpTranslator.Translate(reExp.Target, context, nodeForErrorReport);
        if (!targetResult.IsValid(out var target))
            return Error();

        return Valid(new LocalDerefLoc(target));
    }

    public TranslationResult<Loc> TranslateBoxDeref(ResolvedExp.BoxDeref reExp)
    {
        // *x, *G()
        var targetResult = ResolvedExpIR0ExpTranslator.Translate(reExp.Target, context, nodeForErrorReport);
        if (!targetResult.IsValid(out var target))
            return Error();

        return Valid(new BoxDerefLoc(target));
    }
}

