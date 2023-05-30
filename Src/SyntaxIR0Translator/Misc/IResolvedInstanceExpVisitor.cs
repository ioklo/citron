namespace Citron.Analysis;

interface IResolvedInstanceExpVisitor<TResult>
{
    TResult VisitNormal(ResolvedInstanceExp.Normal reInExp);
    TResult VisitLocalDeref(ResolvedInstanceExp.LocalDeref reInExp);
    TResult VisitBoxDeref(ResolvedInstanceExp.BoxDeref reInExp);
}
