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
            private Dictionary<S.ISyntaxNode, ModuleItemId> typeIdsByNode { get; }
            private Dictionary<S.ISyntaxNode, ModuleItemId> funcIdsByNode { get; }
            private List<TypeSkeleton> typeSkeletons { get; }            

            private TypeSkeleton? scopeSkeleton { get; set; }

            public Context()
            {
                typeIdsByNode = new Dictionary<S.ISyntaxNode, ModuleItemId>();
                funcIdsByNode = new Dictionary<S.ISyntaxNode, ModuleItemId>();
                typeSkeletons = new List<TypeSkeleton>();
                scopeSkeleton = null;
            }
            
            internal void AddTypeSkeleton(S.ISyntaxNode node, string name, int typeParamCount, IEnumerable<string> enumElemNames)
            {
                ModuleItemId typeId;                
                
                if (scopeSkeleton != null)
                    typeId = scopeSkeleton.TypeId.Append(name, typeParamCount);
                else
                    typeId = ModuleItemId.Make(name, typeParamCount);

                typeIdsByNode.Add(node, typeId);
                typeSkeletons.Add(new TypeSkeleton(typeId, enumElemNames));

                if (scopeSkeleton != null)
                    scopeSkeleton.AddMemberTypeId(name, typeParamCount, typeId);
            }

            public void AddFuncId(S.ISyntaxNode node, ModuleItemId funcId)
            {
                funcIdsByNode.Add(node, funcId);
            }

            public ImmutableDictionary<S.ISyntaxNode, ModuleItemId> GetTypeIdsByNode()
            {
                return typeIdsByNode.ToImmutableDictionary();
            }

            public ImmutableDictionary<S.ISyntaxNode, ModuleItemId> GetFuncIdsByNode()
            {
                return funcIdsByNode.ToImmutableDictionary();
            }

            public ImmutableArray<TypeSkeleton> GetTypeSkeletons()
            {
                return typeSkeletons.ToImmutableArray();
            }
        }
    }
}
