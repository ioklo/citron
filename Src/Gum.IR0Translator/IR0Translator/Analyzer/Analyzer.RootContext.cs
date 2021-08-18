using Gum.Collections;
using Gum.Infra;
using System;
using R = Gum.IR0;
using M = Gum.CompileTime;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        // 최상위 레벨 컨텍스트
        class RootContext : ICallableContext, ITypeContainer
        {
            R.ModuleName moduleName;
            ItemValueFactory itemValueFactory;

            ImmutableArray<R.GlobalTypeDecl> globalTypeDecls;
            ImmutableArray<R.GlobalFuncDecl> globalFuncDecls;
            ImmutableArray<R.CallableMemberDecl> callableMemberDecls;
            ImmutableArray<R.Stmt> topLevelStmts;

            AnonymousIdComponent AnonymousIdComponent;

            public RootContext(R.ModuleName moduleName, ItemValueFactory itemValueFactory)
            {
                this.moduleName = moduleName;
                this.itemValueFactory = itemValueFactory;
                this.globalTypeDecls = ImmutableArray<R.GlobalTypeDecl>.Empty;
                this.globalFuncDecls = ImmutableArray<R.GlobalFuncDecl>.Empty;
                this.callableMemberDecls = ImmutableArray<R.CallableMemberDecl>.Empty;
                this.topLevelStmts = ImmutableArray<R.Stmt>.Empty;
            }
            
            public RootContext(RootContext other, CloneContext cloneContext)
            {
                this.moduleName = other.moduleName;
                Infra.Misc.EnsurePure(other.itemValueFactory);
                this.itemValueFactory = other.itemValueFactory;
                this.globalTypeDecls = other.globalTypeDecls;
                this.globalFuncDecls = other.globalFuncDecls;
                this.callableMemberDecls = other.callableMemberDecls;

                Infra.Misc.EnsurePure(other.topLevelStmts);
                this.topLevelStmts = other.topLevelStmts;
            }

            public R.Path.Normal GetPath()
            {
                return new R.Path.Root(moduleName);
            }

            public NormalTypeValue? GetThisTypeValue()
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

                Infra.Misc.EnsurePure(src.itemValueFactory);
                this.itemValueFactory = src.itemValueFactory;
                this.topLevelStmts = src.topLevelStmts;
            }

            public LocalVarInfo? GetLocalVarOutsideLambda(string varName) => null;
            public TypeValue? GetRetTypeValue() => itemValueFactory.Int;            
            public void SetRetTypeValue(TypeValue retTypeValue) => throw new UnreachableCodeException();
            public void AddLambdaCapture(string capturedVarName, TypeValue capturedVarType) => throw new UnreachableCodeException();
            public bool IsSeqFunc() => false;

            public void AddTopLevelStmt(R.Stmt stmt)
            {
                topLevelStmts = topLevelStmts.Add(stmt);
            }

            public R.Script MakeScript()
            {
                return new R.Script(moduleName, globalTypeDecls, globalFuncDecls, callableMemberDecls, topLevelStmts);
            }

            public void AddGlobalFuncDecl(R.GlobalFuncDecl globalFuncDecl)
            {
                throw new NotImplementedException();
            }

            public void AddGlobalTypeDecl(R.GlobalTypeDecl globalTypeDecl)
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
                globalTypeDecls.Add(new R.GlobalTypeDecl(typeDecl));
            }
        }
    }
}