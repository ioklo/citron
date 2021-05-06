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

            public void AddNormalFuncDecl(ImmutableArray<R.LambdaDecl> lambdaDecls, R.Name name, bool bThisCall, ImmutableArray<string> typeParams, ImmutableArray<R.ParamInfo> paramNames, R.Stmt body)
            {
                decls.Add(new R.NormalFuncDecl(lambdaDecls, name, bThisCall, typeParams, paramNames, body));
            }

            public void AddSequenceFuncDecl(ImmutableArray<R.LambdaDecl> lambdaDecls, R.Name name, R.Path yieldType, bool bThisCall, ImmutableArray<string> typeParams, ImmutableArray<R.ParamInfo> paramInfos, R.Stmt body)
            {
                decls.Add(new R.SequenceFuncDecl(lambdaDecls, name, bThisCall, yieldType, typeParams, paramInfos, body));
            }

            public void AddDecl(R.Decl decl)
            {
                decls.Add(decl);
            }

            public ImmutableArray<R.Decl> GetDecls()
            {
                return decls.ToImmutableArray();
            }
        }

        // NamespaceDeclContext

        // TypeDeclContext
    }
}
