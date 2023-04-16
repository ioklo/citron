using Citron.Symbol;
using R = Citron.IR0;

namespace Citron.Analysis
{
    // 기본적으로 load를 한다
    struct ResolvedExpIR0ExpTranslator : IResolvedExpVisitor<R.Exp>
    {
        CoreResolvedExpIR0LocTranslator coreLocTranslator;

        public static R.Exp Translate(ResolvedExp reExp)
        {
            var translator = new ResolvedExpIR0ExpTranslator { coreLocTranslator  = new CoreResolvedExpIR0LocTranslator() };
            return reExp.Accept<ResolvedExpIR0ExpTranslator, R.Exp>(ref translator);
        }

        R.Exp HandleLoc(R.Loc loc, IType type)
        {
            if (type is LocalRefType localRefType)
                return new R.LoadExp(new R.LocalDerefLoc(loc), localRefType.InnerType);
            else if (type is BoxRefType boxRefType)
                return new R.LoadExp(new R.BoxDerefLoc(loc), boxRefType.InnerType);
            else
                return new R.LoadExp(loc, type);
        }

        R.Exp IResolvedExpVisitor<R.Exp>.VisitClassMemberVar(ResolvedExp.ClassMemberVar reExp)
        {
            var loc = coreLocTranslator.TranslateClassMemberVar(reExp);
            return HandleLoc(loc, reExp.Symbol.GetDeclType());
        }

        R.Exp IResolvedExpVisitor<R.Exp>.VisitEnumElemMemberVar(ResolvedExp.EnumElemMemberVar reExp)
        {
            var loc = coreLocTranslator.TranslateEnumElemMemberVar(reExp);
            return HandleLoc(loc, reExp.Symbol.GetDeclType());
        }

        R.Exp IResolvedExpVisitor<R.Exp>.VisitIR0Exp(ResolvedExp.IR0Exp reExp)
        {
            return reExp.Exp;
        }

        R.Exp IResolvedExpVisitor<R.Exp>.VisitLambdaMemberVar(ResolvedExp.LambdaMemberVar reExp)
        {
            var loc = coreLocTranslator.TranslateLambdaMemberVar(reExp);
            return HandleLoc(loc, reExp.Symbol.GetDeclType());
        }

        R.Exp IResolvedExpVisitor<R.Exp>.VisitListIndexer(ResolvedExp.ListIndexer reExp)
        {
            var loc = coreLocTranslator.TranslateListIndexer(reExp);
            return HandleLoc(loc, reExp.ItemType);
        }

        R.Exp IResolvedExpVisitor<R.Exp>.VisitLocalVar(ResolvedExp.LocalVar reExp)
        {
            var loc = coreLocTranslator.TranslateLocalVar(reExp);
            return HandleLoc(loc, reExp.Type);
        }

        R.Exp IResolvedExpVisitor<R.Exp>.VisitStructMemberVar(ResolvedExp.StructMemberVar reExp)
        {
            var loc = coreLocTranslator.TranslateStructMemberVar(reExp);
            return HandleLoc(loc, reExp.Symbol.GetDeclType());
        }

        R.Exp IResolvedExpVisitor<R.Exp>.VisitThis(ResolvedExp.ThisVar reExp)
        {
            var loc = coreLocTranslator.TranslateThis(reExp);
            return HandleLoc(loc, reExp.Type);
        }
    }
}