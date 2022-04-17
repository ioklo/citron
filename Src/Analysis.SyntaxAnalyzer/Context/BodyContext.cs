using Citron.Collections;
using Citron.CompileTime;
using Citron.Infra;
using System.Diagnostics;

using R = Citron.IR0;

namespace Citron.Analysis
{
    // 지금 함수/람다의 선언, 함수는 미리 만들어 져 있을 것이고, 람다의 경우 아직 완전하지 않다.        
    abstract class BodyContext : IMutable<BodyContext>
    {
        // 리턴값이 설정 되어 있으면 true, 아직 모르면(lambda) false
        bool bSetReturn;
        FuncReturn? funcReturn; // bSetReturn이 true일 경우만 유효, constructor일 경우 null

        bool bSeqFunc;

        protected BodyContext(bool bSeqFunc)
        {
            this.bSetReturn = false;
            this.funcReturn = null;
            this.bSeqFunc = bSeqFunc;
        }

        protected BodyContext(FuncReturn funcReturn, bool bSeqFunc)
        {
            this.bSetReturn = true;
            this.funcReturn = funcReturn;
            this.bSeqFunc = bSeqFunc;
        }

        // 리턴값 관련 함수
        public bool IsSetReturn()
        {
            return bSetReturn;
        }

        // not set   (람다인 경우, 때에 따라)
        // no return (constructor인 경우)
        // 미리 정해져 있던 값
        public FuncReturn? GetReturn()
        {
            Debug.Assert(bSetReturn); // IsSetReturn을 먼저 보고 호출해야 한다
            return funcReturn;
        }

        public void SetReturn(bool bRef, ITypeSymbol retType)
        {
            Debug.Assert(bSetReturn); // IsSetReturn을 먼저 보고 호출            

            // not set이었다면 설정
            funcReturn = new FuncReturn(bRef, retType);
        }

        // 바깥쪽
        public abstract IdentifierResult ResolveIdentifierOuter(string idName, ImmutableArray<ITypeSymbol> typeArgs, ResolveHint hint, GlobalContext globalContext);

        // 시퀀스 함수인가
        public bool IsSeqFunc()
        {
            return bSeqFunc;
        }
        
        public abstract ITypeSymbol? GetThisType();
        public abstract bool CanAccess(ISymbolNode target);

        // 새로운 람다 컨텍스트 만들기        
        public abstract BodyContext NewLambdaBodyContext(LocalContext localContext);

        // 람다 관련
        public abstract LambdaMemberVarDeclSymbol MarkCaptured(IdentifierResult.Valid valid);
        public abstract bool IsLambda();
        public abstract R.Loc MakeThisLoc();

        BodyContext IMutable<BodyContext>.Clone(CloneContext context)
        {
            throw new System.NotImplementedException();
        }

        void IMutable<BodyContext>.Update(BodyContext src, UpdateContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}