using Pretune;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;

using Gum.Infra;

namespace Gum.CompileTime
{
    // ItemPath를 다시 만들어 봅시다. 거의 IR0와 비슷하게 나올겁니다
    public record ItemPath(ItemPath? Outer, Name Name, int TypeParamCount = 0, ParamTypes ParamTypes = default);

    public static class ItemPathExtensions
    {
        public static ItemPath Child(this ItemPath outer, string name, int typeParamCount = 0, ParamTypes paramTypes = default)
        {
            return new ItemPath(outer, new Name.Normal(name), typeParamCount, paramTypes);
        }

        public static ItemPath Child(this ItemPath outer, Name name, int typeParamCount = 0, ParamTypes paramTypes = default)
        {
            return new ItemPath(outer, name, typeParamCount, paramTypes);
        }

        public static TypeDeclId ToTypeDeclId(this TypeId type)
        {   
            switch (type)
            {
                case RootTypeId rootType:
                    return new RootTypeDeclId(rootType.Module, rootType.Namespace, new TypeName(rootType.Name, rootType.TypeArgs.Length));

                case MemberTypeId memberType:
                    var outerId = ToTypeDeclId(memberType.Outer);
                    return new MemberTypeDeclId(outerId, new TypeName(memberType.Name, memberType.TypeArgs.Length));

                case TypeVarTypeId:
                case VoidTypeId:
                default:
                    throw new UnreachableCodeException();
            }
        }
    }
}