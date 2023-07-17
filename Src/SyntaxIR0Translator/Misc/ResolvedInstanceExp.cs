using Citron.Symbol;

namespace Citron.Analysis;

// class, struct, enum, list의 멤버 가져오기의 instance를 표현하는 (intermediateExp, resolvedExp 둘다)
abstract record class ResolvedInstanceExp
{
    public abstract TResult Accept<TVisitor, TResult>(ref TVisitor visitor)
        where TVisitor : struct, IResolvedInstanceExpVisitor<TResult>;

    public abstract IType GetExpType();

    public record class Normal(ResolvedExp Exp) : ResolvedInstanceExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitNormal(this);
        public override IType GetExpType() => Exp.GetExpType();
    }
    
    public static ResolvedInstanceExp Make(ResolvedExp reExp)
    {
        var type = reExp.GetExpType();
        return new Normal(reExp);
    }
}
