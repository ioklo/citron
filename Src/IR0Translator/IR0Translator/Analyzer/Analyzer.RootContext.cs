using Citron.Collections;
using Citron.Infra;
using System;
using R = Citron.IR0;
using M = Citron.CompileTime;
using Citron.Analysis;

namespace Citron.IR0Translator
{
    partial class Analyzer
    {
        // 최상위 레벨 컨텍스트
        class RootContext : ICallableContext, ITypeContainer
        {
            ModuleDeclSymbol moduleDecl;
            SymbolFactory symbolFactory;
            ImmutableArray<R.Stmt> topLevelStmts;

            AnonymousIdComponent AnonymousIdComponent;

            public RootContext(ModuleDeclSymbol moduleDecl, SymbolFactory symbolFactory)
            {
                this.moduleDecl = moduleDecl;
                this.symbolFactory = symbolFactory;
                this.topLevelStmts = ImmutableArray<R.Stmt>.Empty;
            }
            
            public RootContext(RootContext other, CloneContext cloneContext)
            {
                this.moduleDecl = other.moduleDecl;
                this.symbolFactory = other.symbolFactory;
                this.topLevelStmts = other.topLevelStmts;
            }

            public R.Path.Normal GetPath()
            {
                return new R.Path.Root(moduleName);
            }

            public NormalTypeValue? GetThisType()
            {
                return null;
            }

            // moduleName, itemValueFactory
            // Clone호출하는 쪽에서 ItemValueFactory를 어떻게 찾는가!
            ICallableContext IMutable<ICallableContext>.Clone(CloneContext context)
            {
                return new RootContext(this, context);
            }

            void IMutable<ICallableContext>.Update(ICallableContext src_callableContext, UpdateContext context)
            {
                var src = (RootContext)src_callableContext;

                this.symbolFactory = src.symbolFactory;
                this.topLevelStmts = src.topLevelStmts;
            }

            public LocalVarInfo? GetLocalVarOutsideLambda(string varName) => null;
            public ITypeSymbol? GetReturn() => symbolFactory.Int;            
            public void SetRetType(ITypeSymbol retTypeValue) => throw new UnreachableCodeException();
            public void AddLambdaCapture(string capturedVarName, ITypeSymbol capturedVarType) => throw new UnreachableCodeException();
            public bool IsSeqFunc() => false;

            public void AddTopLevelStmt(R.Stmt stmt)
            {
                topLevelStmts = topLevelStmts.Add(stmt);
            }

            public R.Script MakeScript()
            {
                stmtBodiesBuilder.Add(M.Name.TopLevel, topLevelStmts);
                return new R.Script(moduleDeclSymbol, stmtBodiesBuilder.ToImmutable());
            }

            public void AddGlobalFuncDecl(R.FuncDecl globalFuncDecl)
            {
                globalFuncDecls = globalFuncDecls.Add(globalFuncDecl);
            }

            public void AddGlobalTypeDecl(R.TypeDecl globalTypeDecl)
            {
                throw new NotImplementedException();
            }

            public void AddCallableMemberDecl(R.CallableMemberDecl decl)
            {
                callableMemberDecls = callableMemberDecls.Add(decl);
            }

            public ImmutableArray<R.CallableMemberDecl> GetCallableMemberDecls() => callableMemberDecls;

            public R.Name.Anonymous NewAnonymousName() => AnonymousIdComponent.NewAnonymousName();

            public RootItemValueOuter MakeRootItemValueOuter(M.NamespacePath namespacePath)
            {
                return new RootItemValueOuter(moduleName.Value, namespacePath);
            }

            void ITypeContainer.AddType(R.TypeDecl typeDecl)
            {
                globalTypeDecls = globalTypeDecls.Add(typeDecl);
            }
        }
    }
}