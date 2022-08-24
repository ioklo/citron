using Citron.Collections;
using System;
using Citron.Symbol;
using Citron.Syntax;
using Citron.Module;
using System.Diagnostics;

namespace Citron.Analysis
{
    public partial class TypeExpEvaluator
    {
        class LocalContext
        {
            Skeleton skeleton; // 현재 Skeleton 위치

            public LocalContext(Skeleton skeleton)
            {
                this.skeleton = skeleton;
            }            

            public LocalContext NewLocalContextWithFuncDecl(ISyntaxNode node)
            {
                var funcSkel = skeleton.GetFuncDeclMember(node);
                Debug.Assert(funcSkel != null);

                return new LocalContext(funcSkel);
            }

            public LocalContext NewLocalContext(Name.Normal name, int typeParamCount)
            {
                var memberSkel = skeleton.GetMember(name, typeParamCount, 0);
                Debug.Assert(memberSkel != null);

                return new LocalContext(memberSkel);
            }
        }
    }
}
