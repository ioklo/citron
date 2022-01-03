using Gum.Infra;
using R = Gum.IR0;
using Pretune;
using Gum.Collections;
using System;
using Gum.Analysis;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        [ImplementIEquatable]
        partial class StructConstructorContext : ICallableContext
        {
            R.Path.Nested path;
            StructSymbol structTypeValue;
            ImmutableArray<R.CallableMemberDecl> callableMemberDecls;
            AnonymousIdComponent AnonymousIdComponent;

            public StructConstructorContext(R.Path.Nested path, StructSymbol structTypeValue)
            {
                this.path = path;
                this.structTypeValue = structTypeValue;
            }

            StructConstructorContext(StructConstructorContext other, CloneContext cloneContext)
            {
                this.path = other.path;
                this.structTypeValue = other.structTypeValue;
                this.AnonymousIdComponent = other.AnonymousIdComponent;
            }

            public void AddLambdaCapture(string capturedVarName, TypeSymbol capturedVarType)
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

            public NormalTypeValue? GetThisTypeValue()
            {
                return structTypeValue;
            }

            public LocalVarInfo? GetLocalVarOutsideLambda(string varName)
            {
                return null;
            }            

            public TypeSymbol? GetRetTypeValue()
            {
                return VoidTypeValue.Instance;
            }

            public bool IsSeqFunc()
            {
                return false;
            }

            public void SetRetTypeValue(TypeSymbol retTypeValue)
            {
                throw new UnreachableCodeException();
            }

            public void AddCallableMemberDecl(R.CallableMemberDecl decl) { callableMemberDecls = callableMemberDecls.Add(decl); }
            public ImmutableArray<R.CallableMemberDecl> GetCallableMemberDecls() { return callableMemberDecls; }
            public R.Name.Anonymous NewAnonymousName() { return AnonymousIdComponent.NewAnonymousName(); }
        }
    }
}