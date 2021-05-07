using Gum.Collections;
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
        abstract class DeclContext
        {
            List<R.Decl> decls;

            public DeclContext()
            {
                decls = new List<R.Decl>();
            }

            public void AddNormalFuncDecl(ImmutableArray<R.Decl> decls, R.Name name, bool bThisCall, ImmutableArray<string> typeParams, ImmutableArray<R.ParamInfo> paramNames, R.Stmt body)
            {
                decls.Add(new R.NormalFuncDecl(decls, name, bThisCall, typeParams, paramNames, body));
            }

            public void AddSequenceFuncDecl(ImmutableArray<R.Decl> decls, R.Name name, R.Path yieldType, bool bThisCall, ImmutableArray<string> typeParams, ImmutableArray<R.ParamInfo> paramInfos, R.Stmt body)
            {
                decls.Add(new R.SequenceFuncDecl(decls, name, bThisCall, yieldType, typeParams, paramInfos, body));
            }

            public void AddDecl(R.Decl decl)
            {
                decls.Add(decl);
            }

            public ImmutableArray<R.Decl> GetDecls()
            {
                return decls.ToImmutableArray();
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
