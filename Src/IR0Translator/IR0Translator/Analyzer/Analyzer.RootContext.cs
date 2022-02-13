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
            R.ModuleName moduleName;
            ItemValueFactory itemValueFactory;

            ImmutableArray<R.TypeDecl> globalTypeDecls;
            ImmutableArray<R.FuncDecl> globalFuncDecls;
            ImmutableArray<R.CallableMemberDecl> callableMemberDecls;
            ImmutableArray<R.Stmt> topLevelStmts;

            AnonymousIdComponent AnonymousIdComponent;

            public RootContext(R.ModuleName moduleName, ItemValueFactory itemValueFactory)
            {
                this.moduleName = moduleName;
                this.itemValueFactory = itemValueFactory;
                this.globalTypeDecls = ImmutableArray<R.TypeDecl>.Empty;
                this.globalFuncDecls = ImmutableArray<R.FuncDecl>.Empty;
                this.callableMemberDecls = ImmutableArray<R.CallableMemberDecl>.Empty;
                this.topLevelStmts = ImmutableArray<R.Stmt>.Empty;
            }
            
            public RootContext(RootContext other, CloneContext cloneContext)
            {
                this.moduleName = other.moduleName;
                this.itemValueFactory = other.itemValueFactory;
                this.globalTypeDecls = other.globalTypeDecls;
                this.globalFuncDecls = other.globalFuncDecls;
                this.callableMemberDecls = other.callableMemberDecls;
                
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

                this.itemValueFactory = src.itemValueFactory;
                this.topLevelStmts = src.topLevelStmts;
            }

            public LocalVarInfo? GetLocalVarOutsideLambda(string varName) => null;
            public ITypeSymbol? GetReturn() => itemValueFactory.Int;            
            public void SetRetType(ITypeSymbol retTypeValue) => throw new UnreachableCodeException();
            public void AddLambdaCapture(string capturedVarName, ITypeSymbol capturedVarType) => throw new UnreachableCodeException();
            public bool IsSeqFunc() => false;

            public void AddTopLevelStmt(R.Stmt stmt)
            {
                topLevelStmts = topLevelStmts.Add(stmt);
            }

            public R.Script MakeScript()
            {
                return new R.Script(moduleName, globalTypeDecls, globalFuncDecls, callableMemberDecls, topLevelStmts);
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