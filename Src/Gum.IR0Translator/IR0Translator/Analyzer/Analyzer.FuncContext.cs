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
            IFuncSymbol func;

            ITypeSymbol? retType;
            ImmutableArray<R.CallableMemberDecl> callableMemberDecls;
            AnonymousIdComponent AnonymousIdComponent;

            static FuncContext Make(IFuncSymbol func)
            {
                return new FuncContext(func);
            }

            FuncContext(IFuncSymbol func)
            {
                this.func = func;

                this.retType = null;
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
                this.func = other.func;
                this.callableMemberDecls = other.callableMemberDecls;
                this.retType = other.retType;
            }

            ICallableContext IMutable<ICallableContext>.Clone(CloneContext cloneContext)
            {
                return new FuncContext(this, cloneContext);
            }

            void IMutable<ICallableContext>.Update(ICallableContext src_funcContext, UpdateContext updateContext)
            {
                var src = (FuncContext)src_funcContext;
            }
            
            public R.Path GetPath() => func.MakeRPath();
            public ITypeSymbol? GetOuterType() { return func.GetOuterType(); }
            // TODO: 지금은 InnerFunc를 구현하지 않으므로, Outside가 없다. 나중에 지원
            public LocalVarInfo? GetLocalVarOutsideLambda(string varName) => null;
            public ITypeSymbol? GetReturn() => func.Get;
            public void SetRetType(ITypeSymbol retTypeValue) { this.retType = retTypeValue; }
            public void AddLambdaCapture(string capturedVarName, ITypeSymbol capturedVarType) => throw new UnreachableCodeException();
            public bool IsSeqFunc() => func.IsSequence();

            public R.Name.Anonymous NewAnonymousName() => AnonymousIdComponent.NewAnonymousName();
        }
    }
}