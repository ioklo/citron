namespace Citron.Analysis;

interface IResolvedInstanceExpVisitor<TResult>
{
    TResult VisitNormal(ResolvedInstanceExp.Normal reInExp);
}
