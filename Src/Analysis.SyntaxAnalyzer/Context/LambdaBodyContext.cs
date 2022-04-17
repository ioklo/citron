using Citron.Collections;
using Citron.CompileTime;
using System;
using R = Citron.IR0;

namespace Citron.Analysis
{
    class LambdaBodyContext : BodyContext
    {
        BodyContext outerBodyContext;
        LocalContext outerLocalContext;

        ImmutableArray<LambdaMemberVarDeclSymbol>.Builder memberVars;

        public LambdaBodyContext(BodyContext outerBodyContext, LocalContext outerLocalContext)
            : base(bSeqFunc: false)
        {
            this.outerBodyContext = outerBodyContext;
            this.outerLocalContext = outerLocalContext;

            this.memberVars = ImmutableArray.CreateBuilder<LambdaMemberVarDeclSymbol>();
        }

        public override bool CanAccess(ISymbolNode target)
        {
            return outerBodyContext.CanAccess(target);
        }

        public override ITypeSymbol? GetThisType()
        {
            // TODO: Lambda에 명시적인 캡쳐 구문이 들어가는 경우 ThisType이 필요하다
            return null;
        }

        public override bool IsLambda()
        {
            return true;
        }

        public override IdentifierResult ResolveIdentifierOuter(string idName, ImmutableArray<ITypeSymbol> typeArgs, ResolveHint hint, GlobalContext globalContext)
        {
            return IdExpIdentifierResolver.Resolve(idName, typeArgs, hint, globalContext, outerBodyContext, outerLocalContext);
        }

        //public override LambdaMemberVarDeclSymbol MarkLocalVarCaptured(Name varName, ITypeSymbol typeSymbol)
        //{
        //    // 같은 이름이 있다면 바로 리턴
        //    foreach (var memberVar in memberVars.AsEnumerable())
        //        if (memberVar.GetName().Equals(varName))
        //            return memberVar;

        //    // 바로 겉에서 찾는다
        //    var localVarInfo = outerLocalContext.GetLocalVarInfo(varName);
        //    if (localVarInfo != null)
        //    {

        //    }

        //    // outerBodyContext.MarkLocalVarCaptured(varName, )
        //    throw new NotImplementedException();
        //}

        public override BodyContext NewLambdaBodyContext(LocalContext outerLocalContext)
        {
            return new LambdaBodyContext(this, outerLocalContext);
        }

        // 암시적 this가 나타났을 때, 
        public override R.Loc MakeThisLoc()
        {
            var thisMemberVar = bodyContext.MarkCaptured(M.Name.CapturedThis, thisType);
            return new R.LambdaMemberVarLoc(thisMemberVar);
        }

        public override LambdaMemberVarDeclSymbol MarkCaptured(IdentifierResult.Valid valid)
        {
            throw new NotImplementedException();
        }
    }
}