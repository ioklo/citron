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
        partial class StructConstructorContext : ICallableContext
        {
            R.Path.Nested path;
            StructTypeValue structTypeValue;
            ImmutableArray<R.Decl> decls;
            AnonymousIdComponent AnonymousIdComponent;

            public StructConstructorContext(R.Path.Nested path, StructTypeValue structTypeValue)
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

            public void AddLambdaCapture(string capturedVarName, TypeValue capturedVarType)
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

            public void AddDecl(R.Decl decl) { decls = decls.Add(decl); }
            public ImmutableArray<R.Decl> GetDecls() { return decls; }
            public R.Name.Anonymous NewAnonymousName() { return AnonymousIdComponent.NewAnonymousName(); }
        }
    }
}