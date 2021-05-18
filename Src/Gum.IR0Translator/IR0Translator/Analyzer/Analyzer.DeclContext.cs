using Gum.Collections;
using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R = Gum.IR0;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        abstract class DeclContext : IMutable<DeclContext>
        {
            ImmutableArray<R.Decl> decls;

            public DeclContext()
            {
                decls = ImmutableArray<R.Decl>.Empty;
            }

            public DeclContext(DeclContext other, CloneContext cloneContext)
            {
                Infra.Misc.EnsurePure(other.decls);
                this.decls = other.decls;
            }

            public abstract DeclContext Clone_DeclContext(CloneContext context);
            DeclContext IMutable<DeclContext>.Clone(CloneContext context)
                => Clone_DeclContext(context);

            void IMutable<DeclContext>.Update(DeclContext src, UpdateContext updateContext)
                => Update(src, updateContext);
            
            // 
            protected void Update(DeclContext src, UpdateContext context)
            {
                Infra.Misc.EnsurePure(src.decls);
                decls = src.decls;
                UpdateChild_DeclContext(src, context);
            }
            
            protected abstract void UpdateChild_DeclContext(DeclContext src, UpdateContext context);

            public void AddNormalFuncDecl(ImmutableArray<R.Decl> decls, R.Name name, bool bThisCall, ImmutableArray<string> typeParams, R.ParamInfo paramInfo, R.Stmt body)
            {
                decls.Add(new R.NormalFuncDecl(decls, name, bThisCall, typeParams, paramInfo, body));
            }

            public void AddSequenceFuncDecl(ImmutableArray<R.Decl> decls, R.Name name, R.Path yieldType, bool bThisCall, ImmutableArray<string> typeParams, R.ParamInfo paramInfo, R.Stmt body)
            {
                decls.Add(new R.SequenceFuncDecl(decls, name, bThisCall, yieldType, typeParams, paramInfo, body));
            }

            public void AddDecl(R.Decl decl)
            {
                decls = decls.Add(decl);
            }

            public ImmutableArray<R.Decl> GetDecls()
            {
                return decls;
            }

            public abstract R.Path.Normal GetPath();

            public R.Path.Nested GetPath(R.Name childName, R.ParamHash paramHash, ImmutableArray<R.Path> typeArgs)
            {
                var path = GetPath();
                return new R.Path.Nested(path, childName, paramHash, typeArgs);
            }
        }

        // NamespaceDeclContext

        // TypeDeclContext
    }
}
