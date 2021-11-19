using Gum.Analysis;
using Gum.Collections;
using Gum.Infra;
using System.Diagnostics;
using R = Gum.IR0;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        class FuncContext : ICallableContext
        {
            NormalTypeValue? thisTypeValue; // global일때는 null, memberfunc의 경우 static이더라도 outer를 알아야 하기 때문에 null이 아니어야 한다
            TypeValue? retTypeValue; // 리턴 타입이 미리 정해져 있다면 이걸 쓴다
            bool bStatic;            // static call 
            bool bSequence;          // 시퀀스 여부
            R.Path.Nested path;
            ImmutableArray<R.CallableMemberDecl> callableMemberDecls;
            AnonymousIdComponent AnonymousIdComponent;

            public FuncContext(NormalTypeValue? thisTypeValue, TypeValue? retTypeValue, bool bStatic, bool bSequence, R.Path.Nested path)
            {
                // static 함수가 아니면, 전역 함수
                Debug.Assert(bStatic || thisTypeValue != null); // static 함수가 아닌데, 전역 함수일수는 없다

                this.thisTypeValue = thisTypeValue;
                this.retTypeValue = retTypeValue;
                this.bStatic = bStatic;
                this.bSequence = bSequence;
                this.path = path;
                this.callableMemberDecls = ImmutableArray<R.CallableMemberDecl>.Empty;
            }

            public void AddCallableMemberDecl(R.CallableMemberDecl decl)
            {
                callableMemberDecls = callableMemberDecls.Add(decl);
            }

            public ImmutableArray<R.CallableMemberDecl> GetCallableMemberDecls()
            {
                return callableMemberDecls;
            }

            public FuncContext(FuncContext other, CloneContext cloneContext)
            {
                this.retTypeValue = other.retTypeValue;
                this.bSequence = other.bSequence;
                this.path = other.path;
            }

            ICallableContext IMutable<ICallableContext>.Clone(CloneContext cloneContext)
            {
                return new FuncContext(this, cloneContext);
            }

            void IMutable<ICallableContext>.Update(ICallableContext src_funcContext, UpdateContext updateContext)
            {
                var src = (FuncContext)src_funcContext;
            }
            
            public R.Path.Normal GetPath() => path;
            public NormalTypeValue? GetThisTypeValue() { return thisTypeValue; }
            // TODO: 지금은 InnerFunc를 구현하지 않으므로, Outside가 없다. 나중에 지원
            public LocalVarInfo? GetLocalVarOutsideLambda(string varName) => null;
            public TypeValue? GetRetTypeValue() => retTypeValue;
            public void SetRetTypeValue(TypeValue retTypeValue) { this.retTypeValue = retTypeValue; }
            public void AddLambdaCapture(string capturedVarName, TypeValue capturedVarType) => throw new UnreachableCodeException();
            public bool IsSeqFunc() => bSequence;

            public R.Name.Anonymous NewAnonymousName() => AnonymousIdComponent.NewAnonymousName();
        }
    }
}