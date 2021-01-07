using Gum.CompileTime;
using System.Collections.Generic;
using System.Linq;

namespace Gum.IR0
{
    partial class TypeExpEvaluator
    {
        // X<int>.Y<T>
        public abstract class TypeExpInfo
        {
            public abstract TypeValue GetTypeValue();
            public abstract TypeExpInfo? GetMemberInfo(string memberName, IEnumerable<TypeValue> typeArgs);

            // 스켈레톤으로 부터 얻는 타입
            public class Internal : TypeExpInfo
            {
                // X<int>.Y<T> 면 skeleton은 Y의 스켈레톤을 갖고 있어야 한다
                // typeValue는 [Internal]X<int>.Y<T>

                Skeleton.Type skeleton; // 멤버를 갖고 오기 위한 수단
                TypeValue.Normal typeValue;

                public Internal(Skeleton.Type skeleton, TypeValue.Normal typeValue) 
                {
                    this.skeleton = skeleton;
                    this.typeValue = typeValue;
                }

                public override TypeValue GetTypeValue()
                {
                    return typeValue;
                }

                public override TypeExpInfo? GetMemberInfo(string memberName, IEnumerable<TypeValue> typeArgsEnum)
                {
                    var typeArgs = typeArgsEnum.ToArray();
                    var memberSkeleton = skeleton.GetMember(memberName, typeArgs.Length) as Skeleton.Type;

                    if (memberSkeleton == null)
                        return null;

                    return new Internal(memberSkeleton, new TypeValue.Normal(
                        typeValue.ModuleName,
                        typeValue.NamespacePath,
                        typeValue.GetAllEntries(),
                        new AppliedItemPathEntry(memberName, string.Empty, typeArgs)));
                }
            }

            // 임시
            public class NoMember : TypeExpInfo
            {
                TypeValue typeValue;

                public NoMember(TypeValue typeValue) 
                {
                    this.typeValue = typeValue;
                }
                public override TypeValue GetTypeValue()
                {
                    return typeValue;
                }
                public override TypeExpInfo? GetMemberInfo(string memberName, IEnumerable<TypeValue> typeArgsEnum)
                {
                    return null;
                }
            }

            public class External : TypeExpInfo
            {
                TypeInfo typeInfo;
                TypeValue.Normal typeValue; 
                public External(TypeInfo typeInfo, TypeValue.Normal typeValue) 
                { 
                    this.typeInfo = typeInfo; 
                    this.typeValue = typeValue; 
                }

                public override TypeExpInfo? GetMemberInfo(string memberName, IEnumerable<TypeValue> typeArgsEnum)
                {
                    var typeArgs = typeArgsEnum.ToArray();
                    var entry = new ItemPathEntry(memberName, typeArgs.Length);
                    var memberTypeInfo = typeInfo.GetItem(entry) as TypeInfo;
                    if (memberTypeInfo == null)
                        return null;

                    return new External(memberTypeInfo, new TypeValue.Normal(
                        typeValue.ModuleName,
                        typeValue.NamespacePath,
                        typeValue.GetAllEntries(),
                        new AppliedItemPathEntry(entry.Name, entry.ParamHash, typeArgs)
                    ));
                }

                public override TypeValue GetTypeValue()
                {
                    return typeValue;
                }
            }
        }

    }
}
