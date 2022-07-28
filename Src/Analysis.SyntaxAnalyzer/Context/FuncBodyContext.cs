using Citron.Collections;
using Citron.CompileTime;
using Citron.Infra;
using System;
using System.Diagnostics;
using R = Citron.IR0;

namespace Citron.Analysis
{
    // 함수의 본문
    class FuncBodyContext : BodyContext, IMutable<FuncBodyContext>
    {
        IFuncDeclSymbol funcDecl;
        ITypeSymbol? thisType;
        
        // 함수 안에서 선언된 람다 수집용
        ImmutableArray<LambdaDeclSymbol> lambdaDecls;
        LambdaIdComponent LambdaIdComponent;

        // 리턴타입 확정 버전
        public FuncBodyContext(IFuncDeclSymbol funcDecl, ITypeSymbol? thisType, FuncReturn funcReturn, bool bSeqFunc)
            : base(funcReturn, bSeqFunc)
        {
            this.funcDecl = funcDecl;
            this.thisType = thisType;

            this.lambdaDecls = default;
            this.LambdaIdComponent = new LambdaIdComponent();
        }
        
        public ITypeSymbol? EnsureCaptureThis()
        {
            throw new NotImplementedException();

            // 1. 일반 함수일 경우 캡쳐가 필요없다. thisType을 리턴한다            
            // 2. 람다라면 this를 캡쳐하고, 상위 람다도 this를 캡쳐해야 한다
            // TODO: this가 value type이면 작성자가 복사 캡쳐인걸 인지해야한다. 에러를 내자
            // return thisType;
        }

        public override ITypeSymbol? GetThisType()
        {
            return thisType;
        }

        // 람다 캡쳐를 추가, R.LambdaMemberVarLoc에서 쓰려고 Symbol을 리턴함
        // TODO: 여러번 캡쳐해도 한번만
        public LambdaMemberVarSymbol EnsureCaptureLocalVar(Name name, ITypeSymbol capturedVarType)
        {
            // 이 함수 컨텍스트가 람다가 아니라면 에러를 내야 한다
            throw new NotImplementedException();
        }

        public override bool CanAccess(ISymbolNode target)
        {
            var targetDecl = target.GetDeclSymbolNode();
            Debug.Assert(targetDecl != null);

            return funcDecl.CanAccess(targetDecl);
        }

        public override IdentifierResult ResolveIdentifierOuter(string idName, ImmutableArray<ITypeSymbol> typeArgs, ResolveHint hint, GlobalContext globalContext)
        {
            // outer가 없으므로
            return IdentifierResult.NotFound.Instance;
        }

        public override R.Loc MakeThisLoc()
        {
            return new R.ThisLoc();
        }

        FuncBodyContext IMutable<FuncBodyContext>.Clone(CloneContext context)
        {
            throw new NotImplementedException();
        }        

        void IMutable<FuncBodyContext>.Update(FuncBodyContext src, UpdateContext context)
        {
            throw new NotImplementedException();
        }

        public override BodyContext NewLambdaBodyContext(LocalContext localContext)
        {
            throw new NotImplementedException();
        }
        
        public override bool IsLambda()
        {
            throw new NotImplementedException();
        }

        public override bool HasOuterBodyContext()
        {
            throw new NotImplementedException();
        }

        public override LambdaMemberVarSymbol Capture(IdentifierResult.LambdaMemberVar outsideLambda)
        {
            throw new NotImplementedException();
        }
    }
}