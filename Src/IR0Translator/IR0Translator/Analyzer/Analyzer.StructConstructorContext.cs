using Citron.Infra;
using R = Citron.IR0;
using Pretune;
using Citron.Collections;
using System;
using Citron.Analysis;

namespace Citron.IR0Translator
{
    partial class Analyzer
    {
        [ImplementIEquatable]
        partial class StructConstructorContext : ICallableContext
        {
            R.Path.Nested path;
            StructSymbol structSymbol;
            ImmutableArray<R.CallableMemberDecl> callableMemberDecls;
            LambdaIdComponent AnonymousIdComponent;

            public StructConstructorContext(R.Path.Nested path, StructSymbol structSymbol)
            {
                this.path = path;
                this.structSymbol = structSymbol;
            }

            StructConstructorContext(StructConstructorContext other, CloneContext cloneContext)
            {
                this.path = other.path;
                this.structSymbol = other.structSymbol;
                this.AnonymousIdComponent = other.AnonymousIdComponent;
            }

            public void AddLambdaCapture(string capturedVarName, ITypeSymbol capturedVarType)
            {
                throw new UnreachableCodeException();
            }

            ICallableContext IMutable<ICallableContext>.Clone(CloneContext context)
            {
                return new StructConstructorContext(this, context);
            }

            void IMutable<ICallableContext>.Update(ICallableContext src_callableContext, UpdateContext updateContext)
            {   
            }

            public R.Path.Normal GetPath()
            {
                return path;
            }

            public ITypeSymbol? GetThisType()
            {
                return structSymbol;
            }

            public LocalVarInfo? GetLocalVarOutsideLambda(string varName)
            {
                return null;
            }            

            public ITypeSymbol? GetReturn()
            {
                return null;
            }

            public bool IsSeqFunc()
            {
                return false;
            }

            public void SetRetType(ITypeSymbol retTypeValue)
            {
                throw new UnreachableCodeException();
            }

            public void AddCallableMemberDecl(R.CallableMemberDecl decl) { callableMemberDecls = callableMemberDecls.Add(decl); }
            public ImmutableArray<R.CallableMemberDecl> GetCallableMemberDecls() { return callableMemberDecls; }
            public R.Name.Anonymous NewAnonymousName() { return AnonymousIdComponent.NewAnonymousName(); }
        }
    }
}