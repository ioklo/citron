using Citron.Infra;
using R = Citron.IR0;
using Pretune;
using Citron.Collections;
using System;
using Citron.Analysis;
using System.Diagnostics;

namespace Citron.IR0Translator
{
    partial class Analyzer
    {
        [ImplementIEquatable]
        partial class ClassConstructorContext : ICallableContext
        {   
            ClassConstructorSymbol symbol;
            ImmutableArray<R.CallableMemberDecl> callableMemberDecls;
            AnonymousIdComponent AnonymousIdComponent;

            public ClassConstructorContext(ClassConstructorSymbol symbol)
            {
                this.symbol = symbol;
            }

            ClassConstructorContext(ClassConstructorContext other, CloneContext cloneContext)
            {
                this.symbol = other.symbol;
                this.callableMemberDecls = other.callableMemberDecls;
                this.AnonymousIdComponent = other.AnonymousIdComponent;
            }

            public void AddLambdaCapture(string capturedVarName, ITypeSymbol capturedVarType)
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

            public R.Path.Nested MakeRPath()
            {
                var nestedPath = symbol.MakeRPath() as R.Path.Nested;
                Debug.Assert(nestedPath != null);

                return nestedPath;
            }

            public LocalVarInfo? GetLocalVarOutsideLambda(string varName)
            {
                return null;
            }            

            public FuncReturn? GetReturn()
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

            public void AddCallableMemberDecl(R.CallableMemberDecl decl) 
            { 
                callableMemberDecls = callableMemberDecls.Add(decl); 
            }

            public ImmutableArray<R.CallableMemberDecl> GetCallableMemberDecls() 
            { 
                return callableMemberDecls; 
            }

            public R.Name.Anonymous NewAnonymousName() 
            { 
                return AnonymousIdComponent.NewAnonymousName(); 
            }

            public ITypeSymbol? GetThisType()
            {
                return symbol.GetOuterType();
            }
        }
    }
}