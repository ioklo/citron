using Citron.Collections;
using System;
using Citron.Symbol;
using Citron.Syntax;
using Citron.Module;
using System.Diagnostics;
using Citron.Infra;
using System.Collections.Generic;

namespace Citron.Analysis
{
    public partial class TypeExpEvaluator
    {
        class LocalContext
        {
            LocalContext? outer;
            Skeleton skeleton; // 현재 Skeleton 위치

            public LocalContext(LocalContext? outer, Skeleton skeleton)
            {
                this.outer = outer;
                this.skeleton = skeleton;
            }            

            public LocalContext NewLocalContextWithFuncDecl(ISyntaxNode node)
            {
                var funcSkel = skeleton.GetFuncDeclMember(node);
                Debug.Assert(funcSkel != null);

                return new LocalContext(this, funcSkel);
            }

            public LocalContext NewLocalContext(Name.Normal name, int typeParamCount)
            {
                var memberSkel = skeleton.GetMember(name, typeParamCount, 0);
                Debug.Assert(memberSkel != null);

                return new LocalContext(this, memberSkel);
            }

            // 해당하는 이름의 
            public UniqueQueryResult<Skeleton> GetUniqueSkeletonMember(Name name, int typeParamCount)
            {
                return skeleton.GetUniqueMember(name, typeParamCount);
            }

            public IEnumerable<Skeleton> GetSkeletonMembers(Name name, int typeParamCount)
            {
                return skeleton.GetMembers(name, typeParamCount);
            }

            public LocalContext? GetOuter()
            {
                return outer;
            }

            // 지금 위치가 namespace인가
            public bool IsOnModuleOrNamespace()
            {
                return skeleton.Kind == SkeletonKind.Module || skeleton.Kind == SkeletonKind.Namespace;
            }

            public SymbolPath? GetNamespacePath()
            {
                Debug.Assert(skeleton.Kind == SkeletonKind.Module || skeleton.Kind == SkeletonKind.Namespace);
                Debug.Assert(skeleton.TypeParamCount == 0);

                if (skeleton.Kind == SkeletonKind.Module)
                    return null;

                Debug.Assert(outer != null);
                SymbolPath? outerPath = outer.GetNamespacePath();

                return new SymbolPath(outerPath, skeleton.Name);
            }

            
        }
    }
}
