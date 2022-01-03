using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using M = Gum.CompileTime;
using Gum.Analysis;

namespace Gum.IR0Translator
{
    public partial class TypeExpEvaluator
    {
        abstract class TypeExpResult
        {
            public abstract TypeExpInfo GetTypeExpInfo();
            public abstract M.TypeId? GetMType();
            public abstract TypeExpResult? GetMemberInfo(string memberName, ImmutableArray<M.TypeId> typeArgs);
        }
        
        class NoMemberTypeExpResult : TypeExpResult
        {
            public static NoMemberTypeExpResult Void { get; } = new NoMemberTypeExpResult(new MTypeTypeExpInfo(M.VoidTypeId.Instance, TypeExpInfoKind.Void, false));
            public static NoMemberTypeExpResult Var { get; } = new NoMemberTypeExpResult(VarTypeExpInfo.Instance);
            public static NoMemberTypeExpResult TypeVar(M.TypeVarTypeId typeVarType)
                => new NoMemberTypeExpResult(new MTypeTypeExpInfo(typeVarType, TypeExpInfoKind.TypeVar, false));
            public static NoMemberTypeExpResult Nullable(M.NullableTypeId nullableType)
                => new NoMemberTypeExpResult(new MTypeTypeExpInfo(nullableType, TypeExpInfoKind.Nullable, false));

            TypeExpInfo typeExpInfo;            

            public NoMemberTypeExpResult(TypeExpInfo typeExpInfo)
            {
                this.typeExpInfo = typeExpInfo;
            }

            public override TypeExpInfo GetTypeExpInfo()
            {
                return typeExpInfo;
            }

            public override M.TypeId? GetMType()
            {
                return typeExpInfo.GetMType();
            }

            public override TypeExpResult? GetMemberInfo(string memberName, ImmutableArray<M.TypeId> typeArgs)
            {
                return null;
            }
        }

        // TODO: 이름 변경 필요, NoMemberTypeExpResult도 MType을 반영할 때가 있으므로, NormalType.. 정도로 바꿔야 할 것 같다
        abstract class MTypeTypeExpResult : TypeExpResult
        {
            MTypeTypeExpInfo typeExpInfo;

            protected MTypeTypeExpResult(MTypeTypeExpInfo typeExpInfo)
            {
                this.typeExpInfo = typeExpInfo;
            }

            public sealed override TypeExpInfo GetTypeExpInfo()
            {
                return typeExpInfo;
            }

            public sealed override M.TypeId? GetMType()
            {
                return typeExpInfo.GetMType();
            }

            public sealed override TypeExpResult? GetMemberInfo(string memberName, ImmutableArray<M.TypeId> typeArgs)
            {
                Debug.Assert(typeExpInfo.Type is M.NormalTypeId);

                var mmemberType = new M.MemberType((M.NormalTypeId)typeExpInfo.Type, new M.Name.Normal(memberName), typeArgs);
                return GetMemberInfo(memberName, typeArgs.Length, mmemberType);
            }

            protected abstract TypeExpResult? GetMemberInfo(string memberName, int typeParamCount, M.MemberType mmemberType);
        }

        class ExternalTypeExpResult : MTypeTypeExpResult
        {
            IModuleTypeDecl typeDecl;

            public ExternalTypeExpResult(MTypeTypeExpInfo typeExpInfo, IModuleTypeDecl typeDecl)
                : base(typeExpInfo)
            {
                this.typeDecl = typeDecl;
            }

            protected override TypeExpResult? GetMemberInfo(string memberName, int typeParamCount, M.MemberType mmemberType)
            {
                var memberType = typeDecl.GetMemberType(new M.TypeName(new M.Name.Normal(memberName), typeParamCount));
                if (memberType == null)
                    return null;

                var memberKind = GetTypeExpInfoKind(memberType);
                var mmemberTypeExpInfo = new MTypeTypeExpInfo(mmemberType, memberKind, false);
                return new ExternalTypeExpResult(mmemberTypeExpInfo, memberType);
            }
        }

        class InternalTypeExpResult : MTypeTypeExpResult
        {
            // X<int>.Y<T> 면 skeleton은 Y의 스켈레톤을 갖고 있어야 한다
            // typeValue는 [Internal]X<int>.Y<T>
            TypeSkeleton skeleton; // 멤버를 갖고 오기 위한 수단

            public InternalTypeExpResult(TypeSkeleton skeleton, MTypeTypeExpInfo typeExpInfo)
                : base(typeExpInfo)
            {
                this.skeleton = skeleton;
            }

            protected override TypeExpResult? GetMemberInfo(string memberName, int typeParamCount, M.MemberType mmemberType)
            {
                var memberSkeleton = skeleton.GetMember(new M.Name.Normal(memberName), typeParamCount);
                if (memberSkeleton == null) return null;

                var memberKind = GetTypeExpInfoKind(skeleton.Kind);
                var mmemberTypeExpInfo = new MTypeTypeExpInfo(mmemberType, memberKind, true);

                return new InternalTypeExpResult(memberSkeleton, mmemberTypeExpInfo);
            }
        }
    }
}
