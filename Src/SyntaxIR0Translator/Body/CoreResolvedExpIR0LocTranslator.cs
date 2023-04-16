using Citron.IR0;
using System;

using ISyntaxNode = Citron.Syntax.ISyntaxNode;

namespace Citron.Analysis;

struct CoreResolvedExpIR0LocTranslator
{
    ScopeContext context;
    ISyntaxNode nodeForErrorReport;

    public CoreResolvedExpIR0LocTranslator(ScopeContext context, ISyntaxNode nodeForErrorReport)
    {
        this.context = context;
        this.nodeForErrorReport = nodeForErrorReport;
    }

    public Loc TranslateThis(ResolvedExp.ThisVar reExp)
    {
        return new ThisLoc();
    }

    public Loc TranslateClassMemberVar(ResolvedExp.ClassMemberVar reExp)
    {
        if (reExp.HasExplicitInstance) // c.x, C.x 둘다 해당
        {
            Loc? instance = null;
            if (reExp.ExplicitInstance != null)
            {
                instance = ResolvedExpIR0LocTranslator.Translate(reExp.ExplicitInstance, context, bWrapExpAsLoc: true, nodeForErrorReport);
            }

            return new ClassMemberLoc(instance, reExp.Symbol);
        }
        else // x, x (static) 둘다 해당
        {
            var instanceLoc = reExp.Symbol.IsStatic() ? null : new ThisLoc();
            return new ClassMemberLoc(instanceLoc, reExp.Symbol);
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

    public Loc TranslateStructMemberVar(ResolvedExp.StructMemberVar reExp)
    {
        if (reExp.HasExplicitInstance) // c.x, C.x 둘다 해당
        {
            Loc? instance = null;

            if (reExp.ExplicitInstance != null)
            {
                instance = ResolvedExpIR0LocTranslator.Translate(reExp.ExplicitInstance, context, bWrapExpAsLoc: true, nodeForErrorReport);
            }

            return new StructMemberLoc(instance, reExp.Symbol);
        }
        else // x, x (static) 둘다 해당
        {
            var instanceLoc = reExp.Symbol.IsStatic() ? null : new ThisLoc();
            return new StructMemberLoc(instanceLoc, reExp.Symbol);
        }
    }

    public Loc TranslateEnumElemMemberVar(ResolvedExp.EnumElemMemberVar reExp)
    {
        var instance = ResolvedExpIR0LocTranslator.Translate(reExp.Instance, context, bWrapExpAsLoc: true, nodeForErrorReport);
        return new EnumElemMemberLoc(instance, reExp.Symbol);
    }

    public Loc TranslateListIndexer(ResolvedExp.ListIndexer reExp)
    {
        return new ListIndexerLoc(reExp.Instance, reExp.Index);
    }
}

