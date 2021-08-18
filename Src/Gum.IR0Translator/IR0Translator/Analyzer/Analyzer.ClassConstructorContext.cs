using Gum.Infra;
using R = Gum.IR0;
using Pretune;
using Gum.Collections;
using System;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        [ImplementIEquatable]
        partial class ClassConstructorContext : ICallableContext
        {
            R.Path.Nested path;
            ClassTypeValue classTypeValue;
            ImmutableArray<R.CallableMemberDecl> callableMemberDecls;
            AnonymousIdComponent AnonymousIdComponent;

            public ClassConstructorContext(R.Path.Nested path, ClassTypeValue classTypeValue)
            {
                this.path = path;
                this.classTypeValue = classTypeValue;
            }

            ClassConstructorContext(ClassConstructorContext other, CloneContext cloneContext)
            {
                this.path = other.path;
                this.classTypeValue = other.classTypeValue;
                this.AnonymousIdComponent = other.AnonymousIdComponent;
            }

            public void AddLambdaCapture(string capturedVarName, TypeValue capturedVarType)
            {
                throw new UnreachableCodeException();
            }

            ICallableContext IMutable<ICallableContext>.Clone(CloneContext context)
            {
                return new ClassConstructorContext(this, context);
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
                return classTypeValue;
            }

            public LocalVarInfo? GetLocalVarOutsideLambda(string varName)
            {
                return null;
            }            

            public TypeValue? GetRetTypeValue()
            {
                return VoidTypeValue.Instance;
            }

            public bool IsSeqFunc()
            {
                return false;
            }

            public void SetRetTypeValue(TypeValue retTypeValue)
            {
                throw new UnreachableCodeException();
            }

            public void AddCallableMemberDecl(R.CallableMemberDecl decl) { callableMemberDecls = callableMemberDecls.Add(decl); }
            public ImmutableArray<R.CallableMemberDecl> GetCallableMemberDecls() { return callableMemberDecls; }
            public R.Name.Anonymous NewAnonymousName() { return AnonymousIdComponent.NewAnonymousName(); }
        }
    }
}