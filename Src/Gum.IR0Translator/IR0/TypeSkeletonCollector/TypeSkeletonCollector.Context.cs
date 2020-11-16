using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using S = Gum.Syntax;

namespace Gum.IR0
{
    partial class TypeSkeletonCollector
    {
        public class Context
        {
            private Dictionary<S.ISyntaxNode, ItemPath> typePathsByNode { get; }
            private Dictionary<S.ISyntaxNode, ItemPath> funcPathsByNode { get; }
            private List<TypeSkeleton> typeSkeletons { get; }            

            private TypeSkeleton? curSkeleton { get; set; }

            public Context()
            {
                typePathsByNode = new Dictionary<S.ISyntaxNode, ItemPath>();
                funcPathsByNode = new Dictionary<S.ISyntaxNode, ItemPath>();
                typeSkeletons = new List<TypeSkeleton>();
                curSkeleton = null;
            }
            
            public TypeSkeleton AddTypeSkeleton(S.ISyntaxNode node, string name, int typeParamCount, IEnumerable<string> enumElemNames)
            {
                ItemPath typePath;
                
                if (curSkeleton != null)
                    typePath = curSkeleton.Path.Append(name, typeParamCount);
                else
                    typePath = new ItemPath(NamespacePath.Root, new ItemPathEntry(name, typeParamCount)); // TODO: NamespaceRoot가 아니라 namespace 선언 상황에 따라 달라진다

                typePathsByNode.Add(node, typePath);

                var typeSkeleton = new TypeSkeleton(typePath, enumElemNames);

                if (curSkeleton != null)
                    curSkeleton.AddMemberTypeSkeleton(name, typeParamCount, typeSkeleton);
                else
                    typeSkeletons.Add(typeSkeleton);

                return typeSkeleton;
            }

            public void ExecInNewTypeScope(TypeSkeleton typeSkeleton, Action action)
            {
                var prevSkeleton = curSkeleton;
                curSkeleton = typeSkeleton;

                try
                {
                    action.Invoke();
                }
                finally
                {
                    curSkeleton = prevSkeleton;
                }
            }

            public void AddFunc(S.ISyntaxNode node, Name name, int typeParamCount)
            {
                // NOTICE: paramHash는 추후에 계산이 된다
                if (curSkeleton == null)
                    funcPathsByNode.Add(node, new ItemPath(NamespacePath.Root, new ItemPathEntry(name, typeParamCount, paramHash: string.Empty)));
                else
                    funcPathsByNode.Add(node, curSkeleton.Path.Append(name, typeParamCount, paramHash: string.Empty));
            }

            public ImmutableDictionary<S.ISyntaxNode, ItemPath> GetTypePathsByNode()
            {
                return typePathsByNode.ToImmutableDictionary();
            }

            public ImmutableDictionary<S.ISyntaxNode, ItemPath> GetFuncPathsByNode()
            {
                return funcPathsByNode.ToImmutableDictionary();
            }

            public ImmutableArray<TypeSkeleton> GetTypeSkeletons()
            {
                return typeSkeletons.ToImmutableArray();
            }
        }
    }
}
