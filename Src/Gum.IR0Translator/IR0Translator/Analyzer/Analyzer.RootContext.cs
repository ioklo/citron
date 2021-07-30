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
        class RootContext : ICallableContext
        {
            R.ModuleName moduleName;
            ItemValueFactory itemValueFactory;

            ImmutableArray<R.Decl> decls;
            ImmutableArray<R.Stmt> topLevelStmts;

            AnonymousIdComponent AnonymousIdComponent;

            public RootContext(R.ModuleName moduleName, ItemValueFactory itemValueFactory)
            {
                this.moduleName = moduleName;
                this.itemValueFactory = itemValueFactory;
                this.decls = ImmutableArray<R.Decl>.Empty;
                this.topLevelStmts = ImmutableArray<R.Stmt>.Empty;
            }
            
            public RootContext(RootContext other, CloneContext cloneContext)
            {
                this.moduleName = other.moduleName;
                Infra.Misc.EnsurePure(other.itemValueFactory);
                this.itemValueFactory = other.itemValueFactory;

                Infra.Misc.EnsurePure(other.decls);
                this.decls = other.decls;

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
                return new R.Script(moduleName, decls, topLevelStmts);
            }

            public void AddDecl(R.Decl funcDecl)
            {
                decls = decls.Add(funcDecl);
            }

            public ImmutableArray<R.Decl> GetDecls() => decls;

            public R.Name.Anonymous NewAnonymousName() => AnonymousIdComponent.NewAnonymousName();

            public RootItemValueOuter MakeRootItemValueOuter(M.NamespacePath namespacePath)
            {
                return new RootItemValueOuter(moduleName.Value, namespacePath);
            }
        }
    }
}