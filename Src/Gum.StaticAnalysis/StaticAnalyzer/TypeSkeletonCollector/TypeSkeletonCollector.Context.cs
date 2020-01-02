using Gum.CompileTime;
using Gum.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gum.StaticAnalysis
{
    public partial class TypeSkeletonCollector
    {
        public class Context
        {
            private Dictionary<ISyntaxNode, ModuleItemId> typeIdsByNode { get; }
            private Dictionary<ISyntaxNode, ModuleItemId> funcIdsByNode { get; }
            private List<TypeSkeleton> typeSkeletons { get; }            

            private TypeSkeleton? scopeSkeleton { get; set; }

            public Context()
            {
                typeIdsByNode = new Dictionary<ISyntaxNode, ModuleItemId>();
                funcIdsByNode = new Dictionary<ISyntaxNode, ModuleItemId>();
                typeSkeletons = new List<TypeSkeleton>();
                scopeSkeleton = null;
            }
            
            internal void AddTypeSkeleton(ISyntaxNode node, string name, int typeParamCount, IEnumerable<string> enumElemNames)
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

            public void AddFuncId(ISyntaxNode node, ModuleItemId funcId)
            {
                funcIdsByNode.Add(node, funcId);
            }

            public ImmutableDictionary<ISyntaxNode, ModuleItemId> GetTypeIdsByNode()
            {
                return typeIdsByNode.ToImmutableDictionary();
            }

            public ImmutableDictionary<ISyntaxNode, ModuleItemId> GetFuncIdsByNode()
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
