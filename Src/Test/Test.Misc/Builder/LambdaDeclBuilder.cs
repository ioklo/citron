using Citron.Infra;
using Citron.Collections;
using Citron.Module;
using Citron.Symbol;

namespace Citron.Test.Misc
{
    public class LambdaDeclBuilder<TOuterDeclBuilder>
    {
        internal delegate LambdaDeclSymbol OnFinish(ImmutableArray<LambdaMemberVarDeclSymbol> memberVarDecls);

        TOuterDeclBuilder outerBuilder;
        OnFinish onFinish;

        Holder<LambdaDeclSymbol> lambdaDeclHolder;
        ImmutableArray<LambdaMemberVarDeclSymbol>.Builder builder;

        internal LambdaDeclBuilder(TOuterDeclBuilder outerBuilder, OnFinish onFinish)
        {
            this.outerBuilder = outerBuilder;
            this.onFinish = onFinish;

            this.lambdaDeclHolder = new Holder<LambdaDeclSymbol>();
            this.builder = ImmutableArray.CreateBuilder<LambdaMemberVarDeclSymbol>();
        }

        public LambdaDeclBuilder<TOuterDeclBuilder> MemberVar(ITypeSymbol declType, string name, out LambdaMemberVarDeclSymbol memberVarDecl)
        {
            memberVarDecl = new LambdaMemberVarDeclSymbol(lambdaDeclHolder, declType, new Name.Normal(name));
            builder.Add(memberVarDecl);
            return this;
        }

        public TOuterDeclBuilder EndLambda(out LambdaDeclSymbol lambdaDecl)
        {
            lambdaDecl = onFinish.Invoke(builder.ToImmutable());
            lambdaDeclHolder.SetValue(lambdaDecl);
            return outerBuilder;
        }
    }
}
