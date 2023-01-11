using Citron.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pretune;
using Citron.Infra;
using Citron.Module;

using static Citron.Analysis.SyntaxAnalysisErrorCode;

using S = Citron.Syntax;
using R = Citron.IR0;

namespace Citron.Analysis
{
    // 어떤 Exp에서 타입 정보 등을 알아냅니다    
    partial class StmtAndExpAnalyzer 
    {
        GlobalContext globalContext;
        BodyContext bodyContext;
        LocalContext localContext;

        public StmtAndExpAnalyzer(GlobalContext globalContext, BodyContext bodyContext, LocalContext localContext)
        {
            this.globalContext = globalContext;
            this.bodyContext = bodyContext;
            this.localContext = localContext;
        }

        StmtAndExpAnalyzer NewAnalyzer()
        {
            var newLocalContext = localContext.NewLocalContext();
            return new StmtAndExpAnalyzer(globalContext, bodyContext, newLocalContext);
        }            

        StmtAndExpAnalyzer NewAnalyzerWithLoop()
        {
            var newLocalContext = localContext.NewLocalContextWithLoop();
            return new StmtAndExpAnalyzer(globalContext, bodyContext, newLocalContext);
        }

        class LocalVarDeclComponent : VarDeclComponent
        {
            GlobalContext globalContext;
            LocalContext localContext;

            ImmutableArray<R.VarDeclElement>.Builder elemsBuilder;

            public LocalVarDeclComponent(GlobalContext globalContext, LocalContext localContext, StmtAndExpAnalyzer stmtAndExpAnalyzer)
                :base(globalContext, stmtAndExpAnalyzer, bCheckLocalInitializer: true)
            {
                this.globalContext = globalContext;
                this.localContext = localContext;
            }

            public override void OnElemCreated(ITypeSymbol type, string name, S.VarDeclElement selem, R.VarDeclElement elem)
            {
                if (localContext.DoesLocalVarNameExistInScope(name))
                    globalContext.AddFatalError(A0103_VarDecl_LocalVarNameShouldBeUniqueWithinScope, selem);

                // varDecl.IsRef는 syntax에서 체크한 것이므로, syntax에서 ref가 아니더라도 ref일 수 있으므로 result.Elem으로 검사를 해야한다.
                localContext.AddLocalVarInfo(elem is R.VarDeclElement.Ref, type, new Name.Normal(name));

                elemsBuilder.Add(elem);
            }

            public override void OnCompleted()
            {   
                // do nothing
            }

            public R.LocalVarDecl Make()
            {
                return new R.LocalVarDecl(elemsBuilder.ToImmutable());
            }
        }

        // var x = 3, y = ref i; 라면 
        R.LocalVarDecl AnalyzeLocalVarDecl(S.VarDecl varDecl)
        {
            var localVarDeclComponent = new LocalVarDeclComponent(globalContext, localContext, this);
            localVarDeclComponent.AnalyzeVarDecl(varDecl);

            return localVarDeclComponent.Make();
        }

        // retType이 null이면 아직 안정해졌다는 뜻이다
        (LambdaInfo LambdaInfo, ImmutableArray<R.Argument> Args, ImmutableArray<R.Stmt> Body) AnalyzeLambda(ITypeSymbol? retType, ImmutableArray<S.LambdaExpParam> sparams, ImmutableArray<S.Stmt> body, S.ISyntaxNode nodeForErrorReport)
        {
            // 람다를 분석합니다
            // [int x = x](int p) => { return 3; }

            // 파라미터는 람다 함수의 지역변수로 취급한다
            // var newLambdaBodyContext = bodyContext.NewLambdaBodyContext(localContext); // new FuncContext(lambdaDeclHolder, bodyContext.GetThisType(), bSeqFunc: false, localContext);

            // 람다 관련 정보는 여기서 수집한다
            var lambdaComponent = new BodyContextLambdaComponent();

            // TODO: 리턴타입은 타입 힌트를 반영해야 한다. 확정 되지 않았을 수도 있다
            bool bRefReturn = false;
            BodyContext newBodyContext;
            if (retType == null)
            {
                newBodyContext = new BodyContext(bSeqFunc: false, lambdaComponent);
            }
            else
            {
                newBodyContext = new BodyContext(new FuncReturn(bRefReturn, retType), bSeqFunc: false, lambdaComponent);
            }

            var newLocalContext = new LocalContext();

            // 람다 파라미터(int p)를 지역 변수로 추가한다
            foreach (var sparam in sparams)
            {
                // TODO: 파라미터 타입은 타입 힌트를 반영해야 한다, ex) func<void, int, int> f = (x, y) => { } 일때, x, y는 int
                if (sparam.Type == null)
                    globalContext.AddFatalError(A9901_NotSupported_LambdaParameterInference, nodeForErrorReport);

                var paramType = globalContext.GetSymbolByTypeExp(sparam.Type);
                newLocalContext.AddLocalVarInfo(sparam.ParamKind == S.FuncParamKind.Ref, paramType, new Name.Normal(sparam.Name));
            }

            var newAnalyzer = new StmtAndExpAnalyzer(globalContext, newBodyContext, newLocalContext);

            // 본문 분석
            newAnalyzer.AnalyzeBody(body, nodeForErrorReport);

            // lambdaBodyContext로 람다를 만들어서 현재 bodyContext에 넣음. 이걸 밖에서 해야하나 아니면 그냥 내부에서 처리하는게 나을까
            // var lambdaDecl = newLambdaBodyContext.MakeLambda();
            // bodyContext.AddLambdaDecl(lambdaDecl);

            // 이걸로 람다가 하나 만들어진다. Symbol이 아니라(Symbol은 거의 immutable이다) 그냥 Analysis에서만 사용 가능한 정보를 준다
            var lambdaInfo = lambdaComponent.MakeLambdaInfo();
        }
    }
}