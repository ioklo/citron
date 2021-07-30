using Gum.Collections;
using Gum.Infra;
using R = Gum.IR0;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        class FuncContext : ICallableContext
        {
            NormalTypeValue? thisTypeValue;
            TypeValue? retTypeValue; // 리턴 타입이 미리 정해져 있다면 이걸 쓴다
            bool bSequence;          // 시퀀스 여부
            R.Path.Nested path;
            ImmutableArray<R.Decl> decls;
            AnonymousIdComponent AnonymousIdComponent;

            public FuncContext(NormalTypeValue? thisTypeValue, TypeValue? retTypeValue, bool bSequence, R.Path.Nested path)
            {
                this.thisTypeValue = thisTypeValue;
                this.retTypeValue = retTypeValue;
                this.bSequence = bSequence;
                this.path = path;
                this.decls = ImmutableArray<R.Decl>.Empty;
            }

            public void AddDecl(R.Decl decl)
            {
                decls = decls.Add(decl);
            }

            public ImmutableArray<R.Decl> GetDecls()
            {
                return decls;
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