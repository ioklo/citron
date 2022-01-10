using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using M = Gum.CompileTime;

namespace Gum.Analysis
{
    //public partial class TypeExpEvaluator
    //{
    //    abstract class TypeExpResult
    //    {
    //        public abstract TypeExpInfo GetTypeExpInfo();
    //        public abstract SymbolId? GetSymbolId();
    //        public abstract TypeExpResult? GetMemberInfo(string memberName, ImmutableArray<SymbolId> typeArgs);
    //    }
        
    //    class NoMemberTypeExpResult : TypeExpResult
    //    {
    //        public static NoMemberTypeExpResult Void { get; } = new NoMemberTypeExpResult(new MTypeTypeExpInfo(M.VoidTypeId.Instance, TypeExpInfoKind.Void, false));
    //        public static NoMemberTypeExpResult Var { get; } = new NoMemberTypeExpResult(VarTypeExpInfo.Instance);
    //        public static NoMemberTypeExpResult TypeVar(ModuleSymbolId typeVarTypeId)
    //            => new NoMemberTypeExpResult(new InternalTypeExpInfo(typeVarTypeId, typeVarType, TypeExpInfoKind.TypeVar, false));
    //        public static NoMemberTypeExpResult Nullable(M.NullableTypeId nullableType)
    //            => new NoMemberTypeExpResult(new MTypeTypeExpInfo(nullableType, TypeExpInfoKind.Nullable, false));

    //        TypeExpInfo typeExpInfo;

    //        public NoMemberTypeExpResult(TypeExpInfo typeExpInfo)
    //        {
    //            this.typeExpInfo = typeExpInfo;
    //        }

    //        public override TypeExpInfo GetTypeExpInfo()
    //        {
    //            return typeExpInfo;
    //        }

    //        public override SymbolId? GetSymbolId()
    //        {
    //            return typeExpInfo.GetSymbolId();
    //        }

    //        public override TypeExpResult? GetMemberInfo(string memberName, ImmutableArray<SymbolId> typeArgs)
    //        {
    //            return null;
    //        }
    //    }

    //    // TODO: 이름 변경 필요, NoMemberTypeExpResult도 MType을 반영할 때가 있으므로, NormalType.. 정도로 바꿔야 할 것 같다
    //    abstract class MTypeTypeExpResult : TypeExpResult
    //    {
    //        MTypeTypeExpInfo typeExpInfo;

    //        protected MTypeTypeExpResult(MTypeTypeExpInfo typeExpInfo)
    //        {
    //            this.typeExpInfo = typeExpInfo;
    //        }

    //        public sealed override TypeExpInfo GetTypeExpInfo()
    //        {
    //            return typeExpInfo;
    //        }

    //        public sealed override SymbolId? GetSymbolId()
    //        {
    //            return typeExpInfo.GetSymbolId();
    //        }

    //        public override TypeExpResult? GetMemberInfo(string memberName, ImmutableArray<SymbolId> typeArgs)
    //        {
    //            var typeId = typeExpInfo.GetSymbolId();
    //            Debug.Assert(typeId != null);

    //            var memberTypeId = typeId.Child(new M.Name.Normal(memberName), typeArgs);
    //            return GetMemberInfo(memberName, typeArgs.Length, memberTypeId);
    //        }

    //        protected abstract TypeExpResult? GetMemberInfo(string memberName, int typeParamCount, SymbolId memberTypeId);
    //    }
        
    //    class SymbolTypeExpResult : TypeExpResult
    //    {
    //        MTypeTypeExpInfo typeExpInfo;
    //        ITypeDeclSymbolNode typeDecl;

    //        public SymbolTypeExpResult(MTypeTypeExpInfo typeExpInfo, ITypeDeclSymbolNode typeDecl)
    //        {
    //            this.typeExpInfo = typeExpInfo;
    //            this.typeDecl = typeDecl;
    //        }

    //        public override TypeExpResult? GetMemberInfo(string memberName, ImmutableArray<SymbolId> typeArgs)
    //        {
    //            var typeId = typeExpInfo.GetSymbolId();
    //            Debug.Assert(typeId != null);
                
    //            var memberTypeNode = typeDecl.GetMemberDeclNode(new M.Name.Normal(memberName), typeArgs.Length, default) as ITypeDeclSymbolNode;
    //            if (memberTypeNode == null)
    //                return null;
                
    //            var memberTypeId = typeId.Child(new M.Name.Normal(memberName), typeArgs);
    //            var memberKind = GetTypeExpInfoKind(memberTypeNode);
    //            var mmemberTypeExpInfo = new MTypeTypeExpInfo(memberTypeId, memberKind, false);
    //            return new SymbolTypeExpResult(mmemberTypeExpInfo, memberTypeNode);
    //        }
    //    }

    //    class InternalTypeExpResult : TypeExpResult
    //    {
    //        MTypeTypeExpInfo typeExpInfo;

    //        // X<int>.Y<T> 면 skeleton은 Y의 스켈레톤을 갖고 있어야 한다
    //        // typeValue는 [Internal]X<int>.Y<T>
    //        TypeSkeleton skeleton; // 멤버를 갖고 오기 위한 수단

    //        public InternalTypeExpResult(TypeSkeleton skeleton, MTypeTypeExpInfo typeExpInfo)
    //        {
    //            this.typeExpInfo = typeExpInfo;
    //            this.skeleton = skeleton;
    //        }

    //        public override TypeExpResult? GetMemberInfo(string memberName, ImmutableArray<SymbolId> typeArgs)
    //        {
    //            var memberSkeleton = skeleton.GetMember(new M.Name.Normal(memberName), typeArgs.Length);
    //            if (memberSkeleton == null) return null;

    //            var memberKind = GetTypeExpInfoKind(skeleton.Kind);

    //            var typeId = typeExpInfo.GetSymbolId();
    //            Debug.Assert(typeId != null);
    //            var memberTypeId = typeId.Child(new M.Name.Normal(memberName), typeArgs);

    //            var mmemberTypeExpInfo = new MTypeTypeExpInfo(memberTypeId, memberKind, true);

    //            return new InternalTypeExpResult(memberSkeleton, mmemberTypeExpInfo);
    //        }
    //    }
    //}
}
