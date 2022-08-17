using Citron.Collections;
using Citron.Module;
using Citron.Infra;
using System;
using System.Diagnostics;

using R = Citron.IR0;

namespace Citron.Analysis
{
    // 지금 함수/람다의 선언, 함수는 미리 만들어 져 있을 것이고, 람다의 경우 아직 완전하지 않다.        
    class BodyContext : IMutable<BodyContext>
    {
        // 리턴값이 설정 되어 있으면 true, 아직 모르면(lambda) false
        bool bSetReturn;
        FuncReturn? funcReturn; // bSetReturn이 true일 경우만 유효, constructor일 경우 null

        bool bSeqFunc;
        BodyContextLambdaComponent lambdaComponent;

        public BodyContext(bool bSeqFunc, BodyContextLambdaComponent lambdaComponent)
        {
            this.bSetReturn = false;
            this.funcReturn = null;
            this.bSeqFunc = bSeqFunc;
            this.lambdaComponent = lambdaComponent;
        }

        public BodyContext(FuncReturn funcReturn, bool bSeqFunc, BodyContextLambdaComponet lambdaComponent)
        {
            this.bSetReturn = true;
            this.funcReturn = funcReturn;
            this.bSeqFunc = bSeqFunc;
            this.lambdaComponent = lambdaComponent;
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
        

        // 시퀀스 함수인가
        public bool IsSeqFunc()
        {
            return bSeqFunc;
        }

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