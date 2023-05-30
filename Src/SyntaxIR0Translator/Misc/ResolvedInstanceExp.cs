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

    public record class LocalDeref(ResolvedExp InnerExp, IType Type) : ResolvedInstanceExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLocalDeref(this);
        public override IType GetExpType() => Type;
    }

    public record class BoxDeref(ResolvedExp InnerExp, IType Type) : ResolvedInstanceExp
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitBoxDeref(this);
        public override IType GetExpType() => Type;
    }

    public static ResolvedInstanceExp Make(ResolvedExp reExp)
    {
        var type = reExp.GetExpType();

        switch(type)
        {
            case LocalRefType localRefType:
                return new LocalDeref(reExp, localRefType.InnerType);

            case BoxRefType boxRefType:
                return new BoxDeref(reExp, boxRefType.InnerType);

            default:
                return new Normal(reExp);
        }
    }
}
